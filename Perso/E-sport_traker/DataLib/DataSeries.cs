namespace DataLib
{
    // Une série de valeurs, chacune horodatée. Les dates sont stockées EN PARALLÈLE
    // de la séquence de valeurs : T n'a donc aucune contrainte et la série peut
    // contenir aussi bien des matchs que des doubles (un KDA, un score...).
    public class DataSeries<T>
    {
        private readonly IEnumerable<T> _data;
        private readonly IEnumerable<DateTime> _timestamps;

        private DataSeries(IEnumerable<T> data, IEnumerable<DateTime> timestamps)
            => (_data, _timestamps) = (data, timestamps);

        // Chaque paire associe une date à une valeur : c'est le seul constructeur,
        // donc le seul endroit où les deux séquences sont appairées.
        public static DataSeries<T> From(IEnumerable<(DateTime Timestamp, T Value)> pairs)
            => new DataSeries<T>(pairs.Select(p => p.Value), pairs.Select(p => p.Timestamp));

        public static DataSeries<T> FromCsv(string path, Func<DateTime, string[], T> parser)
        {
            IEnumerable<string> lines = File.ReadAllLines(path).Skip(1); // ignorer l'en-tête
            return From(lines.Select(line =>
            {
                string[] cols = line.Split(',');
                DateTime timestamp = DateTime.Parse(cols[0]);
                return (timestamp, parser(timestamp, cols));
            }));
        }

        public int Count => _data.Count();

        public IEnumerable<T> Values => _data;

        public IEnumerable<DateTime> Timestamps => _timestamps;

        // Les éléments appairés à leur date. Toute opération qui ne garde qu'une
        // partie des éléments passe par ici : les deux séquences restent alignées.
        private IEnumerable<(DateTime Timestamp, T Value)> Pairs()
            => _timestamps.Zip(_data, (timestamp, value) => (timestamp, value));

        public DataSeries<T> Filter(Func<T, bool> predicate)
            => From(Pairs().Where(p => predicate(p.Value)));

        // Retourne une nouvelle série contenant uniquement les valeurs aberrantes
        // (celles qui satisfont le prédicat). La série source n'est jamais modifiée.
        public DataSeries<T> Outliers(Func<T, bool> predicate)
            => From(Pairs().Where(p => predicate(p.Value)));

        // Retourne une nouvelle série nettoyée : les valeurs décrites par le prédicat
        // (les outliers) sont retirées. La série source n'est jamais modifiée.
        public DataSeries<T> Sanitize(Func<T, bool> predicate)
            => From(Pairs().Where(p => !predicate(p.Value)));

        public DataSeries<T> FilterByDate(Func<DateTime, bool> predicate)
            => From(Pairs().Where(p => predicate(p.Timestamp)));

        // Map : applique mapper à chaque valeur et retourne une NOUVELLE série.
        // La source n'est jamais modifiée ; le type change (T -> TResult) mais les
        // timestamps traversent la transformation, ils restent alignés sur les valeurs.
        public DataSeries<TResult> Transform<TResult>(Func<T, TResult> mapper)
            => new DataSeries<TResult>(_data.Select(mapper), _timestamps);

        // Normalisation min-max : ramène chaque valeur évaluée dans [0, 1] avec
        //     x' = (x - min) / (max - min)
        // La plus petite valeur vaut 0, la plus grande vaut 1, les autres sont
        // placées proportionnellement : les séries deviennent comparables, quelle
        // que soit leur unité (un KDA, un vision score, des headshots...).
        public DataSeries<double> Normalize(Func<T, double> evaluator)
        {
            // Il faut connaître les extrêmes, donc tout voir une fois : c'est le
            // seul endroit de la librairie qui rompt la paresse. On évalue ici,
            // une seule fois par élément, pour ne pas rappeler l'évaluateur ensuite.
            var evaluated = Pairs()
                .Select(p => (p.Timestamp, Value: evaluator(p.Value)))
                .ToList();

            if (evaluated.Count == 0)
                return DataSeries<double>.From(Enumerable.Empty<(DateTime Timestamp, double Value)>());

            double min = evaluated.Min(e => e.Value);
            double max = evaluated.Max(e => e.Value);
            double range = max - min;

            // Série constante (amplitude nulle) : rien à étaler, on renvoie 0 pour
            // tout le monde plutôt que de diviser par zéro.
            return DataSeries<double>.From(evaluated.Select(e =>
                (e.Timestamp, range == 0 ? 0.0 : (e.Value - min) / range)));
        }

        // Moyenne glissante : la valeur d'indice i est la moyenne des éléments
        // d'indices [i - windowSize + 1 .. i], donc des windowSize derniers éléments.
        // Les windowSize - 1 premiers indices n'ont pas de fenêtre complète : on les
        // écarte, la série lissée est donc plus courte. Chaque valeur lissée garde la
        // date du DERNIER élément de sa fenêtre (la fenêtre est causale : elle regarde
        // en arrière, elle ne connaît pas le futur).
        public DataSeries<double> Smooth(int windowSize, Func<T, double> evaluator)
        {
            if (windowSize < 1)
                throw new ArgumentOutOfRangeException(nameof(windowSize), windowSize,
                    "La taille de la fenêtre doit être au moins 1.");

            // Comme Normalize : on doit accéder aux éléments par index, donc on
            // matérialise, et on évalue une seule fois par élément.
            var evaluated = Pairs()
                .Select(p => (p.Timestamp, Value: evaluator(p.Value)))
                .ToList();

            // Tous les indices i pour lesquels i - windowSize + 1 >= 0 :
            // ce sont les seuls où une fenêtre de taille windowSize est disponible.
            var indices = Enumerable.Range(
                windowSize - 1,
                Math.Max(0, evaluated.Count - windowSize + 1));

            return DataSeries<double>.From(indices.Select(i =>
            {
                // Le lambda capture windowSize (le paramètre) et evaluated :
                // la fenêtre [i - windowSize + 1 .. i] = Skip(...).Take(...).
                double moyenne = evaluated
                    .Skip(i - windowSize + 1)
                    .Take(windowSize)
                    .Average(e => e.Value);

                return (evaluated[i].Timestamp, moyenne);
            }));
        }
    }
}
