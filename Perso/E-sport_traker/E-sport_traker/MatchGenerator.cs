using DataLib;

namespace E_sport_traker
{
    public static class MatchGenerator
    {
        private static readonly string[] Maps = { "Dust2", "Mirage", "Inferno", "Nuke", "Ancient", "Anubis" };
        private static readonly string[] Sides = { "CT", "T" };
        private static readonly Random Random = new();

        public static List<DataPoint<Cs2Match>> GenerateCs2Matches(
            string player, int count, DateTime startDate, int daysBetween = 4)
        {
            var matches = new List<DataPoint<Cs2Match>>(count);
            var date = startDate;

            for (int i = 0; i < count; i++)
            {
                var match = new Cs2Match(
                    player,
                    Maps[Random.Next(Maps.Length)],
                    Sides[Random.Next(Sides.Length)],
                    Random.Next(14, 27),   // kills
                    Random.Next(6, 14),    // deaths
                    Random.Next(1, 6),     // assists
                    Random.Next(0, 5),     // mvps
                    Random.NextDouble() < 0.6); // won

                matches.Add(new DataPoint<Cs2Match>(date, match));
                date = date.AddDays(daysBetween + Random.Next(0, 3));
            }

            return matches;
        }
    }
}