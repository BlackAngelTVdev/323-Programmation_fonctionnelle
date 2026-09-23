using DataLib;
using E_sport_traker;

Func<Cs2Match, bool> isValid = m =>
    m.Kills + m.Assists <= 50 &&
    m.Deaths >= 1;

if (args.Contains("--generate"))
{
    string target = args[Array.IndexOf(args, "--generate") + 1];

    string[] players = target == "all"
        ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
        : new[] { target };

    foreach (string player in players)
    {
        string path = $"./data/{player.ToLower()}_generated.csv";
        DataSeries<Cs2Match> series = MatchGenerator.GenerateCs2(player, 20);
        CommandLine.Cs2(path, $"{player} (généré)").Save(series.Filter(isValid));
        Console.WriteLine($"{player} : données générées et exportées");
    }
    return;
}

// 3.3 — CLI : --player <nom> --filter wins|losses|all --error [strict|soft|hard]
if (args.Any(arg => arg.StartsWith("--")))
{
    // Aide demandée, avec ou sans autres flags.
    if (args.Contains("--help") || args.Contains("-h"))
    {
        CommandLine.ShowHelp();
        return;
    }

    CommandLine.Run(args);
    return;
}

// --- Vérifications des étapes 3.1 et 3.2 ------------------------------------
MatchDataset<ValorantMatch> valorantSet = CommandLine.Valorant("data/valorant.csv");
MatchDataset<Cs2Match> cs2Set = CommandLine.Cs2("data/cs2.csv");
MatchDataset<LolMatch> lolSet = CommandLine.Lol("data/lol.csv");

DataSeries<ValorantMatch> valorant = valorantSet.Series;
DataSeries<Cs2Match> cs2 = cs2Set.Series;
DataSeries<LolMatch> lol = lolSet.Series;

Console.WriteLine($"Valorant : {valorant.Count} matchs");
Console.WriteLine($"CS2      : {cs2.Count} matchs");
Console.WriteLine($"LoL      : {lol.Count} matchs");

DataSeries<ValorantMatch> q1 = valorant.FilterByDate(d => d.Month <= 3);
Console.WriteLine($"Matchs jan–mars : {q1.Count}");

DataSeries<ValorantMatch> q1Wins = valorant.FilterByDate(d => d.Month <= 3).Filter(m => m.Won);
Console.WriteLine($"Victoires jan–mars : {q1Wins.Count}");

// 3.1 — Détection des outliers : on passe un prédicat qui décrit la valeur aberrante.
DataSeries<ValorantMatch> baaad = valorant.Outliers(m => m.Kills < 0);
Console.WriteLine($"Valorant       : {valorant.Count} matchs (inchangé)"); // 25 — immuable
Console.WriteLine($"Outliers Kills : {baaad.Count}");

Console.WriteLine($"Outliers CS2 : {cs2.Outliers(cs2Set.IsOutlier).Count}");
Console.WriteLine($"Outliers LoL : {lol.Outliers(lolSet.IsOutlier).Count}");

// 3.2 — Nettoyage : Sanitize enlève les valeurs impossibles de chaque jeu.
DataSeries<ValorantMatch> cleanValorant = valorant.Sanitize(valorantSet.IsOutlier);
DataSeries<Cs2Match> cleanCs2 = cs2.Sanitize(cs2Set.IsOutlier);
DataSeries<LolMatch> cleanLol = lol.Sanitize(lolSet.IsOutlier);

Console.WriteLine($"Valorant nettoyé : {valorant.Count} -> {cleanValorant.Count} (source inchangée)");
Console.WriteLine($"CS2 nettoyé      : {cs2.Count} -> {cleanCs2.Count}");
Console.WriteLine($"LoL nettoyé      : {lol.Count} -> {cleanLol.Count}");

// Paresse : Filter ne matérialise rien, le prédicat ne tourne qu'à l'énumération.
int appels = 0;
DataSeries<ValorantMatch> paresseux = valorant.Filter(m => { appels++; return m.Won; });
Console.WriteLine($"Appels avant matérialisation : {appels}"); // 0
Console.WriteLine($"Appels après .Count : {paresseux.Count} (total {appels})");

DataSeries<Cs2Match> raphaelGenerated = MatchGenerator.GenerateCs2("Raphaël", 20);
Console.WriteLine(raphaelGenerated.Count); // 20

DataSeries<Cs2Match> raphaelValid = raphaelGenerated.Filter(isValid);
Console.WriteLine($"Avant : {raphaelGenerated.Count}, après : {raphaelValid.Count}");

CommandLine.Cs2("./data/raphael_generated.csv", "Raphaël (généré)").Save(raphaelValid);

// --- 4.1 Transform : le KDA, une seule métrique pour les trois jeux ---------
// Le mapper vient du domaine (Metrics) : DataLib ne sait qu'appliquer une
// fonction, il ignore ce qu'est un KDA. La librairie reste donc générique.
DataSeries<double> kdaValorant = valorant.Transform(Metrics.Kda);
Console.WriteLine($"KDA Valorant : {kdaValorant.Count} valeurs, source {valorant.Count} matchs (inchangée)");

// Le chaînage Filter(...).Transform(...) n'est possible que parce que chaque
// méthode retourne un NOUVEL objet : immuabilité -> composition.
DataSeries<double> kdaLea = valorant.Filter(m => m.Player == "Léa").Transform(Metrics.Kda);
DataSeries<double> kdaLeaWins = valorant.Filter(m => m.Player == "Léa" && m.Won).Transform(Metrics.Kda);
Console.WriteLine($"KDA de Léa : {kdaLea.Count} matchs, dont {kdaLeaWins.Count} gagnés");

// Les timestamps ont traversé la transformation : la série de doubles reste une
// timeseries, on peut encore la découper par date ou par valeur.
Console.WriteLine($"KDA de Léa du {kdaLea.Timestamps.First():yyyy-MM-dd} au {kdaLea.Timestamps.Last():yyyy-MM-dd}");
Console.WriteLine($"KDA Valorant jan-mars : {kdaValorant.FilterByDate(d => d.Month <= 3).Count} valeurs (FilterByDate sur des doubles)");
Console.WriteLine($"KDA Valorant >= 2.0   : {kdaValorant.Filter(v => v >= 2).Count} valeurs");

// Comparaison inter-jeux : la même métrique rend les 5 joueurs comparables.
double Moyenne(DataSeries<double> series) => series.Count == 0 ? 0 : series.Values.Average();

void Ligne(string joueur, string jeu, DataSeries<double> kda)
    => Console.WriteLine($"{joueur,-8} {jeu,-9} {kda.Count,2} matchs   KDA moyen {Moyenne(kda):F2}");

Ligne("Léa", "Valorant", valorant.Filter(m => m.Player == "Léa").Transform(Metrics.Kda));
Ligne("Dylan", "Valorant", valorant.Filter(m => m.Player == "Dylan").Transform(Metrics.Kda));
Ligne("Raphaël", "CS2", cs2.Filter(m => m.Player == "Raphaël").Transform(Metrics.Kda));
Ligne("Kiara", "CS2", cs2.Filter(m => m.Player == "Kiara").Transform(Metrics.Kda));
Ligne("Noé", "LoL", lol.Filter(m => m.Player == "Noé").Transform(Metrics.Kda));

// Mapper défini en lambda (plutôt qu'en groupe de méthodes) sur la formule brute.
Console.WriteLine($"KDA Valorant gagnés : {Moyenne(valorant.Filter(m => m.Won).Transform(m => Metrics.Kda(m.Kills, m.Assists, m.Deaths))):F2}");

// --- 4.2 Normalize : comparer des pommes et des poires ----------------------
// Un KDA de 5 ne veut pas dire la même chose selon le jeu : chaque jeu a sa
// propre échelle. On ramène donc chaque série dans [0, 1].
DataSeries<double> kdaDylan = valorant.Filter(m => m.Player == "Dylan").Transform(Metrics.Kda);
DataSeries<double> kdaRaphael = cs2.Filter(m => m.Player == "Raphaël").Transform(Metrics.Kda);
DataSeries<double> kdaNoe = lol.Filter(m => m.Player == "Noé").Transform(Metrics.Kda);

// L'évaluateur dit ce qu'on normalise : sur une série de KDA déjà calculés c'est
// l'identité, sur une série de matchs c'est la métrique du domaine.
DataSeries<double> kdaLeaNorm = kdaLea.Normalize(v => v);
DataSeries<double> kdaRaphaelNorm = cs2.Filter(m => m.Player == "Raphaël").Normalize(Metrics.Kda);
DataSeries<double> kdaNoeNorm = kdaNoe.Normalize(v => v);

void Norm(string joueur, DataSeries<double> brut, DataSeries<double> norm)
    => Console.WriteLine($"{joueur,-8} KDA brut {brut.Values.Min(),5:F2} .. {brut.Values.Max(),5:F2}" +
                         $"  ->  normalisé {norm.Values.Min():F2} .. {norm.Values.Max():F2}" +
                         $"   ({norm.Count} valeurs, toutes dans [0,1] : {norm.Values.All(v => v >= 0 && v <= 1)})");

Norm("Léa", kdaLea, kdaLeaNorm);
Norm("Dylan", kdaDylan, kdaDylan.Normalize(v => v));
Norm("Raphaël", kdaRaphael, kdaRaphaelNorm);
Norm("Noé", kdaNoe, kdaNoeNorm);

// Les timestamps ont suivi la normalisation : le max reste retrouvable dans le temps.
var meilleurLea = kdaLeaNorm.Timestamps
    .Zip(kdaLeaNorm.Values, (date, kda) => (date, kda))
    .First(p => p.kda == 1);
Console.WriteLine($"Meilleur match de Léa : {meilleurLea.date:yyyy-MM-dd} (normalisé 1.00, KDA brut {kdaLea.Values.Max():F2})");

// Pommes et poires : deux grandeurs de natures différentes, désormais comparables.
DataSeries<double> visionNoeNorm = lol.Filter(m => m.Player == "Noé").Normalize(m => m.VisionScore);
Console.WriteLine($"Noé — KDA normalisé moyen {kdaNoeNorm.Values.Average():F2}" +
                  $" / vision score normalisé moyen {visionNoeNorm.Values.Average():F2}");

// --- 4.3 Smooth : moyenne glissante ----------------------------------------
int window = 3;
DataSeries<double> lisseLea = kdaLea.Smooth(window, v => v);
window = 10; // SANS EFFET : window a été copiée à l'appel de Smooth (passage d'argument).
             // Le lambda capture le PARAMÈTRE windowSize, qui ne change plus jamais.
Console.WriteLine($"KDA Léa : {kdaLea.Count} valeurs brutes -> {lisseLea.Count} lissées (fenêtre 3, les 2 premières écartées)");

// Fenêtre causale : la valeur lissée d'indice i est la moyenne des bruts [i-2 .. i],
// et elle garde la date de i (le dernier élément de sa fenêtre).
var brutsList = kdaLea.Timestamps.Zip(kdaLea.Values, (d, v) => (d, v)).ToList();
var lissesList = lisseLea.Timestamps.Zip(lisseLea.Values, (d, v) => (d, v)).ToList();
foreach (int i in Enumerable.Range(brutsList.Count - 3, 3))
{
    string fenetre = string.Join(", ", brutsList.Skip(i - 2).Take(3).Select(b => b.v.ToString("F2")));
    int j = i - 2; // indice correspondant dans la série lissée (elle commence à i = 2)
    Console.WriteLine($"   {lissesList[j].d:yyyy-MM-dd}  lissé {lissesList[j].v,5:F2}  =  moyenne de [{fenetre}]");
}

// Lisser atténue les pointes : l'amplitude se réduit, la tendance reste.
double Amplitude(DataSeries<double> s) => s.Values.Max() - s.Values.Min();
Console.WriteLine($"Léa : amplitude brute {Amplitude(kdaLea):F2} -> lissée {Amplitude(lisseLea):F2}");

// L'évaluateur porte sur T : ici une série de matchs CS2, fenêtre de 5.
DataSeries<double> lisseRaphael = cs2.Filter(m => m.Player == "Raphaël").Smooth(5, Metrics.Kda);
Console.WriteLine($"Raphaël : KDA lissé(w=5) sur {lisseRaphael.Count} matchs, " +
                  $"du {lisseRaphael.Timestamps.First():yyyy-MM-dd} au {lisseRaphael.Timestamps.Last():yyyy-MM-dd}");

// Composition : Smooth puis Normalize, chaque étape retourne une nouvelle série.
DataSeries<double> lisseNormalise = kdaLea.Smooth(3, v => v).Normalize(v => v);
Console.WriteLine($"Smooth(3) puis Normalize : {lisseNormalise.Count} valeurs, " +
                  $"min {lisseNormalise.Values.Min():F2}, max {lisseNormalise.Values.Max():F2}");

// --- 4.4 (bonus) SelectMany : le flatMap ------------------------------------
// Le coaching staff veut la LISTE PLATE de tous les KDA de l'équipe, tous jeux
// confondus. Or une collection de séries est une collection IMBRIQUÉE : Select
// en garderait la structure (une séquence de séquences), SelectMany l'aplatit.
DataSeries<double> kdaKiara = cs2.Filter(m => m.Player == "Kiara").Transform(Metrics.Kda);

var allSeries = new[] { kdaLea, kdaRaphael, kdaNoe, kdaDylan, kdaKiara };

// ⚠ LE PIÈGE (volontairement faux, à titre de démonstration) :
// Select conserve l'imbrication -> on obtient 5 « valeurs » (une par joueur),
// pas les KDA. Le nombre paraît plausible, il est simplement au MAUVAIS NIVEAU :
// on compte les séries au lieu des matchs. C'est le bug classique du SelectMany.
IEnumerable<IEnumerable<double>> parSelect = allSeries.Select(s => s.Values);
int seaux = parSelect.Count();                          // 5 joueurs
int valeursPlates = allSeries.SelectMany(s => s.Values).Count(); // tous les matchs
Console.WriteLine($"Select (BUGUÉ)   : {seaux} « valeurs »  <-  {seaux} séries comptées" +
                  $" au lieu des {valeursPlates} matchs de l'équipe");

// ✅ SelectMany (flatMap) : une seule séquence plate, chaque série est recollée
// bout à bout. Le type obtenu est IEnumerable<double>, plus IEnumerable<IEnumerable<double>>.
IEnumerable<double> allKda = allSeries.SelectMany(s => s.Values);
Console.WriteLine($"KDA de l'équipe entière : {allKda.Count()} valeurs, moyenne {allKda.Average():F2} " +
                  $"(min {allKda.Min():F2}, max {allKda.Max():F2})");

// Vérification : aucun match ne se perd ni ne se duplique dans l'aplatissement.
foreach (DataSeries<double> s in allSeries)
    Console.WriteLine($"   {s.Count,2} matchs");
Console.WriteLine($"   {allSeries.Sum(s => s.Count),2} matchs au total = {allKda.Count()} valeurs aplaties" +
                  $" ({allSeries.Sum(s => s.Count) == allKda.Count()})");

// --- Vérifications de l'étape 4 ---------------------------------------------
Console.WriteLine();
Console.WriteLine("--- Vérifications étape 4 ---");

// kdaLea.Count = 13 : uniquement les matchs de Léa, aucun autre joueur.
Console.WriteLine($"[ok] kdaLea.Count = {kdaLea.Count} (matchs de Léa uniquement : {kdaLea.Count == 13})");

// Normalisation min-max : les bornes 0.0 et 1.0 sont atteintes EXACTEMENT,
// et aucune valeur ne sort de [0, 1].
Console.WriteLine($"[ok] normalisé : min {kdaLeaNorm.Values.Min():F2}, max {kdaLeaNorm.Values.Max():F2} " +
                  $"(bornes exactes : {kdaLeaNorm.Values.Min() == 0.0 && kdaLeaNorm.Values.Max() == 1.0}, " +
                  $"toutes dans [0,1] : {kdaLeaNorm.Values.All(v => v >= 0 && v <= 1)})");

// Smooth(1) : une fenêtre d'un seul élément = l'identité (mêmes valeurs, même count).
DataSeries<double> lisseLea1 = kdaLea.Smooth(1, v => v);
bool identite = lisseLea1.Count == kdaLea.Count &&
                lisseLea1.Values.Zip(kdaLea.Values, (lisse, brut) => lisse == brut).All(same => same);
Console.WriteLine($"[ok] Smooth(1) = identité : {identite} ({lisseLea1.Count} valeurs conservées)");

// Smooth(3) : la moyenne glissante rapproche les valeurs voisines, donc la somme
// des écarts entre valeurs consécutives diminue.
double EcartsConsecutifs(DataSeries<double> s)
{
    List<double> v = s.Values.ToList();
    return v.Zip(v.Skip(1), (a, b) => Math.Abs(b - a)).Sum();
}

Console.WriteLine($"[ok] Smooth(3) réduit les écarts : {EcartsConsecutifs(kdaLea):F2} -> {EcartsConsecutifs(lisseLea):F2} " +
                  $"(réduits : {EcartsConsecutifs(lisseLea) < EcartsConsecutifs(kdaLea)})");

// Immuabilité : après toutes ces transformations, la série source est intacte.
Console.WriteLine($"[ok] valorant.Count = {valorant.Count} après toutes les transformations (inchangé : {valorant.Count == 25})");
