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
        DataSeries<Cs2Match> series = MatchGenerator.GenerateCs2(player, 20);
        ExportCs2(series.Filter(isValid), $"./data/{player.ToLower()}_generated.csv");
        Console.WriteLine($"{player} : données générées et exportées");
    }
    return;
}

DataSeries<ValorantMatch> valorant = DataSeries<ValorantMatch>.FromCsv("data/valorant.csv", ValorantMatch.Parse);
DataSeries<Cs2Match> cs2 = DataSeries<Cs2Match>.FromCsv("data/cs2.csv", Cs2Match.Parse);
DataSeries<LolMatch> lol = DataSeries<LolMatch>.FromCsv("data/lol.csv", LolMatch.Parse);

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

// Validation des contraintes sur les trois sources avant analyse.
Console.WriteLine($"Outliers CS2 : {cs2.Outliers(m => !isValid(m)).Count}");
Console.WriteLine($"Outliers LoL : {lol.Outliers(m => m.Kills < 0 || m.Deaths < 0 || m.Cs < 0).Count}");

// 3.2 — Nettoyage : Sanitize enlève les valeurs impossibles de chaque jeu.
DataSeries<ValorantMatch> cleanValorant = valorant.Sanitize(m =>
    m.Kills < 0 || m.Kills > 50 ||
    m.Deaths < 0 || m.Deaths > 30 ||
    m.Assists < 0);

DataSeries<Cs2Match> cleanCs2 = cs2.Sanitize(m =>
    m.Kills + m.Assists > 50 ||
    m.Deaths < 0);

DataSeries<LolMatch> cleanLol = lol.Sanitize(m =>
    m.Kills > 10 ||
    m.Deaths < 1 ||
    m.Assists < 0 ||
    m.Cs < 0);

Console.WriteLine($"Valorant nettoyé : {valorant.Count} -> {cleanValorant.Count} (source inchangée)");
Console.WriteLine($"CS2 nettoyé      : {cs2.Count} -> {cleanCs2.Count}");
Console.WriteLine($"LoL nettoyé      : {lol.Count} -> {cleanLol.Count}");

DataSeries<Cs2Match> raphaelGenerated = MatchGenerator.GenerateCs2("Raphaël", 20);
Console.WriteLine(raphaelGenerated.Count); // 20

DataSeries<Cs2Match> raphaelValid = raphaelGenerated.Filter(isValid);
Console.WriteLine($"Avant : {raphaelGenerated.Count}, après : {raphaelValid.Count}");

ExportCs2(raphaelValid, "./data/raphael_generated.csv");

static void ExportCs2(DataSeries<Cs2Match> matches, string path)
{
    string header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
    IEnumerable<string> lines = matches.Values.Select(m =>
        $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Map},{m.StartSide}," +
        $"{m.Kills},{m.Deaths},{m.Assists},{m.Mvps},{m.Won.ToString().ToLower()}"
    );
    File.WriteAllLines(path, lines.Prepend(header));
}
