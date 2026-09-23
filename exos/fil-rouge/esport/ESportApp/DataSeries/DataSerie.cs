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

        // Créer une datasérie de type générique à partir d'un fichier CSV.
        // Comme le format du fichier est spécifique au type, on ne sait pas
        // comment le parser. C'est l'appelant de la méthode qui doit nous donner
        // le bon outil, sous la forme de la fonction `parser`.

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
        
        // --- Filtrer (exercices 02 et 03) ------------------------------------
        // Garde les éléments qui satisfont le prédicat. La série d'origine n'est
        // jamais modifiée : on retourne une NOUVELLE série.
        public DataSerie<T> Filter(Func<T, bool> predicate)
        {
            return From(_data.Where(predicate));
        }

        // Montre les valeurs aberrantes, c'est-à-dire celles que le prédicat
        // fourni par l'appelant considère comme impossibles.
        public DataSerie<T> Outliers(Func<T, bool> isOutlier)
        {
            return From(_data.Where(isOutlier));
        }

        // Nettoie la série en écartant les valeurs aberrantes.
        // C'est exactement le complément de `Outliers`.
        public DataSerie<T> Sanitize(Func<T, bool> isOutlier)
        {
            return From(_data.Where(item => !isOutlier(item)));
        }

        // --- Transformer (exercice 04) ---------------------------------------
        // Applique le `mapper` à chaque élément. Le type de sortie n'a aucune
        // raison d'être le type d'entrée : DataSerie<ValorantMatch> devient
        // par exemple DataSerie<double> quand on calcule des KDA.
        public DataSerie<TResult> Transform<TResult>(Func<T, TResult> mapper)
        {
            return DataSerie<TResult>.From(_data.Select(mapper));
        }

        // Évalue chaque élément avec l'outil fourni, puis ramène le résultat
        // dans [0.0, 1.0] : la plus petite valeur devient 0, la plus grande 1.
        // Indispensable pour comparer des jeux dont les échelles diffèrent.
        public DataSerie<double> Normalize(Func<T, double> evaluator)
        {
            List<double> valeurs = _data.Select(evaluator).ToList();

            if (valeurs.Count == 0)
                return DataSerie<double>.From(valeurs);

            double min = valeurs.Min();
            double max = valeurs.Max();

            // Toutes les valeurs identiques : pas d'échelle, donc pas de division
            if (max == min)
                return DataSerie<double>.From(valeurs.Select(v => 0.0).ToList());

            return DataSerie<double>.From(valeurs.Select(v => (v - min) / (max - min)).ToList());
        }

        // Moyenne glissante : la valeur d'indice i est la moyenne des éléments
        // [i .. i+windowSize-1]. Une série de n éléments en produit
        // n - windowSize + 1 — avec une fenêtre de 1, rien ne change.
        public DataSerie<double> Smooth(Func<T, double> evaluator, int windowSize)
        {
            List<double> valeurs = _data.Select(evaluator).ToList();
            int combien = Math.Max(0, valeurs.Count - windowSize + 1);

            return DataSerie<double>.From(
                Enumerable.Range(0, combien)
                    .Select(debut => valeurs.Skip(debut).Take(windowSize).Average())
                    .ToList());
        }

        // --- Agréger (exercice 05) --------------------------------------------
        public double MME(Func<T, double> value)
        {
            List<double> valeurs = _data.Select(value).ToList();
            if (valeurs.Count == 0) return 0;
            if (valeurs.Count == 1) return valeurs[0];

            return valeurs.Aggregate(valeurs[0], (mme, v) => (v + mme) / 2);
        }

        public int Count => _data.Count();
        public IEnumerable<T> Values => _data;

        public override string ToString()
        {
            return $"DataSerie<{typeof(T).Name}>: {Count} points: {Environment.NewLine}{String.Join(Environment.NewLine,_data.Select(s => s).ToArray())}";
        }
    }
}
