using DataLib;

var valorantMatches = new[]
{
    new ValorantMatch("Léa", "Jett",  18, 6, 4, 8,  13, true),
    new ValorantMatch("Léa", "Reyna", 22, 8, 2, 11,  9, false),
    new ValorantMatch("Léa", "Neon",  20, 7, 5,  9, 13, true),
};
var valorant = DataSeries<ValorantMatch>.From(valorantMatches);


var cs2Matches = new[]
{
    new Cs2Match("Raphaël", "Mirage",  "CT", 21, 14, 5, 2, true),
    new Cs2Match("Kiara",   "Dust2",   "T",  26, 11, 1, 4, true),
    new Cs2Match("Raphaël", "Inferno", "T",  14, 16, 6, 1, false),
};
var cs2 = DataSeries<Cs2Match>.From(cs2Matches);


var lolMatches = new[]
{
    new LolMatch("Noé", "Thresh", 2, 4, 18, 42, 71, true),
    new LolMatch("Noé", "Thresh", 1, 6, 12, 35, 64, false),
};
var lol = DataSeries<LolMatch>.From(lolMatches);


Console.WriteLine($"Valorant : {valorant.Count} matchs");
Console.WriteLine($"CS2      : {cs2.Count} matchs");
Console.WriteLine($"LoL      : {lol.Count} matchs");


var wins = valorant.Values.Where(m => m.Won);
Console.WriteLine($"\nVictoires de Léa : {wins.Count()}");


Console.WriteLine("\n--- CS2 ---");
foreach (var m in cs2.Values)
    Console.WriteLine($"  {m.Player} | {m.Map} ({m.StartSide}) | {m.Kills}/{m.Deaths}/{m.Assists} | MVPs: {m.Mvps} | Won: {m.Won}");

Console.WriteLine("\n--- LoL ---");
foreach (var m in lol.Values)
    Console.WriteLine($"  {m.Player} | {m.Champion} | {m.Kills}/{m.Deaths}/{m.Assists} | CS: {m.Cs} | Vision: {m.VisionScore} | Won: {m.Won}");

Console.WriteLine("\n--- Valo ---");
foreach (var m in valorant.Values)
    Console.WriteLine($"  {m.Player} | {m.Agent} | kill {m.Kills} | assists {m.Assists} | Won: {m.Won}");


public class ValorantMatch
{
    public string Player { get; }
    public string Agent { get; }
    public int Kills { get; }
    public int Deaths { get; }
    public int Assists { get; }
    public int Score { get; }
    public int Rounds { get; }
    public bool Won { get; }

    public ValorantMatch(string player, string agent, int kills, int deaths,
                         int assists, int score, int rounds, bool won)
    {
        Player = player; Agent = agent; Kills = kills; Deaths = deaths;
        Assists = assists; Score = score; Rounds = rounds; Won = won;
    }
}

public class Cs2Match
{
    public string Player { get; }
    public string Map { get; }
    public string StartSide { get; }
    public int Kills { get; }
    public int Deaths { get; }
    public int Assists { get; }
    public int Mvps { get; }
    public bool Won { get; }

    public Cs2Match(string player, string map, string startSide, int kills,
                    int deaths, int assists, int mvps, bool won)
    {
        Player = player; Map = map; StartSide = startSide; Kills = kills;
        Deaths = deaths; Assists = assists; Mvps = mvps; Won = won;
    }
}

public class LolMatch
{
    public string Player { get; }
    public string Champion { get; }
    public int Kills { get; }
    public int Deaths { get; }
    public int Assists { get; }
    public int Cs { get; }
    public int VisionScore { get; }
    public bool Won { get; }

    public LolMatch(string player, string champion, int kills, int deaths,
                    int assists, int cs, int visionScore, bool won)
    {
        Player = player; Champion = champion; Kills = kills; Deaths = deaths;
        Assists = assists; Cs = cs; VisionScore = visionScore; Won = won;
    }
}
