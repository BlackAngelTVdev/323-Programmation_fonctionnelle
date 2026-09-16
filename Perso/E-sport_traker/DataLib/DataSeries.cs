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
    }
}
