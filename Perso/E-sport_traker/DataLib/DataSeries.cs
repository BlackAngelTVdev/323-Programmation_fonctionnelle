namespace DataLib
{
    public class DataSeries<T>
    {
        private readonly IEnumerable<DataPoint<T>> _data;

        private DataSeries(IEnumerable<DataPoint<T>> data) => _data = data;

        public static DataSeries<T> From(IEnumerable<DataPoint<T>> source) => new DataSeries<T>(source);

        public static DataSeries<T> FromCsv(string path, Func<string[], T> parser)
        {
            IEnumerable<string> lines = File.ReadAllLines(path).Skip(1); // ignorer l'en-tête
            return new DataSeries<T>(lines.Select(line =>
            {
                string[] cols = line.Split(',');
                return new DataPoint<T>(DateTime.Parse(cols[0]), parser(cols));
            }));
        }

        public int Count => _data.Count();

        public IEnumerable<T> Values => _data.Select(dp => dp.Value);

        public IEnumerable<DataPoint<T>> DataPoints => _data;

        public DataSeries<T> Filter(Func<T, bool> predicate)
            => new DataSeries<T>(_data.Where(dp => predicate(dp.Value)));

        public DataSeries<T> FilterByDate(Func<DateTime, bool> predicate)
            => new DataSeries<T>(_data.Where(dp => predicate(dp.Timestamp)));
    }
}