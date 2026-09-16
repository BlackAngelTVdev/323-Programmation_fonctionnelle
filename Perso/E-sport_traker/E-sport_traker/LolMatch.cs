using DataLib;

namespace E_sport_traker
{
    public class LolMatch : ITimestamped
    {
        public DateTime Timestamp { get; }
        public string Player { get; }
        public string Champion { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Cs { get; }
        public int VisionScore { get; }
        public bool Won { get; }

        public LolMatch(DateTime timestamp, string player, string champion, int kills, int deaths,
                        int assists, int cs, int visionScore, bool won)
        {
            Timestamp = timestamp;
            Player = player; Champion = champion; Kills = kills; Deaths = deaths;
            Assists = assists; Cs = cs; VisionScore = visionScore; Won = won;
        }

        public static LolMatch Parse(DateTime timestamp, string[] cols) =>
            new LolMatch(
                timestamp,
                cols[1],
                cols[2],
                int.Parse(cols[4]),
                int.Parse(cols[5]),
                int.Parse(cols[6]),
                int.Parse(cols[7]),
                int.Parse(cols[8]),
                bool.Parse(cols[9]));
    }
}
