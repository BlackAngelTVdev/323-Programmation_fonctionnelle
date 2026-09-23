using DataSeries;

namespace ESportApp
{
    // Générateur de matchs simulés (exercice 02).
    // Raphaël, Kiara, Dylan et Noé ont rejoint Team Helvetia en cours de saison :
    // leurs matchs de pré-saison sont reconstitués ici.
    //
    // `Enumerable.Range(...).Select(...)` remplace la boucle for : on décrit CE
    // qu'on veut, pas comment l'accumuler. La lambda passée à `Select` utilise
    // `rng`, `maps`, `start`... déclarés en dehors d'elle : c'est une closure.
    public static class MatchGenerator
    {
        // La série est matérialisée (ToList) avant d'être emballée : sans cela,
        // chaque parcours de la série relancerait les tirages de `rng` et
        // donnerait des valeurs différentes à chaque lecture.
        public static DataSerie<Cs2Match> GenerateCs2(string player, int count, int seed = 42)
        {
            Random rng = new Random(seed);
            string[] maps = { "Dust2", "Mirage", "Inferno", "Nuke", "Ancient" };
            string[] sides = { "CT", "T" };
            DateTime start = new DateTime(2023, 9, 1); // début de la pré-saison

            return DataSerie<Cs2Match>.From(
                Enumerable.Range(1, count)
                    .Select(i => new Cs2Match(
                        start.AddDays(i),
                        player,
                        maps[rng.Next(maps.Length)],
                        sides[rng.Next(sides.Length)],
                        rng.Next(10, 28),  // kills — MR12, max 24 rounds
                        rng.Next(6, 18),   // deaths
                        rng.Next(0, 8),    // assists
                        rng.Next(0, 5),    // mvps
                        rng.Next(2) == 0   // won
                    ))
                    .ToList());
        }

        // Dylan est Controller : moins de kills que Léa, mais plus d'assists.
        public static DataSerie<ValorantMatch> GenerateValorant(string player, int count, int seed = 42)
        {
            Random rng = new Random(seed);
            string[] agents = { "Omen", "Astra", "Brimstone" };
            DateTime start = new DateTime(2023, 9, 1);

            return DataSerie<ValorantMatch>.From(
                Enumerable.Range(1, count)
                    .Select(i =>
                    {
                        bool won = rng.Next(2) == 0;
                        return new ValorantMatch(
                            start.AddDays(i),
                            player,
                            agents[rng.Next(agents.Length)],
                            rng.Next(8, 21),               // kills
                            rng.Next(6, 17),               // deaths
                            rng.Next(4, 13),               // assists
                            rng.Next(2, 10),               // headshots
                            won ? 13 : rng.Next(0, 13),    // roundsWon — 13 si victoire
                            won);
                    })
                    .ToList());
        }

        // Noé est Support : peu de kills, beaucoup d'assists, peu de CS.
        public static DataSerie<LolMatch> GenerateLol(string player, int count, int seed = 42)
        {
            Random rng = new Random(seed);
            string[] champions = { "Thresh", "Nautilus", "Rakan", "Leona" };
            DateTime start = new DateTime(2023, 9, 1);

            return DataSerie<LolMatch>.From(
                Enumerable.Range(1, count)
                    .Select(i => new LolMatch(
                        start.AddDays(i),
                        player,
                        champions[rng.Next(champions.Length)],
                        rng.Next(0, 7),    // kills
                        rng.Next(2, 9),    // deaths
                        rng.Next(10, 23),  // assists
                        rng.Next(30, 81),  // cs
                        rng.Next(55, 86),  // visionScore
                        rng.Next(2) == 0   // won
                    ))
                    .ToList());
        }
    }
}
