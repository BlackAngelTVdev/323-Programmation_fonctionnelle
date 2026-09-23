using DataLib;
using System.Text.RegularExpressions;
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

    // Résultat d'une extraction (5.1) : le détail des valeurs retenues, une ligne
    // par match, plus l'indicateur calculé (min, max, avg ou mme).
    public sealed record ExtractReport(string Name, int Total, int Outliers, int Kept,
                                       string StatName, Func<ICombatMatch, double> Stat,
                                       string ExtractorName, double Extracted,
                                       IReadOnlyList<(DateTime Timestamp, string Player, double Value)> Values);

    public static class CommandLine
    {
        // --- Aide de la CLI : un seul endroit décrit les flags, texte affiché tel quel.
        public static void ShowHelp()
        {
            // Palette ANSI
            string reset = "\u001b[0m";
            string bold = "\u001b[1m";
            string dim = "\u001b[2m";
            string cyan = "\u001b[38;2;0;229;255m";
            string magenta = "\u001b[38;2;236;72;153m";
            string purple = "\u001b[38;2;168;85;247m";
            string yellow = "\u001b[38;2;250;204;21m";
            string green = "\u001b[38;2;74;222;128m";
            string gray = "\u001b[38;2;148;163;184m";

            int width = 80; // Largeur exacte de la boîte intérieur

            // Fonction locale pour formater une ligne avec une bordure droite dynamique
            void PrintLine(string content)
            {
                // Supprime temporairement les codes ANSI pour calculer la vraie longueur visible
                string visibleText = Regex.Replace(content, @"\u001b\[[0-9;]*m", "");
                int padding = width - visibleText.Length;
                if (padding < 0) padding = 0;

                Console.WriteLine($"{purple}║{reset} {content}{new string(' ', padding)} {purple}║{reset}");
            }

            // Bordure supérieure
            Console.WriteLine($"{purple}╔{new string('═', width + 2)}╗{reset}");

            // En-tête
            PrintLine($"{cyan}{bold}E-sport_traker{reset} — {gray}analyse des matchs e-sport (Valorant, CS2, LoL){reset}");
            Console.WriteLine($"{purple}╠{new string('═', width + 2)}╣{reset}");

            // Section Utilisation
            PrintLine("");
            PrintLine($"{yellow}{bold}UTILISATION :{reset}");
            PrintLine($"  {green}dotnet run{reset}                                {gray}Mode démo (étapes 3.1, 3.2 et 4){reset}");
            PrintLine($"  {green}dotnet run -- --generate <joueur|all>{reset}");
            PrintLine($"                                              {gray}Génère 20 matchs CS2 fictifs{reset}");
            PrintLine($"                                              {gray}dans data/<joueur>_generated.csv{reset}");
            PrintLine($"  {green}dotnet run -- [options]{reset}                   {gray}Analyse les 3 jeux (csv){reset}");
            PrintLine($"  {green}dotnet run -- --player <joueur> [options]{reset}");
            PrintLine($"                                              {gray}Analyse le jeu principal + data{reset}");

            // Section Options
            PrintLine("");
            PrintLine($"{magenta}{bold}OPTIONS :{reset}");
            PrintLine($"  {cyan}--player <nom>{reset}      Joueur à analyser. {dim}Joueurs connus :{reset}");
            PrintLine($"                      {yellow}Léa{reset} (Val), {yellow}Dylan{reset} (Val), {yellow}Raphaël{reset} (CS2),");
            PrintLine($"                      {yellow}Kiara{reset} (CS2), {yellow}Noé{reset} (LoL)");
            PrintLine($"  {cyan}--filter <mode>{reset}     Filtre résultat : {yellow}all{reset} {gray}(défaut){reset}, {green}wins{reset}, {magenta}losses{reset}");
            PrintLine($"  {cyan}--error <mode>{reset}      Gestion outliers : {yellow}none{reset}, {magenta}strict{reset}, {green}soft{reset}, {purple}hard{reset}");
            PrintLine($"  {cyan}--stat <metrique>{reset}   Métrique affichée : {yellow}kda{reset}, {green}kills{reset}, {magenta}assists{reset}");
            PrintLine($"  {cyan}--game <jeu>{reset}       Un seul jeu : {yellow}valorant{reset}, {green}cs2{reset}, {magenta}lol{reset} {dim}(requis avec --extract){reset}");
            PrintLine($"  {cyan}--extract <indic>{reset}  Indicateur : {yellow}min{reset}, {yellow}max{reset}, {yellow}avg{reset}, {yellow}mme{reset} {dim}(MME = forme du moment){reset}");
            PrintLine($"  {cyan}-h, --help{reset}          Affiche cette aide.");

            // Section Exemples
            PrintLine("");
            PrintLine($"{green}{bold}EXEMPLES :{reset}");
            PrintLine($"  {gray}dotnet run -- --generate all{reset}");
            PrintLine($"  {gray}dotnet run -- --player Léa --filter wins{reset}");
            PrintLine($"  {gray}dotnet run -- --error hard{reset}");
            PrintLine($"  {gray}dotnet run -- --game cs2 --player Raphaël --extract min --stat kills{reset}");
            PrintLine($"  {gray}dotnet run -- --game cs2 --player Kiara --extract mme --stat kills{reset}");
            PrintLine($"  {gray}dotnet run -- --player Raphaël --filter wins --error soft --stat kills{reset}");

            // Bordure inférieure
            Console.WriteLine($"{purple}╚{new string('═', width + 2)}╝{reset}");
        }

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

        // --- Indicateurs d'extraction (5.1) : chaque entrée réduit une série de
        // valeurs en UN nombre. min/max/avg sont des plis classiques ; mme est le
        // pli exponentiel de DataSeries.MME, qui privilégie les valeurs récentes.
        public static readonly IReadOnlyDictionary<string, Func<DataSeries<double>, double>> Extractors =
            new Dictionary<string, Func<DataSeries<double>, double>>
            {
                ["min"] = s => s.Values.Min(),
                ["max"] = s => s.Values.Max(),
                ["avg"] = s => s.Values.Average(),
                ["mme"] = s => s.MME(v => v),
            };

        // --- Politique face aux erreurs, décrite comme des drapeaux plutôt que
        // comme un if/else sur le mode.
        public sealed record ErrorPolicy(string Name, bool Clean, bool Save, bool Stop);

    // Wrappers pour la table GameLoaders : appelés depuis des méthodes (donc après
    // l'initialisation de la classe), ils évitent les avertissements d'ordre
    // d'initialisation des champs statiques.
    private static bool IsValorantOutlier(ICombatMatch m) => ValorantOutlier((ValorantMatch)m);
    private static bool IsCs2Outlier(ICombatMatch m) => Cs2Outlier((Cs2Match)m);
    private static bool IsLolOutlier(ICombatMatch m) => LolOutlier((LolMatch)m);

    // Les datasets par jeu : une seule table pour que --game soit une donnée,
    // pas un switch. Chaque fabrique charge le CSV correspondant à la volée.
    public static readonly IReadOnlyDictionary<string, Func<string, MatchDataset<ICombatMatch>>> GameLoaders =
        new Dictionary<string, Func<string, MatchDataset<ICombatMatch>>>(StringComparer.OrdinalIgnoreCase)
        {
            // La contravariance de Func ne permet pas de renvoyer directement
            // MatchDataset<ValorantMatch> comme MatchDataset<ICombatMatch> :
            // on reconstruit le dataset sur l'interface, les prédicats et mappers
            // de CommandLine étant déjà écrits contre ICombatMatch.
            ["valorant"] = path => new MatchDataset<ICombatMatch>("Valorant", path,
                DataSeries<ICombatMatch>.FromCsv(path, ValorantMatch.Parse),
                IsValorantOutlier, ValorantHeader, m =>
                    $"{m.Timestamp:yyyy-MM-dd},{((ValorantMatch)m).Player},{((ValorantMatch)m).Agent},{((ValorantMatch)m).Kills},{((ValorantMatch)m).Deaths}," +
                    $"{((ValorantMatch)m).Assists},{((ValorantMatch)m).Score},{((ValorantMatch)m).Rounds},{((ValorantMatch)m).Won.ToString().ToLower()}"),
            ["cs2"] = path => new MatchDataset<ICombatMatch>("CS2", path,
                DataSeries<ICombatMatch>.FromCsv(path, Cs2Match.Parse),
                IsCs2Outlier, Cs2Header, m =>
                    $"{m.Timestamp:yyyy-MM-dd},{((Cs2Match)m).Player},{((Cs2Match)m).Map},{((Cs2Match)m).StartSide},{((Cs2Match)m).Kills}," +
                    $"{((Cs2Match)m).Deaths},{((Cs2Match)m).Assists},{((Cs2Match)m).Mvps},{((Cs2Match)m).Won.ToString().ToLower()}"),
            ["lol"] = path => new MatchDataset<ICombatMatch>("LoL", path,
                DataSeries<ICombatMatch>.FromCsv(path, LolMatch.Parse),
                IsLolOutlier, LolHeader, m =>
                    $"{m.Timestamp:yyyy-MM-dd},{((LolMatch)m).Player},{((LolMatch)m).Champion},,{((LolMatch)m).Kills}," +
                    $"{((LolMatch)m).Deaths},{((LolMatch)m).Assists},{((LolMatch)m).Cs},{((LolMatch)m).VisionScore},{((LolMatch)m).Won.ToString().ToLower()}"),
        };

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
                                      ErrorPolicy Policy, Func<ICombatMatch, double> Stat, string StatName,
                                      string? Game, string? Extract, string ExtractorName);

        public static void Run(string[] args)
        {
            Options? options = Parse(args);
            if (options is null) return;

            // 5.1 — Extraction d'indicateur : chemin dédié (un seul jeu, détail
            // ligne à ligne puis l'indicateur), le résumé multi-jeux ne s'applique pas.
            if (options.Extract is not null)
            {
                RunExtraction(options);
                return;
            }

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
            // Aide demandée : on affiche et on ne lance aucune analyse.
            if (args.Any(a => a is "--help" or "-h"))
            {
                ShowHelp();
                return null;
            }

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

            // --game : restreint l'analyse à un seul jeu (cs2 | valorant | lol).
            string? game = FlagValue(args, "--game");
            if (game is not null && !GameLoaders.ContainsKey(game))
            {
                Console.WriteLine($"--game inconnu : {game} (attendu : {string.Join(" | ", GameLoaders.Keys)})");
                return null;
            }

            // --extract : exige un jeu (un indicateur n'a de sens que sur une série homogène).
            string? extract = FlagValue(args, "--extract");
            if (extract is not null)
            {
                if (game is null)
                {
                    Console.WriteLine("--extract exige --game <cs2|valorant|lol> : un indicateur se calcule sur un seul jeu.");
                    return null;
                }
                if (!Extractors.ContainsKey(extract))
                {
                    Console.WriteLine($"--extract inconnu : {extract} (attendu : {string.Join(" | ", Extractors.Keys)})");
                    return null;
                }
            }

            return new Options(player, filter, filterName, policy, stat, statName,
                               game, extract, extract ?? "");
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
            if (options.Game is not null)
            {
                yield return () => Execute(GameLoaders[options.Game](CsvPath(options.Game)), options.Player, options);
                yield break;
            }

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

        private static string CsvPath(string game) => game.ToLowerInvariant() switch
        {
            "valorant" => "data/valorant.csv",
            "cs2" => "data/cs2.csv",
            _ => "data/lol.csv",
        };

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

        // 5.1 — Extraction d'un indicateur numérique (min | max | avg | mme).
        // Un seul jeu (imposé par --extract) : on affiche le détail des valeurs
        // retenues, une ligne par match, puis l'indicateur demandé.
        private static void RunExtraction(Options options)
        {
            MatchDataset<ICombatMatch> ds = GameLoaders[options.Game!](CsvPath(options.Game!));

            // Filtre résultat -> filtre joueur -> retrait des outliers. Chaque étape
            // retourne une nouvelle série : la source reste intacte et les timestamps
            // restent alignés sur les valeurs. Les joueurs voyagent dans kept, donc
            // values (transformée de kept) reste alignée sur les noms.
            DataSeries<ICombatMatch> filtered = ds.Series
                .Filter(options.Filter)
                .Filter(m => options.Player is null ||
                             string.Equals(m.Player, options.Player, StringComparison.OrdinalIgnoreCase));
            DataSeries<ICombatMatch> kept = filtered.Sanitize(ds.IsOutlier);
            DataSeries<double> values = kept.Transform(options.Stat);

            Console.WriteLine($"{ds.Name} : {ds.Series.Count} matchs, {filtered.Count - kept.Count} écarté(s), {kept.Count} retenu(s)");

            // Série vide (joueur absent de ce jeu, filtre trop restrictif, tout
            // écarté comme outliers...) : un indicateur est incalculable, on le dit
            // au lieu de laisser Min/Average/MME lever une exception.
            if (kept.Count == 0)
            {
                if (options.Player is not null &&
                    !ds.Series.Values.Any(m => string.Equals(m.Player, options.Player, StringComparison.OrdinalIgnoreCase)))
                    Console.WriteLine($"  {options.Player} n'a aucun match enregistré dans {ds.Name} (data/{Path.GetFileName(CsvPath(options.Game!))}).");
                else
                    Console.WriteLine("  Aucune valeur retenue : indicateur incalculable.");
                return;
            }

            foreach ((DateTime date, ICombatMatch match, double value) in
                     values.Timestamps.Zip(kept.Values, (d, m) => (d, m))
                         .Zip(values.Values, (p, v) => (p.d, p.m, v)))
                Console.WriteLine($"  {date:yyyy-MM-dd}  {match.Player,-10}{options.StatName} = {value:F2}");

            double indicator = Extractors[options.ExtractorName](values);
            Console.WriteLine($"  {ExtractorLabel(options.ExtractorName, options.StatName)} : {indicator:F2}");
        }

        private static string ExtractorLabel(string extract, string stat) => extract switch
        {
            "min" => $"Min ({stat})",
            "max" => $"Max ({stat})",
            "avg" => $"Moyenne ({stat})",
            _ => $"MME ({stat}) - forme du moment",
        };

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
