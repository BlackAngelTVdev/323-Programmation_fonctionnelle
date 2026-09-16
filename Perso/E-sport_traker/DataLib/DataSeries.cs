namespace DataLib
{
    public class DataSeries<T> where T : ITimestamped
    {
        private readonly IEnumerable<T> _data;

        private DataSeries(IEnumerable<T> data) => _data = data;

        public static DataSeries<T> From(IEnumerable<T> source) => new DataSeries<T>(source);

        public static DataSeries<T> FromCsv(string path, Func<DateTime, string[], T> parser)
        {
            IEnumerable<string> lines = File.ReadAllLines(path).Skip(1); // ignorer l'en-tête
            return new DataSeries<T>(lines.Select(line =>
            {
                string[] cols = line.Split(',');
                return parser(DateTime.Parse(cols[0]), cols);
            }));
        }

        public int Count => _data.Count();

        public IEnumerable<T> Values => _data;

        public DataSeries<T> Filter(Func<T, bool> predicate)
            => new DataSeries<T>(_data.Where(predicate));

        // Retourne une nouvelle série contenant uniquement les valeurs aberrantes
        // (celles qui satisfont le prédicat). La série source n'est jamais modifiée.
        public DataSeries<T> Outliers(Func<T, bool> predicate)
            => DataSeries<T>.From(_data.Where(predicate));

        public DataSeries<T> FilterByDate(Func<DateTime, bool> predicate)
            => new DataSeries<T>(_data.Where(v => predicate(v.Timestamp)));
    }
}
