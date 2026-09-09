using DataLib;

namespace E_sport_traker
{
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

        public static DataPoint<ValorantMatch> Parse(string[] cols) =>
            new DataPoint<ValorantMatch>(
                DateTime.Parse(cols[0]),
                new ValorantMatch(
                    cols[1], 
                    cols[2], 
                    int.Parse(cols[3]), 
                    int.Parse(cols[4]),
                    int.Parse(cols[5]),
                    int.Parse(cols[6]),
                    int.Parse(cols[7]),
                    bool.Parse(cols[8])));
    }
}