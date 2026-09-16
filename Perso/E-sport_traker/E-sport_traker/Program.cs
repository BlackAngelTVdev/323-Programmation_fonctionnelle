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
