using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSeries
{
    public class Cs2Match
    {
        public string Player { get; }
        public string Map { get; }
        public string StartSide { get; }  // côté joué en 1re mi-temps (CT ou T)
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Mvps { get; }
        public bool Won { get; }

        public Cs2Match(string player, string map, string startSide, int kills,
                        int deaths, int assists, int mvps, bool won)
        {
            Player = player;    
            Map = map;  
            StartSide = startSide;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Mvps = mvps;
            Won = won;
        }

        public override string ToString()
        {
            return $"Player: {Player}, Map: {Map}, Start Side: {StartSide}, Kills: {Kills}, Deaths: {Deaths}, Assists: {Assists}, MVPs: {Mvps}, Won: {Won}";
        }
    }
}
