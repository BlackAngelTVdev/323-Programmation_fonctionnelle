namespace DataLib
{
    public class DataSeries<T>
    {
        private readonly IEnumerable<T> _data;

        private DataSeries(IEnumerable<T> data) => _data = data;

        public static DataSeries<T> From(IEnumerable<T> source) => new DataSeries<T>(source);

        public static DataSeries<T> LoadCsv(string path, Func<string[], T> parse) =>
            From(File.ReadLines(path)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parse));

        public int Count => _data.Count();

        public IEnumerable<T> Values => _data;
    }
}
