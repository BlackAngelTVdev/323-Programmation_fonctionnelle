using DataLib;
using E_sport_traker;

var dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
var raphaelMatches = MatchGenerator.GenerateCs2Matches("Raphaël", 20, new DateTime(2024, 3, 8));


var valorant = DataSeries<DataPoint<ValorantMatch>>.LoadCsv(Path.Combine(dataFolder, "valorant.csv"), ValorantMatch.Parse);
var cs2 = DataSeries<DataPoint<Cs2Match>>.LoadCsv(Path.Combine(dataFolder, "cs2.csv"), Cs2Match.Parse);
var lol = DataSeries<DataPoint<LolMatch>>.LoadCsv(Path.Combine(dataFolder, "lol.csv"), LolMatch.Parse);


Console.WriteLine($"Valorant : {valorant.Count} matchs");
Console.WriteLine($"CS2      : {cs2.Count} matchs");
Console.WriteLine($"LoL      : {lol.Count} matchs");

var exportPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "raphael_matches.csv");
var lines = raphaelMatches
    .Select(dp => $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide},{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps},{(dp.Value.Won ? "true" : "false")}");

File.WriteAllLines(exportPath, new[] { "date,player,map,start_side,kills,deaths,assists,mvps,won" }.Concat(lines));
Console.WriteLine($"{raphaelMatches.Count} matchs exportés dans {exportPath}");

