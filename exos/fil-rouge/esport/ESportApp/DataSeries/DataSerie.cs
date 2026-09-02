using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSeries
{
    public class DataSerie<T>
    {
        private readonly IEnumerable<T> _data;

        private DataSerie(IEnumerable<T> data) => _data = data;

        public static DataSerie<T> From(IEnumerable<T> source)
        {
            return new DataSerie<T>(source);
        }

        public static DataSerie<T> FromCsv(string filename, Func<string[], T> parser)
        {
            List<T> data = new List<T>();
            try
            {
                List<string> content = File.ReadAllLines(filename).ToList();
                foreach (string line in content.Skip(1))
                {
                    string[] cols = line.Split(',');
                    data.Add(parser(cols));
                }
            } catch (Exception e) {
                Console.WriteLine($"Erreur d'ouverture du fichier {e.Message}");
            }
            return From(data);
        }
        
        public int Count => _data.Count();
        public IEnumerable<T> Values => _data;
    }
}
