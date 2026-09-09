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

DataSeries<Cs2Match> raphaelGenerated = MatchGenerator.GenerateCs2("Raphaël", 20);
Console.WriteLine(raphaelGenerated.Count); // 20

DataSeries<Cs2Match> raphaelValid = raphaelGenerated.Filter(isValid);
Console.WriteLine($"Avant : {raphaelGenerated.Count}, après : {raphaelValid.Count}");

ExportCs2(raphaelValid, "./data/raphael_generated.csv");

static void ExportCs2(DataSeries<Cs2Match> matches, string path)
{
    string header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
    IEnumerable<string> lines = matches.DataPoints.Select(dp =>
        $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide}," +
        $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps},{dp.Value.Won.ToString().ToLower()}"
    );
    File.WriteAllLines(path, lines.Prepend(header));
}