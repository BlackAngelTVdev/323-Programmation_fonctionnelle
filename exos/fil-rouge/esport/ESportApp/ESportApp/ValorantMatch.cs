using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESportApp
{
    public class ValorantMatch
    {
        public DateTime Date { get; }
        public string Player { get; }
        public string Agent { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Headshots { get; }
        public int RoundsWon { get; }
        public bool Won { get; }

        public ValorantMatch(DateTime date, string player, string agent, int kills, int deaths,
                     int assists, int headshots, int roundsWon, bool won)
        {
            Date = date;
            Player = player;
            Agent = agent;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Headshots = headshots;
            RoundsWon = roundsWon;
            Won = won;
        }

        public override string ToString()
        {
            return $"Date: {Date:yyyy-MM-dd}, Player: {Player}, Agent: {Agent}, Kills: {Kills}, Deaths: {Deaths}, Assists: {Assists}, Headshots: {Headshots}, Rounds Won: {RoundsWon}, Won: {Won}";
        }
    }
}
