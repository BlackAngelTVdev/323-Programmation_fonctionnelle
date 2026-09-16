using DataLib;

namespace E_sport_traker
{
    // Une source de données : sa série, son prédicat d'outlier et sa sérialisation CSV.
    // C'est ce qui permet au pipeline de rester générique (le type T porte toute
    // l'information spécifique au jeu).
    public sealed class MatchDataset<T> where T : IMatch
    {
        public string Name { get; }
        public string Path { get; }
        public DataSeries<T> Series { get; }
        public Func<T, bool> IsOutlier { get; }
        public string Header { get; }
        public Func<T, string> ToRow { get; }

        public MatchDataset(string name, string path, DataSeries<T> series, Func<T, bool> isOutlier,
                            string header, Func<T, string> toRow)
        {
            Name = name;
            Path = path;
            Series = series;
            IsOutlier = isOutlier;
            Header = header;
            ToRow = toRow;
        }

        // On n'écrase jamais la source : la version nettoyée va dans <fichier>_clean.csv.
        public string CleanPath => Path.EndsWith(".csv") ? Path[..^4] + "_clean.csv" : Path + "_clean.csv";

        public void Save(DataSeries<T> series, string path)
            => File.WriteAllLines(path, series.Values.Select(ToRow).Prepend(Header));

        // Réécrit le fichier source de ce dataset.
        public void Save(DataSeries<T> series) => Save(series, Path);
    }

    public sealed record ModeReport(string Name, int Total, int Filtered, int Outliers,
                                    int Remaining, double StatMean, IReadOnlyList<string> OutlierRows, string? SavedPath);

    public static class CommandLine
    {
        // --- Dispatch fonctionnel : chaque critère est une donnée, pas une branche.
        // Ajouter un critère = ajouter une entrée ici, sans toucher au pipeline.
        public static readonly IReadOnlyDictionary<string, Func<IMatch, bool>> WinFilters =
            new Dictionary<string, Func<IMatch, bool>>
            {
                ["all"] = _ => true,
                ["wins"] = m => m.Won,
                ["losses"] = m => !m.Won,
            };

        // --- Table de sélecteurs : le flag --stat choisit la FONCTION de transformation.
        // Un Func<ICombatMatch, double> est une valeur comme une autre : ajouter une stat
        // = une ligne ici, et l'appel à Transform ne change jamais.
        public static readonly IReadOnlyDictionary<string, Func<ICombatMatch, double>> StatSelectors =
            new Dictionary<string, Func<ICombatMatch, double>>
            {
                ["kda"] = m => Metrics.Kda(m.Kills, m.Assists, m.Deaths),
                ["kills"] = m => m.Kills,
                ["assists"] = m => m.Assists,
            };

        // --- Politique face aux erreurs, décrite comme des drapeaux plutôt que
        // comme un if/else sur le mode.
        public sealed record ErrorPolicy(string Name, bool Clean, bool Save, bool Stop);

        public static readonly IReadOnlyDictionary<string, ErrorPolicy> ErrorPolicies =
            new Dictionary<string, ErrorPolicy>
            {
                ["none"] = new("none", Clean: false, Save: false, Stop: false),
                ["strict"] = new("strict", Clean: false, Save: false, Stop: true),
                ["soft"] = new("soft", Clean: true, Save: false, Stop: false),
                ["hard"] = new("hard", Clean: true, Save: true, Stop: false),
            };

        // --- Prédicats d'outliers, partagés avec les vérifications 3.1 / 3.2.
        public static readonly Func<ValorantMatch, bool> ValorantOutlier = m =>
            m.Kills < 0 || m.Kills > 50 ||
            m.Deaths < 0 || m.Deaths > 30 ||
            m.Assists < 0;

        public static readonly Func<Cs2Match, bool> Cs2Outlier = m =>
            m.Kills + m.Assists > 50 || m.Deaths < 0;

        public static readonly Func<LolMatch, bool> LolOutlier = m =>
            m.Kills > 10 || m.Deaths < 1 || m.Assists < 0 || m.Cs < 0;

        // --- Sérialisation : les en-têtes reprennent la disposition des CSV sources,
        // pour qu'un fichier _clean.csv reste relisible par les Parse.
        public const string ValorantHeader = "date,player,agent,kills,deaths,assists,headshots,rounds_won,won";
        public const string Cs2Header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
        public const string LolHeader = "date,player,champion,role,kills,deaths,assists,cs,vision_score,won";

        public static MatchDataset<ValorantMatch> Valorant(string path, string name = "Valorant") =>
            new(name, path, DataSeries<ValorantMatch>.FromCsv(path, ValorantMatch.Parse),
                ValorantOutlier, ValorantHeader, m =>
                    $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Agent},{m.Kills},{m.Deaths}," +
                    $"{m.Assists},{m.Score},{m.Rounds},{m.Won.ToString().ToLower()}");

        public static MatchDataset<Cs2Match> Cs2(string path, string name = "CS2") =>
            new(name, path, DataSeries<Cs2Match>.FromCsv(path, Cs2Match.Parse),
                Cs2Outlier, Cs2Header, m =>
                    $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Map},{m.StartSide},{m.Kills}," +
                    $"{m.Deaths},{m.Assists},{m.Mvps},{m.Won.ToString().ToLower()}");

        // Le rôle (colonne 3) n'est pas modélisé : on le laisse vide pour garder les
        // colonnes alignées avec LolMatch.Parse.
        public static MatchDataset<LolMatch> Lol(string path, string name = "LoL") =>
            new(name, path, DataSeries<LolMatch>.FromCsv(path, LolMatch.Parse),
                LolOutlier, LolHeader, m =>
                    $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Champion},,{m.Kills}," +
                    $"{m.Deaths},{m.Assists},{m.Cs},{m.VisionScore},{m.Won.ToString().ToLower()}");

        private sealed record Options(string? Player, Func<IMatch, bool> Filter, string FilterName,
                                      ErrorPolicy Policy, Func<ICombatMatch, double> Stat, string StatName);

        public static void Run(string[] args)
        {
            Options? options = Parse(args);
            if (options is null) return;

            Console.WriteLine($"--player {options.Player ?? "(tous)"} | --filter {options.FilterName} | --error {options.Policy.Name} | --stat {options.StatName}");
            Console.WriteLine();

            List<ModeReport> reports = new();
            foreach (Func<ModeReport> job in BuildJobs(options))
            {
                ModeReport report = job();
                reports.Add(report);
                // En strict on montre le diagnostic de chaque source ; sinon on ne
                // détaille que ce qui a réellement été éliminé.
                if (options.Policy.Stop || report.Outliers > 0) PrintOutliers(report);
            }

            // strict : on a affiché les outliers, on s'arrête sans rien nettoyer.
            if (options.Policy.Stop)
            {
                Console.WriteLine();
                Console.WriteLine("Mode strict : aucun nettoyage appliqué, arrêt.");
                return;
            }

            Console.WriteLine();
            PrintSummary(reports, options.StatName);
        }

        private static Options? Parse(string[] args)
        {
            string? player = FlagValue(args, "--player");
            string filterName = FlagValue(args, "--filter") ?? "all";

            // --error sans valeur : on suppose soft (on élimine sans écrire).
            string errorName = args.Contains("--error") ? FlagValue(args, "--error") ?? "soft" : "none";

            if (!WinFilters.TryGetValue(filterName, out Func<IMatch, bool>? filter))
            {
                Console.WriteLine($"--filter inconnu : {filterName} (attendu : {string.Join(" | ", WinFilters.Keys)})");
                return null;
            }
            if (!ErrorPolicies.TryGetValue(errorName, out ErrorPolicy? policy))
            {
                Console.WriteLine($"--error inconnu : {errorName} (attendu : strict | soft | hard)");
                return null;
            }

            // --stat sans valeur : on retombe sur kda (la métrique par défaut).
            string statName = FlagValue(args, "--stat") ?? "kda";
            if (!StatSelectors.TryGetValue(statName, out Func<ICombatMatch, double>? stat))
            {
                Console.WriteLine($"--stat inconnue : {statName} (attendu : {string.Join(" | ", StatSelectors.Keys)})");
                return null;
            }
            return new Options(player, filter, filterName, policy, stat, statName);
        }

        private static string? FlagValue(string[] args, string flag)
        {
            int i = Array.IndexOf(args, flag);
            if (i < 0) return null;
            int next = i + 1;
            return next < args.Length && !args[next].StartsWith("--") ? args[next] : null;
        }

        private static IEnumerable<Func<ModeReport>> BuildJobs(Options options)
        {
            if (options.Player is null)
            {
                yield return () => Execute(Valorant("data/valorant.csv"), null, options);
                yield return () => Execute(Cs2("data/cs2.csv"), null, options);
                yield return () => Execute(Lol("data/lol.csv"), null, options);
                yield break;
            }

            Dictionary<string, string> roster = Roster();
            if (!roster.TryGetValue(options.Player, out string? game))
            {
                Console.WriteLine($"Joueur inconnu : {options.Player} (connus : {string.Join(", ", roster.Keys)})");
                yield break;
            }

            // Le CSV réel du jeu principal du joueur (roster.csv).
            switch (game)
            {
                case "Valorant":
                    yield return () => Execute(Valorant("data/valorant.csv"), options.Player, options);
                    break;
                case "CS2":
                    yield return () => Execute(Cs2("data/cs2.csv"), options.Player, options);
                    break;
                case "LoL":
                    yield return () => Execute(Lol("data/lol.csv"), options.Player, options);
                    break;
            }

            // Puis ses matchs générés (exercice 02) — toujours du CS2.
            string generated = $"data/{options.Player.ToLower()}_generated.csv";
            if (File.Exists(generated))
                yield return () => Execute(Cs2(generated, $"{options.Player} (généré)"), options.Player, options);
            else
                Console.WriteLine($"Pas de données générées pour {options.Player} ({generated})");
        }

        private static ModeReport Execute<T>(MatchDataset<T> dataset, string? player, Options options)
            where T : ICombatMatch
        {
            // Le prédicat commun (IMatch) est réappliqué sur T : variance non permise
            // car T pourrait être un struct implémentant IMatch.
            DataSeries<T> filtered = dataset.Series.Filter(m => options.Filter(m));
            if (player is not null)
                filtered = filtered.Filter(m => string.Equals(m.Player, player, StringComparison.OrdinalIgnoreCase));

            // Outliers détectés (diagnostic, toujours calculé pour le rapport).
            DataSeries<T> outliers = filtered.Outliers(dataset.IsOutlier);
            IReadOnlyList<string> outlierRows = outliers.Values.Select(dataset.ToRow).ToList();

            // Nettoyage éventuel : la série source n'est jamais modifiée.
            DataSeries<T> kept = options.Policy.Clean ? filtered.Sanitize(dataset.IsOutlier) : filtered;

            // La transformation choisie (--stat) s'applique à la série retenue :
            // le sélecteur est une valeur tirée de la table, la source reste intacte.
            DataSeries<double> stats = kept.Transform(m => options.Stat(m));
            double statMean = stats.Count == 0 ? 0.0 : stats.Values.Average();

            string? saved = null;
            if (options.Policy.Save)
            {
                saved = dataset.CleanPath;
                dataset.Save(kept, saved);
            }

            return new ModeReport(dataset.Name, dataset.Series.Count, filtered.Count,
                                  outliers.Count, kept.Count, statMean, outlierRows, saved);
        }

        private static void PrintOutliers(ModeReport report)
        {
            Console.WriteLine($"{report.Name} : {report.Outliers} outlier(s)");
            foreach (string row in report.OutlierRows)
                Console.WriteLine($"  {row}");
        }

        private static void PrintSummary(IReadOnlyList<ModeReport> reports, string statName)
        {
            Console.WriteLine($"{"Jeu",-22}{"Total",7}{"Filtrés",9}{"Outliers",10}{"Restants",10}{$"{statName} moy",12}");
            foreach (ModeReport r in reports)
            {
                Console.WriteLine($"{r.Name,-22}{r.Total,7}{r.Filtered,9}{r.Outliers,10}{r.Remaining,10}{r.StatMean,12:F2}");
                if (r.SavedPath is not null)
                    Console.WriteLine($"  -> nettoyé écrit dans {r.SavedPath}");
            }
        }

        private static Dictionary<string, string> Roster() =>
            File.ReadAllLines("data/roster.csv").Skip(1)
                .Where(line => line.Length > 0)
                .Select(line => line.Split(','))
                .ToDictionary(cols => cols[0], cols => cols[1], StringComparer.OrdinalIgnoreCase);
    }
}
