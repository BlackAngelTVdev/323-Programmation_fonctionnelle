using DataLib;
using E_sport_traker;

string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
List<DataPoint<Cs2Match>> raphaelMatches = MatchGenerator.GenerateCs2Matches("Raphaël", 20, new DateTime(2024, 3, 8));


DataSeries<DataPoint<ValorantMatch>> valorant = DataSeries<DataPoint<ValorantMatch>>.LoadCsv(Path.Combine(dataFolder, "valorant.csv"), ValorantMatch.Parse);
DataSeries<DataPoint<Cs2Match>> cs2 = DataSeries<DataPoint<Cs2Match>>.LoadCsv(Path.Combine(dataFolder, "cs2.csv"), Cs2Match.Parse);
DataSeries<DataPoint<LolMatch>> lol = DataSeries<DataPoint<LolMatch>>.LoadCsv(Path.Combine(dataFolder, "lol.csv"), LolMatch.Parse);


Console.WriteLine($"Valorant : {valorant.Count} matchs");
Console.WriteLine($"CS2      : {cs2.Count} matchs");
Console.WriteLine($"LoL      : {lol.Count} matchs");

string exportPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "raphael_matches.csv");
IEnumerable<string> lines = raphaelMatches.Select(dp => $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide},{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps},{(dp.Value.Won ? "true" : "false")}");

File.WriteAllLines(exportPath, new[] { "date,player,map,start_side,kills,deaths,assists,mvps,won" }.Concat(lines));
Console.WriteLine($"{raphaelMatches.Count} matchs exportés dans {exportPath}");

