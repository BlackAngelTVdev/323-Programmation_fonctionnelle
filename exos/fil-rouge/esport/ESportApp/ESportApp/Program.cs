// EsportApp — analyse des performances de Team Helvetia (Valorant, CS2, LoL).
//
// Interface en ligne de commande construite à la main : aucune librairie
// externe, juste des comparaisons sur le tableau `args`.
//
// Chaque jeu a son propre modèle de match — qui porte sa date — sans type
// commun entre eux. Ce qui se partageait autrefois par une interface se passe
// désormais en paramètre, sous forme de fonctions.
// Flags reconnus à ce stade du fil rouge (étape 5.3) :
//   --help --version --game --player --filter --stat --normalize --smooth
//   --generate --error --extract --hasCrushed --hasBeenCrushed --hasBeenGod
//   --progress
// --mme est accepté mais n'a aucun effet seul : seul --extract mme est reconnu.

using DataSeries;
using ESportApp;

const string version = "0.5";

string[] knownFlags =
{
    "--help", "--version", "--game", "--player", "--filter", "--stat",
    "--normalize", "--smooth", "--generate", "--error", "--extract", "--mme",
    "--hasCrushed", "--hasBeenCrushed", "--hasBeenGod", "--progress"
};

// Les flags qui attendent une valeur juste après eux
string[] valueFlags =
{
    "--game", "--player", "--filter", "--stat", "--smooth", "--generate", "--error", "--extract",
    "--hasCrushed", "--hasBeenCrushed", "--hasBeenGod"
};

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

// --hasCrushed/--hasBeenCrushed/--hasBeenGod comparent le KDA de chaque
// match retenu à un seuil ; l'app répond Yes ou No.
string? hasCrushedRaw = ValueOf("--hasCrushed");
string? hasBeenCrushedRaw = ValueOf("--hasBeenCrushed");
string? hasBeenGodRaw = ValueOf("--hasBeenGod");
bool seuilDemande = hasCrushedRaw != null || hasBeenCrushedRaw != null || hasBeenGodRaw != null;

// --progress : KDA moyen par mois pour un joueur d'un jeu donné.
bool progressDemande = args.Contains("--progress");

// --smooth attend la taille de la fenêtre ; 0 signifie « pas de lissage »
int smoothWindow = 0;
if (args.Contains("--smooth") && !int.TryParse(ValueOf("--smooth"), out smoothWindow))
    smoothWindow = -1;

// Tables de fonctions : le flag CLI choisit une fonction, pas un if/else.
// Ajouter un critère = ajouter une ligne dans la table.
// Les trois jeux n'ont aucun type commun : chacun a donc ses propres tables.
// Les formules se ressemblent — c'est le prix de modèles indépendants.
Dictionary<string, Func<ValorantMatch, bool>> valorantFilters = new Dictionary<string, Func<ValorantMatch, bool>>
{
    ["wins"] = m => m.Won,
    ["losses"] = m => !m.Won,
    ["all"] = m => true,
};

Dictionary<string, Func<Cs2Match, bool>> cs2Filters = new Dictionary<string, Func<Cs2Match, bool>>
{
    ["wins"] = m => m.Won,
    ["losses"] = m => !m.Won,
    ["all"] = m => true,
};

Dictionary<string, Func<LolMatch, bool>> lolFilters = new Dictionary<string, Func<LolMatch, bool>>
{
    ["wins"] = m => m.Won,
    ["losses"] = m => !m.Won,
    ["all"] = m => true,
};

Dictionary<string, Func<ValorantMatch, double>> valorantSelectors = new Dictionary<string, Func<ValorantMatch, double>>
{
    ["kda"] = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
    ["kills"] = m => m.Kills,
    ["assists"] = m => m.Assists,
};

Dictionary<string, Func<Cs2Match, double>> cs2Selectors = new Dictionary<string, Func<Cs2Match, double>>
{
    ["kda"] = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
    ["kills"] = m => m.Kills,
    ["assists"] = m => m.Assists,
};

Dictionary<string, Func<LolMatch, double>> lolSelectors = new Dictionary<string, Func<LolMatch, double>>
{
    ["kda"] = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
    ["kills"] = m => m.Kills,
    ["assists"] = m => m.Assists,
};

string[] games = { "valorant", "cs2", "lol" };
string[] errorModes = { "strict", "soft", "hard" };
string[] extractModes = { "min", "max", "avg", "mme" };
string[] filterModes = { "wins", "losses", "all" };
string[] statNames = { "kda", "kills", "assists" };

if (game != null && !games.Contains(game))
{
    Console.WriteLine($"Jeu inconnu : {game} (attendu : {string.Join(", ", games)})");
    return;
}

if (!filterModes.Contains(filterMode))
{
    Console.WriteLine($"Filtre inconnu : {filterMode} (attendu : {string.Join(", ", filterModes)})");
    return;
}

if (!statNames.Contains(statName))
{
    Console.WriteLine($"Stat inconnue : {statName} (attendu : {string.Join(", ", statNames)})");
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

foreach ((string flag, string? seuil) in new[]
         {
             ("--hasCrushed", hasCrushedRaw),
             ("--hasBeenCrushed", hasBeenCrushedRaw),
             ("--hasBeenGod", hasBeenGodRaw)
         })
{
    if (seuil != null && !double.TryParse(seuil, out _))
    {
        Console.WriteLine($"Valeur invalide pour {flag} : {seuil} (attendu : un nombre)");
        return;
    }
}

if (seuilDemande && player == null)
{
    Console.WriteLine("--hasCrushed/--hasBeenCrushed/--hasBeenGod nécessitent --player <nom>.");
    return;
}

if (progressDemande && (game == null || player == null))
{
    Console.WriteLine("--progress nécessite --game <valorant|cs2|lol> et --player <nom>.");
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

DataSerie<ValorantMatch> valorant =
    DataSerie<ValorantMatch>.FromCsv(@"data/valorant.csv", ParseValorant);
DataSerie<Cs2Match> cs2 =
    DataSerie<Cs2Match>.FromCsv(@"data/cs2.csv", ParseCS2);
DataSerie<LolMatch> lol =
    DataSerie<LolMatch>.FromCsv(@"data/lol.csv", ParseLoL);

// ─── --hasCrushed / --hasBeenCrushed / --hasBeenGod : Yes/No sur le KDA ──────

if (seuilDemande)
{
    // Pas de type commun entre les trois jeux : on réduit chaque série à des
    // KDA (des `double`) avant de les rassembler.
    List<double> kdas = valorant.Values
        .Where(m => m.Player == player && valorantFilters[filterMode](m))
        .Select(valorantSelectors["kda"])
        .Concat(cs2.Values
            .Where(m => m.Player == player && cs2Filters[filterMode](m))
            .Select(cs2Selectors["kda"]))
        .Concat(lol.Values
            .Where(m => m.Player == player && lolFilters[filterMode](m))
            .Select(lolSelectors["kda"]))
        .ToList();

    if (hasCrushedRaw != null)
        Console.WriteLine(kdas.Any(v => v > double.Parse(hasCrushedRaw)) ? "Yes" : "No");

    if (hasBeenCrushedRaw != null)
        Console.WriteLine(kdas.Any(v => v < double.Parse(hasBeenCrushedRaw)) ? "Yes" : "No");

    if (hasBeenGodRaw != null)
        Console.WriteLine(kdas.Count > 0 && kdas.All(v => v > double.Parse(hasBeenGodRaw)) ? "Yes" : "No");

    return;
}

// ─── --progress : KDA moyen par mois, un GroupBy par mois ────────────────────

if (progressDemande)
{
    // Chaque jeu est réduit au même couple (date, KDA) : c'est ce couple, et
    // non une interface commune, qui permet d'écrire la suite une seule fois.
    IEnumerable<(DateTime Date, double Kda)> matchs = game switch
    {
        "valorant" => valorant.Values
            .Where(m => m.Player == player && valorantFilters[filterMode](m))
            .Select(m => (m.Date, valorantSelectors["kda"](m))),
        "cs2" => cs2.Values
            .Where(m => m.Player == player && cs2Filters[filterMode](m))
            .Select(m => (m.Date, cs2Selectors["kda"](m))),
        "lol" => lol.Values
            .Where(m => m.Player == player && lolFilters[filterMode](m))
            .Select(m => (m.Date, lolSelectors["kda"](m))),
        _ => Enumerable.Empty<(DateTime, double)>()
    };

    // GroupBy(mois).Select(g => g.Average(...)) : un Fold par clé.
    var progression = matchs
        .GroupBy(m => $"{m.Date:yyyy-MM}")
        .OrderBy(g => g.Key)
        .Select(g => new { Mois = g.Key, KdaMoyen = g.Average(m => m.Kda) });

    foreach (var mois in progression)
        Console.WriteLine($"{mois.Mois}     {mois.KdaMoyen:F2}");

    return;
}

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
    if (!Report("Valorant", valorant, valorantAberrant,
                m => m.Date, m => m.Player, valorantFilters[filterMode], valorantSelectors[statName],
                ExportValorant)) return;

if (game == null || game == "cs2")
    if (!Report("CS2", cs2, cs2Aberrant,
                m => m.Date, m => m.Player, cs2Filters[filterMode], cs2Selectors[statName],
                ExportCs2)) return;

if (game == null || game == "lol")
    if (!Report("LoL", lol, lolAberrant,
                m => m.Date, m => m.Player, lolFilters[filterMode], lolSelectors[statName],
                ExportLol)) return;

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
    Console.WriteLine("Seuils (KDA, nécessitent --player)");
    Console.WriteLine("  --hasCrushed <v>             Yes si un KDA > v dans les parties sélectionnées");
    Console.WriteLine("  --hasBeenCrushed <v>         Yes si un KDA < v dans les parties sélectionnées");
    Console.WriteLine("  --hasBeenGod <v>             Yes si tous les KDA > v dans les parties sélectionnées");
    Console.WriteLine();
    Console.WriteLine("Progression (nécessite --game et --player)");
    Console.WriteLine("  --progress                   KDA moyen par mois");
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
//
// La fonction ne connaît aucun type de match : tout ce dont elle a besoin
// (la date, le joueur, le filtre, la stat) lui est passé sous forme de
// fonctions. C'est ce qui remplace l'interface commune d'avant.
bool Report<T>(string label,
               DataSerie<T> serie,
               Func<T, bool> estAberrant,
               Func<T, DateTime> date,
               Func<T, string> joueur,
               Func<T, bool> filtre,
               Func<T, double> stat,
               Action<DataSerie<T>, string> export)
{
    DataSerie<T> aberrants = serie.Outliers(estAberrant);

    if (errorMode == "strict" && aberrants.Count > 0)
    {
        Console.WriteLine($"{label} : {aberrants.Count} valeur(s) aberrante(s) — arrêt (--error strict)");
        foreach (T match in aberrants.Values)
            Console.WriteLine($"  {match}");
        return false;
    }

    DataSerie<T> propre = serie.Sanitize(estAberrant);

    if (errorMode == "hard")
    {
        string fichier = $"{label.ToLower()}_clean.csv";
        export(propre, fichier);
        Console.WriteLine($"{label} : série nettoyée sauvée dans {fichier}");
    }

    // --player et --filter s'enchaînent : chaque Filter retourne une nouvelle
    // série, donc la composition est naturelle.
    DataSerie<T> retenus = propre
        .Filter(m => player == null || joueur(m) == player)
        .Filter(filtre);

    Console.WriteLine($"{label} : {serie.Count} matchs, "
                    + $"{aberrants.Count} écarté(s), {retenus.Count} retenu(s)");

    if (extractMode != null)
    {
        if (retenus.Count == 0)
        {
            Console.WriteLine("  aucun match retenu — rien à extraire");
            Console.WriteLine();
            return true;
        }

        foreach (T match in retenus.Values)
            Console.WriteLine($"  {date(match):yyyy-MM-dd}  {joueur(match),-8}  {statName} = {stat(match):F2}");

        double resultat = extractMode switch
        {
            "min"     => retenus.Values.Select(stat).Min(),
            "max"     => retenus.Values.Select(stat).Max(),
            "avg" => retenus.Values.Select(stat).Average(),
            "mme"     => retenus.MME(stat),
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

    // La série de matchs devient une série de nombres.
    // Normalize fait le même travail que Transform, en ramenant en plus le
    // résultat dans [0.0, 1.0] : inutile d'enchaîner les deux.
    DataSerie<double> valeurs = normalize
        ? retenus.Normalize(stat)
        : retenus.Transform(stat);

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

    foreach ((T match, double valeur) in retenus.Values.Skip(decalage).Zip(valeurs.Values))
        Console.WriteLine($"  {date(match):yyyy-MM-dd}  {joueur(match),-8}  {etiquette} = {valeur:F2}");

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
            DataSerie<Cs2Match> matchsCs2 =
                MatchGenerator.GenerateCs2(joueur, 20, joueur == "Kiara" ? 7 : 42)
                              .Filter(cs2Valide);
            ExportCs2(matchsCs2, fichier);
            Console.WriteLine($"{joueur} : {matchsCs2.Count} matchs CS2 générés → {fichier}");
            break;

        case "Dylan":
            DataSerie<ValorantMatch> matchsValorant =
                MatchGenerator.GenerateValorant(joueur, 20, 11)
                              .Filter(valorantValide);
            ExportValorant(matchsValorant, fichier);
            Console.WriteLine($"{joueur} : {matchsValorant.Count} matchs Valorant générés → {fichier}");
            break;

        case "Noé":
            DataSerie<LolMatch> matchsLol =
                MatchGenerator.GenerateLol(joueur, 20, 3)
                              .Filter(lolValide);
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

ValorantMatch ParseValorant(string[] cols)
{
    return new ValorantMatch(DateTime.Parse(cols[0]), cols[1], cols[2],
        int.Parse(cols[3]), int.Parse(cols[4]), int.Parse(cols[5]),
        int.Parse(cols[6]), int.Parse(cols[7]), bool.Parse(cols[8]));
}

Cs2Match ParseCS2(string[] cols)
{
    return new Cs2Match(DateTime.Parse(cols[0]), cols[1], cols[2], cols[3],
        int.Parse(cols[4]), int.Parse(cols[5]), int.Parse(cols[6]),
        int.Parse(cols[7]), bool.Parse(cols[8]));
}

LolMatch ParseLoL(string[] cols)
{
    return new LolMatch(DateTime.Parse(cols[0]), cols[1], cols[2],
        int.Parse(cols[4]), int.Parse(cols[5]), int.Parse(cols[6]),
        int.Parse(cols[7]), int.Parse(cols[8]), bool.Parse(cols[9]));
}

// ─── Exports : le CSV produit doit rester lisible par FromCsv ────────────────

void ExportValorant(DataSerie<ValorantMatch> matchs, string chemin)
{
    string entete = "date,player,agent,kills,deaths,assists,headshots,rounds_won,won";
    IEnumerable<string> lignes = matchs.Values.Select(m =>
        $"{m.Date:yyyy-MM-dd},{m.Player},{m.Agent},{m.Kills}," +
        $"{m.Deaths},{m.Assists},{m.Headshots},{m.RoundsWon}," +
        $"{m.Won.ToString().ToLower()}");
    File.WriteAllLines(chemin, lignes.Prepend(entete));
}

void ExportCs2(DataSerie<Cs2Match> matchs, string chemin)
{
    string entete = "date,player,map,start_side,kills,deaths,assists,mvps,won";
    IEnumerable<string> lignes = matchs.Values.Select(m =>
        $"{m.Date:yyyy-MM-dd},{m.Player},{m.Map},{m.StartSide}," +
        $"{m.Kills},{m.Deaths},{m.Assists},{m.Mvps}," +
        $"{m.Won.ToString().ToLower()}");
    File.WriteAllLines(chemin, lignes.Prepend(entete));
}

void ExportLol(DataSerie<LolMatch> matchs, string chemin)
{
    // Le CSV LoL a une colonne `role` que LolMatch ne stocke pas : le seul
    // joueur LoL du roster est Support, on la réécrit telle quelle.
    string entete = "date,player,champion,role,kills,deaths,assists,cs,vision_score,won";
    IEnumerable<string> lignes = matchs.Values.Select(m =>
        $"{m.Date:yyyy-MM-dd},{m.Player},{m.Champion},Support," +
        $"{m.Kills},{m.Deaths},{m.Assists},{m.Cs}," +
        $"{m.VisionScore},{m.Won.ToString().ToLower()}");
    File.WriteAllLines(chemin, lignes.Prepend(entete));
}
