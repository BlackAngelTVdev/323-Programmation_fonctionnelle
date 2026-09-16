using DataLib;

namespace E_sport_traker
{
    public static class MatchGenerator
    {
        public static DataSeries<Cs2Match> GenerateCs2(string player, int count, int seed = 42)
        {
            var rng = new Random(seed);
            var maps = new[] { "Dust2", "Mirage", "Inferno", "Nuke", "Ancient" };
            var sides = new[] { "CT", "T" };
            var start = new DateTime(2023, 9, 1); // début de la pré-saison

            // Chaque match généré porte sa date : on appaire (Timestamp, match)
            // pour construire la série, qui garde les deux séquences alignées.
            return DataSeries<Cs2Match>.From(
                Enumerable.Range(1, count)
                    .Select(i => new Cs2Match(
                        start.AddDays(i),
                        player,
                        maps[rng.Next(maps.Length)],
                        sides[rng.Next(2)],
                        rng.Next(10, 28),   // kills
                        rng.Next(6, 18),    // deaths
                        rng.Next(0, 8),     // assists
                        rng.Next(0, 5),     // mvps
                        rng.Next(2) == 0    // won
                    ))
                    .Select(m => (m.Timestamp, m))
            );
        }
    }
}