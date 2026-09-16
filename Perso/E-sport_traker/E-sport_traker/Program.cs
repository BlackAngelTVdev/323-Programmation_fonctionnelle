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
