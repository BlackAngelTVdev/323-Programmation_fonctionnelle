using DataLib;

namespace E_sport_traker
{
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

        public static DataPoint<Cs2Match> Parse(string[] cols) =>
            new DataPoint<Cs2Match>(
                DateTime.Parse(cols[0]),
                new Cs2Match(cols[1],
                    cols[2],
                    cols[3],
                    int.Parse(cols[4]),
                    int.Parse(cols[5]),
                    int.Parse(cols[6]),
                    int.Parse(cols[7]),
                    bool.Parse(cols[8])));
    }
}