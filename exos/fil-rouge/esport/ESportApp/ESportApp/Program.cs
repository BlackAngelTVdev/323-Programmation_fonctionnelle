// See https://aka.ms/new-console-template for more information
using DataSeries;
using ESportApp;

DataSerie<DataPoint<ValorantMatch>> valorant;
DataSerie<DataPoint<Cs2Match>> cs2;
DataSerie<DataPoint<LolMatch>> lol;

valorant = DataSerie< DataPoint<ValorantMatch>>.FromCsv(@"data\valorant.csv", ParseValorant);
cs2 = DataSerie< DataPoint<Cs2Match>>.FromCsv(@"data\cs2.csv", ParseCS2);
lol = DataSerie< DataPoint<LolMatch>>.FromCsv(@"data\Lol.csv", ParseLoL);

Console.WriteLine($"Il y a  {valorant.Count} matches dans la série Valorant"); // 3
Console.WriteLine($"Il y a  {cs2.Count} matches dans la série CS2"); // 3
Console.WriteLine($"Il y a  {lol.Count} matches dans la série LoL"); // 3

Console.ReadKey();

DataPoint<ValorantMatch> ParseValorant(string[] cols)
{
    ValorantMatch match = new ValorantMatch(cols[1], cols[2], int.Parse(cols[3]), int.Parse(cols[4]), int.Parse(cols[5]), int.Parse(cols[6]), int.Parse(cols[7]), cols[8] == "TRUE");
    DateTime date = DateTime.Parse(cols[0]);
    return new DataPoint<ValorantMatch>(date,match);
}
DataPoint<Cs2Match> ParseCS2(string[] cols)
{
    return new DataPoint<Cs2Match>(DateTime.Parse(cols[0]), new Cs2Match(cols[1], cols[2], cols[3], int.Parse(cols[4]), int.Parse(cols[5]), int.Parse(cols[6]), int.Parse(cols[7]), cols[8] == "TRUE"));
}
DataPoint<LolMatch> ParseLoL(string[] cols)
{
    return new DataPoint<LolMatch>(DateTime.Parse(cols[0]), new LolMatch(cols[1], cols[2], int.Parse(cols[4]), int.Parse(cols[5]), int.Parse(cols[6]), int.Parse(cols[7]), int.Parse(cols[8]), cols[9] == "TRUE"));
}
