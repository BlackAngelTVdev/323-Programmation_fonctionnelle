using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESportApp
{
    public class LolMatch
    {
        public LolMatch(string player, string champion, int kills, int deaths, int assists, int cs, int visionScore, bool won)
        {
            Player = player;
            Champion = champion;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Cs = cs;
            VisionScore = visionScore;
            Won = won;
        }

        public string Player { get; }
        public string Champion { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Cs { get; }
        public int VisionScore { get; }
        public bool Won { get; }

        public override string ToString()
        {
            return $"Player: {Player}, Champion: {Champion}, Kills: {Kills}, Deaths: {Deaths}, Assists: {Assists}, CS: {Cs}, Vision Score: {VisionScore}, Won: {Won}";
        }
    }
}
