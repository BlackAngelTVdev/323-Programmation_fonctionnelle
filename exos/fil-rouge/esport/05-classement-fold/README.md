# Exercice 05 — Classement de saison

> Partie 4 — `.Fold()` + `.Statistics()` + `.SlidingWindow()`

## Concepts théoriques

- [Thématique 04 — Fold et agrégation](../../../../thematiques/04-fold-agregation.md)
- [Fold — l'agrégation universelle](../../../../supports/source/04-Reduce.md#fold-—-l-agregation-universelle)
- [GroupBy — agrégation par clé](../../../../supports/source/04-Reduce.md#groupby)

## Contexte

Établir le classement officiel des 5 joueurs de Team Helvetia pour la saison.
Le coaching staff veut savoir : qui est le plus régulier ? qui progresse le plus vite ?

`.Fold()` est l'outil fondamental qui permet de répondre à toutes ces questions
en une seule abstraction.

---

## Concept FP : Fold — l'agrégation universelle

`Sum`, `Count`, `Max`, `Any`, `All` sont tous des cas particuliers de `Fold`.
Implémenter `Fold` une seule fois suffit à exprimer n'importe quelle agrégation.

```
[a, b, c, d] avec seed s et f :
s → f(s, a) → f(f(s,a), b) → f(f(f(s,a),b), c) → résultat final
```

→ Théorie : [Reduce / Aggregate](../../../../supports/source/04-Reduce.md) ·
[Fold — l'agrégation universelle](../../../../supports/source/04-Reduce.md#fold-—-l-agregation-universelle)

---

## Étape 1 — Implémenter `.Fold<TResult>()`

**Avant de coder :** quelle méthode LINQ fait exactement ce que décrit le schéma ci-dessus —
accumuler une valeur en appliquant une fonction à chaque élément ?

<details>
<summary>Indice</summary>

`Aggregate(seed, combiner)` — c'est le Fold de LINQ.
La méthode de la bibliothèque n'a qu'à déléguer à `Aggregate`.

</details>

```csharp
public TResult Fold<TResult>(TResult seed, Func<TResult, T, TResult> combiner)
{
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public TResult Fold<TResult>(TResult seed, Func<TResult, T, TResult> combiner)
    => _data.Aggregate(seed, combiner);
```

</details>

Réécrire les agrégations classiques avec `Fold` sur les KDA de Léa :

```csharp
var kdaValues = kdaLea; // DataSeries<double>

var sum   = kdaValues.Fold(0.0, (acc, val) => acc + val);
var count = kdaValues.Fold(0,   (acc, _)   => acc + 1);
var best  = kdaValues.Fold(double.MinValue, (acc, val) => val > acc ? val : acc);

var mean = sum / count;
Console.WriteLine($"KDA moyen de Léa : {mean:F2}");
Console.WriteLine($"KDA max de Léa   : {best:F2}");
```

Reproduire pour les 4 autres joueurs et afficher le classement.

---

## Étape 2 — `.SlidingWindow(size)` — progression mensuelle

**Avant de coder :** une fenêtre glissante de taille 5 à partir d'une liste de 13 éléments
produit combien de fenêtres ? Quelle formule générale ?

<details>
<summary>Indice</summary>

`count - size + 1` fenêtres. Pour 13 éléments avec taille 5 : `13 - 5 + 1 = 9` fenêtres.

</details>

```csharp
public IEnumerable<DataSeries<T>> SlidingWindow(int size)
{
    var values = _data.ToList();
    return Enumerable.Range(0, Math.Max(0, values.Count - size + 1))
        .Select(i => // extraire une fenêtre de `size` éléments à partir de l'indice i
        );
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public IEnumerable<DataSeries<T>> SlidingWindow(int size)
{
    var values = _data.ToList();
    return Enumerable.Range(0, Math.Max(0, values.Count - size + 1))
        .Select(i => DataSeries<T>.From(values.Skip(i).Take(size)));
}
```

</details>

Calculer la moyenne KDA par fenêtre de 5 matchs pour Léa :

```csharp
var progression = kdaLea
    .SlidingWindow(5)
    .Select(window => window.Fold(0.0, (acc, v) => acc + v) / 5);

Console.WriteLine("Progression KDA Léa (fenêtres de 5 matchs) :");
foreach (var avg in progression)
    Console.WriteLine($"  {avg:F2}");
```

---

## Étape 3 — `.Statistics()` — qui est le plus régulier ?

```csharp
public class SeriesStats
{
    public double Min { get; }
    public double Max { get; }
    public double Mean { get; }
    public double StdDev { get; }

    public SeriesStats(double min, double max, double mean, double stdDev)
    {
        Min = min;
        Max = max;
        Mean = mean;
        StdDev = stdDev;
    }
}

public SeriesStats Statistics()
{
    var values   = _data.Cast<double>().ToList();
    var mean     = // ...
    var variance = // ...
    return new SeriesStats(min: /* ... */, max: /* ... */, mean: mean, stdDev: /* ... */);
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public SeriesStats Statistics()
{
    var values   = _data.Cast<double>().ToList();
    var mean     = values.Aggregate(0.0, (acc, v) => acc + v) / values.Count;
    var variance = values.Aggregate(0.0, (acc, v) => acc + Math.Pow(v - mean, 2)) / values.Count;
    return new SeriesStats(
        min:    values.Min(),
        max:    values.Max(),
        mean:   mean,
        stdDev: Math.Sqrt(variance)
    );
}
```

</details>

Comparer les profils — un écart-type faible = joueur régulier :

```csharp
var statsLea     = kdaLea.Statistics();
var statsRaphael = kdaRaphael.Statistics();
Console.WriteLine($"Léa     — KDA moy : {statsLea.Mean:F2}, écart-type : {statsLea.StdDev:F2}");
Console.WriteLine($"Raphaël — KDA moy : {statsRaphael.Mean:F2}, écart-type : {statsRaphael.StdDev:F2}");
```

Qui mérite la place de titulaire aux playoffs ?

---

## Étape 4 — Interface CLI

Ajouter `--rank` pour afficher le classement des joueurs par KDA moyen,
et `--window <n>` pour afficher la progression sur des fenêtres glissantes.

**Avant de coder :** Comment trier une collection de tuples `(nom, kdaMoyen)` par valeur décroissante ?
Pour `--window`, comment récupérer `n` sous forme d'entier depuis `args` ?

```
dotnet run -- --rank
dotnet run -- --game valorant --player Léa --stat kda --window 3
```

<details>
<summary>Voir la solution</summary>

```csharp
if (args.Contains("--rank"))
{
    var players = new[]
    {
        ("Léa",     kdaLea.Fold(0.0,     (a, v) => a + v) / kdaLea.Count),
        ("Raphaël", kdaRaphael.Fold(0.0, (a, v) => a + v) / kdaRaphael.Count),
        ("Noé",     kdaNoe.Fold(0.0,     (a, v) => a + v) / kdaNoe.Count),
        ("Dylan",   kdaDylan.Fold(0.0,   (a, v) => a + v) / kdaDylan.Count),
        ("Kiara",   kdaKiara.Fold(0.0,   (a, v) => a + v) / kdaKiara.Count),
    };
    foreach (var (name, kda) in players.OrderByDescending(p => p.Item2))
        Console.WriteLine($"{name,-10} KDA moy : {kda:F2}");
}

int window = args.Contains("--window")
    ? int.Parse(args[Array.IndexOf(args, "--window") + 1])
    : 5;
```

</details>

---

## Étape bonus (avancé) — GroupBy

> Étape optionnelle — pour aller plus loin.

Le classement de l'étape 4 construit les moyennes joueur par joueur, à la main.
`GroupBy` fait le partitionnement automatiquement : les stats **par joueur** en un seul pipeline.

```csharp
// Tous les matchs Valorant (Léa + Dylan) — stats par joueur en un pipeline
var ranking = valorant.Values
    .GroupBy(m => m.Player)
    .Select(g => new
    {
        Player  = g.Key,
        Matches = g.Count(),
        AvgKda  = g.Aggregate(0.0, (acc, m) =>
                      acc + (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths))
                  / g.Count()
    })
    .OrderByDescending(s => s.AvgKda);

foreach (var s in ranking)
    Console.WriteLine($"{s.Player,-10} {s.Matches} matchs — KDA moy : {s.AvgKda:F2}");
```

Le motif `GroupBy(clé).Select(g => g.Aggregate(...))` = partitionner, puis réduire chaque
partition — un `Fold` par clé.
→ [GroupBy — agréger par clé](../../../../supports/source/04-Reduce.md#groupby)

---

## Vérification

- `Fold` sur liste vide retourne `seed`
- `SlidingWindow(5)` sur 13 matchs produit 9 fenêtres (13 - 5 + 1 = 9)
- `Statistics().Mean` correspond à `Fold(0.0, (acc,v)=>acc+v) / Count`
- Les écarts-types permettent de distinguer les profils réguliers des profils variables
