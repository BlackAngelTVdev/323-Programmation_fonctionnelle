// EsportApp — analyse des performances de Team Helvetia (Valorant, CS2, LoL).
//
// Interface en ligne de commande construite à la main : aucune librairie
// externe, juste des comparaisons sur le tableau `args`.
// Flags reconnus à ce stade du fil rouge (étape 5.1) :
//   --help --version --game --player --filter --stat --normalize --smooth
//   --generate --error --extract
// --mme est accepté mais n'a aucun effet seul : seul --extract mme est reconnu.

using DataSeries;
using ESportApp;

const string version = "0.5";

string[] knownFlags =
{
    "--help", "--version", "--game", "--player", "--filter", "--stat",
    "--normalize", "--smooth", "--generate", "--error", "--extract", "--mme"
};

// Les flags qui attendent une valeur juste après eux
string[] valueFlags = { "--game", "--player", "--filter", "--stat", "--smooth", "--generate", "--error", "--extract" };

Console.WriteLine($"EsportApp v{version}");

// ─── Flags sans valeur ───────────────────────────────────────────────────────

if (args.Length == 0 || args.Contains("--help"))
{
    ShowHelp();
    return;
}

// ─── Validation de la ligne de commande ──────────────────────────────────────

string? unknownFlag = args.FirstOrDefault(a => a.StartsWith("--") && !knownFlags.Contains(a));
if (unknownFlag != null)
{
    Console.WriteLine($"Flag inconnu : {unknownFlag}");
    ShowHelp();
    return;
}

string? flagSansValeur = valueFlags.FirstOrDefault(f => args.Contains(f) && ValueOf(f) == null);
if (flagSansValeur != null)
{
    Console.WriteLine($"Le flag {flagSansValeur} attend une valeur.");
    ShowHelp();
    return;
}

// ─── Lecture des valeurs ─────────────────────────────────────────────────────

string? game = ValueOf("--game");
string? player = ValueOf("--player");
string filterMode = ValueOf("--filter") ?? "all";
string statName = ValueOf("--stat") ?? "kda";
string errorMode = ValueOf("--error") ?? "soft";

// --normalize n'attend pas de valeur : sa seule présence suffit
bool normalize = args.Contains("--normalize");

// --extract min|max|avg|mme : extraire un seul indicateur de la série.
// Seul --extract mme est reconnu ; --mme seul n'a aucun effet.
string? extractMode = ValueOf("--extract");

// --smooth attend la taille de la fenêtre ; 0 signifie « pas de lissage »
int smoothWindow = 0;
if (args.Contains("--smooth") && !int.TryParse(ValueOf("--smooth"), out smoothWindow))
    smoothWindow = -1;

// Tables de fonctions : le flag CLI choisit une fonction, pas un if/else.
// Ajouter un critère = ajouter une ligne dans la table.
Dictionary<string, Func<IMatch, bool>> filters = new Dictionary<string, Func<IMatch, bool>>
{
    ["wins"] = m => m.Won,
    ["losses"] = m => !m.Won,
    ["all"] = m => true,
};

Dictionary<string, Func<IMatch, double>> selectors = new Dictionary<string, Func<IMatch, double>>
{
    ["kda"] = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
    ["kills"] = m => m.Kills,
    ["assists"] = m => m.Assists,
};

string[] games = { "valorant", "cs2", "lol" };
string[] errorModes = { "strict", "soft", "hard" };
string[] extractModes = { "min", "max", "avg", "mme" };

if (game != null && !games.Contains(game))
{
    Console.WriteLine($"Jeu inconnu : {game} (attendu : {string.Join(", ", games)})");
    return;
}

if (!filters.ContainsKey(filterMode))
{
    Console.WriteLine($"Filtre inconnu : {filterMode} (attendu : {string.Join(", ", filters.Keys)})");
    return;
}

if (!selectors.ContainsKey(statName))
{
    Console.WriteLine($"Stat inconnue : {statName} (attendu : {string.Join(", ", selectors.Keys)})");
    return;
}

if (!errorModes.Contains(errorMode))
{
    Console.WriteLine($"Mode d'erreur inconnu : {errorMode} (attendu : {string.Join(", ", errorModes)})");
    return;
}

if (extractMode != null && !extractModes.Contains(extractMode))
{
    Console.WriteLine($"Indicateur à extraire inconnu : {extractMode} (attendu : {string.Join(", ", extractModes)})");
    return;
}

if (smoothWindow < 0)
{
    Console.WriteLine($"Fenêtre de lissage invalide : {ValueOf("--smooth")} (attendu : un entier >= 1)");
    return;
}

if (args.Contains("--smooth") && smoothWindow < 1)
{
    Console.WriteLine("Fenêtre de lissage invalide : la taille minimale est 1.");
    return;
}

// ─── --generate : simuler les matchs manquants, puis quitter ─────────────────

// Un seul prédicat de validité, réutilisé pour tous les joueurs : une fonction
// est une valeur comme une autre.
Func<Cs2Match, bool> cs2Valide = m => m.Kills + m.Assists <= 50 && m.Deaths >= 1;
Func<ValorantMatch, bool> valorantValide = m => m.Kills + m.Assists <= 50 && m.Deaths >= 1;
Func<LolMatch, bool> lolValide = m => m.Deaths >= 1 && m.Cs >= 0;

if (args.Contains("--generate"))
{
    string cible = ValueOf("--generate")!;
    string[] joueurs = cible == "all"
        ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
        : new[] { cible };

    foreach (string joueur in joueurs)
        Generate(joueur);

    return;
}

// ─── Chargement des données ──────────────────────────────────────────────────

DataSerie<DataPoint<ValorantMatch>> valorant =
    DataSerie<DataPoint<ValorantMatch>>.FromCsv(@"data/valorant.csv", ParseValorant);
DataSerie<DataPoint<Cs2Match>> cs2 =
    DataSerie<DataPoint<Cs2Match>>.FromCsv(@"data/cs2.csv", ParseCS2);
DataSerie<DataPoint<LolMatch>> lol =
    DataSerie<DataPoint<LolMatch>>.FromCsv(@"data/lol.csv", ParseLoL);

// ─── Analyse ─────────────────────────────────────────────────────────────────

// Prédicats d'aberration (exercice 03) — un par jeu, car les contraintes
// métier ne sont pas les mêmes.
Func<ValorantMatch, bool> valorantAberrant = m =>
    m.Kills < 0 || m.Kills > 50 ||
    m.Deaths < 0 || m.Deaths > 30 ||
    m.Assists < 0;

Func<Cs2Match, bool> cs2Aberrant = m =>
    m.Kills + m.Assists > 50 ||
    m.Deaths < 0;

Func<LolMatch, bool> lolAberrant = m =>
    m.Kills > 10 ||
    m.Deaths < 1 ||
    m.Assists < 0 ||
    m.Cs < 0;

Console.WriteLine();

if (game == null || game == "valorant")
    if (!Report("Valorant", valorant, dp => valorantAberrant(dp.Value), ExportValorant)) return;

if (game == null || game == "cs2")
    if (!Report("CS2", cs2, dp => cs2Aberrant(dp.Value), ExportCs2)) return;

if (game == null || game == "lol")
    if (!Report("LoL", lol, dp => lolAberrant(dp.Value), ExportLol)) return;

// ─── Fonctions ───────────────────────────────────────────────────────────────

void ShowHelp()
{
    Console.WriteLine("Usage: EsportApp [options]");
    Console.WriteLine();
    Console.WriteLine("  Analyse des performances de Team Helvetia (Valorant, CS2, LoL).");
    Console.WriteLine();
    Console.WriteLine("Sélection des données");
    Console.WriteLine("  --game   valorant|cs2|lol    Jeu à analyser              (défaut : les trois)");
    Console.WriteLine("  --player <nom>               Restreindre à un joueur     (défaut : tous)");
    Console.WriteLine("  --filter wins|losses|all     Issue des matchs retenus    (défaut : all)");
    Console.WriteLine();
    Console.WriteLine("Analyse");
    Console.WriteLine("  --stat   kda|kills|assists   Indicateur calculé/affiché  (défaut : kda)");
    Console.WriteLine("  --normalize                  Ramène l'indicateur dans [0.0, 1.0]");
    Console.WriteLine("  --smooth <n>                 Moyenne glissante sur n valeurs");
    Console.WriteLine("                                 (normalisation puis lissage, dans cet ordre)");
    Console.WriteLine("  --extract min|max|avg|mme    Extraire un indicateur (avec le détail des matchs)");
    Console.WriteLine();
    Console.WriteLine("Données");
    Console.WriteLine("  --generate <joueur|all>      Simule et exporte les matchs manquants, puis quitte");
    Console.WriteLine("  --error  strict|soft|hard    Traitement des valeurs aberrantes (défaut : soft)");
    Console.WriteLine("                                 strict : les affiche et s'arrête");
    Console.WriteLine("                                 soft   : les élimine et continue");
    Console.WriteLine("                                 hard   : les élimine, sauve le CSV nettoyé, continue");
    Console.WriteLine();
    Console.WriteLine("Divers");
    Console.WriteLine("  --help                       Affiche cette aide");
    Console.WriteLine("  --version                    Affiche la version");
}

// Retourne la valeur qui suit un flag, ou null si le flag est absent ou si
// aucune valeur ne le suit.
string? ValueOf(string flag)
{
    int i = Array.IndexOf(args, flag);
    if (i < 0 || i + 1 >= args.Length || args[i + 1].StartsWith("--"))
        return null;
    return args[i + 1];
}

// Analyse une série : écarte les aberrations selon --error, applique --player
// et --filter, puis affiche l'indicateur choisi par --stat.
// Retourne false quand --error strict impose l'arrêt du programme.
bool Report<T>(string label,
               DataSerie<DataPoint<T>> serie,
               Func<DataPoint<T>, bool> estAberrant,
               Action<DataSerie<DataPoint<T>>, string> export) where T : IMatch
{
    DataSerie<DataPoint<T>> aberrants = serie.Outliers(estAberrant);

    if (errorMode == "strict" && aberrants.Count > 0)
    {
        Console.WriteLine($"{label} : {aberrants.Count} valeur(s) aberrante(s) — arrêt (--error strict)");
        foreach (DataPoint<T> point in aberrants.Values)
            Console.WriteLine($"  {point.Timestamp:yyyy-MM-dd}  {point.Value}");
        return false;
    }

    DataSerie<DataPoint<T>> propre = serie.Sanitize(estAberrant);

    if (errorMode == "hard")
    {
        string fichier = $"{label.ToLower()}_clean.csv";
        export(propre, fichier);
        Console.WriteLine($"{label} : série nettoyée sauvée dans {fichier}");
    }

    // --player et --filter s'enchaînent : chaque Filter retourne une nouvelle
    // série, donc la composition est naturelle.
    DataSerie<DataPoint<T>> retenus = propre
        .Filter(dp => player == null || dp.Value.Player == player)
        .Filter(dp => filters[filterMode](dp.Value));

    Console.WriteLine($"{label} : {serie.Count} matchs, "
                    + $"{aberrants.Count} écarté(s), {retenus.Count} retenu(s)");

    // La série de matchs devient une série de nombres.
    // Normalize fait le même travail que Transform, en ramenant en plus le
    // résultat dans [0.0, 1.0] : inutile d'enchaîner les deux.
    Func<DataPoint<T>, double> selecteur = dp => selectors[statName](dp.Value);

    if (extractMode != null)
    {
        if (retenus.Count == 0)
        {
            Console.WriteLine("  aucun match retenu — rien à extraire");
            Console.WriteLine();
            return true;
        }

        foreach (DataPoint<T> point in retenus.Values)
            Console.WriteLine($"  {point.Timestamp:yyyy-MM-dd}  {point.Value.Player,-8}  {statName} = {selecteur(point):F2}");

        double resultat = extractMode switch
        {
            "min"     => retenus.Values.Select(selecteur).Min(),
            "max"     => retenus.Values.Select(selecteur).Max(),
            "avg" => retenus.Values.Select(selecteur).Average(),
            "mme"     => retenus.MME(selecteur),
            _         => throw new InvalidOperationException($"Indicateur inconnu : {extractMode}")
        };

        string libelle = extractMode switch
        {
            "min"     => "Min",
            "max"     => "Max",
            "avg" => "Moyenne",
            "mme"     => "MME",
            _         => extractMode
        };
        string suffixe = extractMode == "mme" ? " - forme du moment" : "";

        Console.WriteLine($"  {libelle} ({statName}){suffixe} : {resultat:F2}");
        Console.WriteLine();
        return true;
    }

    DataSerie<double> valeurs = normalize
        ? retenus.Normalize(selecteur)
        : retenus.Transform(selecteur);

    if (smoothWindow > 0)
        // La série est déjà numérique : l'évaluateur est l'identité.
        valeurs = valeurs.Smooth(v => v, smoothWindow);

    if (smoothWindow > retenus.Count)
    {
        Console.WriteLine($"  fenêtre de lissage ({smoothWindow}) plus large que la série ({retenus.Count}) — rien à afficher");
        Console.WriteLine();
        return true;
    }

    // Une moyenne glissante est datée par le DERNIER match de sa fenêtre :
    // les windowSize-1 premiers matchs n'ouvrent aucune fenêtre complète.
    int decalage = smoothWindow > 0 ? smoothWindow - 1 : 0;
    string etiquette = statName
                     + (normalize ? " normalisé" : "")
                     + (smoothWindow > 0 ? $" lissé({smoothWindow})" : "");

    foreach ((DataPoint<T> point, double valeur) in retenus.Values.Skip(decalage).Zip(valeurs.Values))
        Console.WriteLine($"  {point.Timestamp:yyyy-MM-dd}  {point.Value.Player,-8}  {etiquette} = {valeur:F2}");

    Console.WriteLine();
    return true;
}

// Simule puis exporte les matchs de pré-saison d'une recrue.
void Generate(string joueur)
{
    string fichier = $"{joueur.ToLower()}_generated.csv";

    switch (joueur)
    {
        case "Raphaël":
        case "Kiara":
            DataSerie<DataPoint<Cs2Match>> matchsCs2 =
                MatchGenerator.GenerateCs2(joueur, 20, joueur == "Kiara" ? 7 : 42)
                              .Filter(dp => cs2Valide(dp.Value));
            ExportCs2(matchsCs2, fichier);
            Console.WriteLine($"{joueur} : {matchsCs2.Count} matchs CS2 générés → {fichier}");
            break;

        case "Dylan":
            DataSerie<DataPoint<ValorantMatch>> matchsValorant =
                MatchGenerator.GenerateValorant(joueur, 20, 11)
                              .Filter(dp => valorantValide(dp.Value));
            ExportValorant(matchsValorant, fichier);
            Console.WriteLine($"{joueur} : {matchsValorant.Count} matchs Valorant générés → {fichier}");
            break;

        case "Noé":
            DataSerie<DataPoint<LolMatch>> matchsLol =
                MatchGenerator.GenerateLol(joueur, 20, 3)
                              .Filter(dp => lolValide(dp.Value));
            ExportLol(matchsLol, fichier);
            Console.WriteLine($"{joueur} : {matchsLol.Count} matchs LoL générés → {fichier}");
            break;

        default:
            Console.WriteLine($"Joueur inconnu : {joueur} "
                            + "(attendu : Raphaël, Kiara, Dylan, Noé ou all)");
            break;
    }
}

// ─── Parsers : le domaine est ici, la bibliothèque l'ignore ──────────────────

DataPoint<ValorantMatch> ParseValorant(string[] cols)
{
    ValorantMatch match = new ValorantMatch(cols[1], cols[2], int.Parse(cols[3]), int.Parse(cols[4]),
        int.Parse(cols[5]), int.Parse(cols[6]), int.Parse(cols[7]), bool.Parse(cols[8]));
    DateTime date = DateTime.Parse(cols[0]);
    return new DataPoint<ValorantMatch>(date, match);
}

DataPoint<Cs2Match> ParseCS2(string[] cols)
{
    return new DataPoint<Cs2Match>(DateTime.Parse(cols[0]),
        new Cs2Match(cols[1], cols[2], cols[3], int.Parse(cols[4]), int.Parse(cols[5]),
            int.Parse(cols[6]), int.Parse(cols[7]), bool.Parse(cols[8])));
}

DataPoint<LolMatch> ParseLoL(string[] cols)
{
    return new DataPoint<LolMatch>(DateTime.Parse(cols[0]),
        new LolMatch(cols[1], cols[2], int.Parse(cols[4]), int.Parse(cols[5]),
            int.Parse(cols[6]), int.Parse(cols[7]), int.Parse(cols[8]), bool.Parse(cols[9])));
}

// ─── Exports : le CSV produit doit rester lisible par FromCsv ────────────────

void ExportValorant(DataSerie<DataPoint<ValorantMatch>> matchs, string chemin)
{
    string entete = "date,player,agent,kills,deaths,assists,headshots,rounds_won,won";
    IEnumerable<string> lignes = matchs.Values.Select(dp =>
        $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Agent},{dp.Value.Kills}," +
        $"{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Headshots},{dp.Value.RoundsWon}," +
        $"{dp.Value.Won.ToString().ToLower()}");
    File.WriteAllLines(chemin, lignes.Prepend(entete));
}

void ExportCs2(DataSerie<DataPoint<Cs2Match>> matchs, string chemin)
{
    string entete = "date,player,map,start_side,kills,deaths,assists,mvps,won";
    IEnumerable<string> lignes = matchs.Values.Select(dp =>
        $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide}," +
        $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps}," +
        $"{dp.Value.Won.ToString().ToLower()}");
    File.WriteAllLines(chemin, lignes.Prepend(entete));
}

void ExportLol(DataSerie<DataPoint<LolMatch>> matchs, string chemin)
{
    // Le CSV LoL a une colonne `role` que LolMatch ne stocke pas : le seul
    // joueur LoL du roster est Support, on la réécrit telle quelle.
    string entete = "date,player,champion,role,kills,deaths,assists,cs,vision_score,won";
    IEnumerable<string> lignes = matchs.Values.Select(dp =>
        $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Champion},Support," +
        $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Cs}," +
        $"{dp.Value.VisionScore},{dp.Value.Won.ToString().ToLower()}");
    File.WriteAllLines(chemin, lignes.Prepend(entete));
}
