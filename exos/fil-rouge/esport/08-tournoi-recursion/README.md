# Exercice 08 — Bracket de tournoi

> Partie 8 — `.Decompose()` récursif + cas de base + règle de combinaison

## Concepts théoriques

- [Thématique 08 — Récursivité](../../../../thematiques/08-recursion.md)
- [Récursivité — décomposition fonctionnelle](../../../../supports/source/08-Recursivite.md)
- [Récursion et Fold](../../../../supports/source/08-Recursivite.md#recursion-et-fold)

## Contexte

Team Helvetia se qualifie pour les playoffs. Le bracket de tournoi fonctionne
par élimination directe — on divise les équipes en deux moitiés, chaque moitié joue
ses matchs, et on combine les résultats. C'est exactement la structure d'une récursion.

`Decompose` divise récursivement une série en sous-séries — utile pour l'analyse
multi-échelle : voir les tendances à court terme (5 matchs) et long terme (toute la saison).

---

## Concept FP : Récursion = décomposition fonctionnelle

Tout algorithme récursif suit le même schéma :
1. **Cas de base** : le plus petit problème résolu directement
2. **Règle de combinaison** : résoudre en combinant des solutions plus petites

Que se passerait-il sans le cas de base ?
→ [Récursion et programmation fonctionnelle](../../../../supports/source/08-Recursivite.md#recursion-et-programmation-fonctionnelle)

---

## Étape 1 — Implémenter `.Decompose(minSize)`

```csharp
public IEnumerable<DataSeries<T>> Decompose(int minSize)
{
    var values = _data.ToList();

    if (/* cas de base */)
        return // ...

    int mid   = // ...
    var left  = // ...
    var right = // ...

    return // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public IEnumerable<DataSeries<T>> Decompose(int minSize)
{
    var values = _data.ToList();

    if (values.Count <= minSize)
        return new[] { this };

    int mid   = values.Count / 2;
    var left  = DataSeries<T>.From(values.Take(mid));
    var right = DataSeries<T>.From(values.Skip(mid));

    return left.Decompose(minSize).Concat(right.Decompose(minSize));
}
```

</details>

> La récursion et `Aggregate` (Fold, exercice 05) sont les deux faces de la même pièce :
> `Aggregate` *est* la récursion, généralisée et rendue itérative pour éviter les stack overflows.
> → [Récursion et Fold](../../../../supports/source/08-Recursivite.md#recursion-et-fold)

---

## Étape 2 — Tracer l'arbre de décomposition

Prédire le résultat pour 8 éléments avec `minSize = 2` :

<details>
<summary>Voir l'arbre</summary>

```
[M1 M2 M3 M4 M5 M6 M7 M8]
         ↓ Decompose(2)
   [M1 M2 M3 M4]      [M5 M6 M7 M8]
       ↓                    ↓
  [M1 M2] [M3 M4]     [M5 M6] [M7 M8]
```

4 sous-séries de 2 éléments.

</details>

```csharp
var series8   = DataSeries<double>.From(kdaLea.DataPoints.Take(8));
var subSeries = series8.Decompose(minSize: 2);

Console.WriteLine(subSeries.Count()); // 4
foreach (var s in subSeries)
    Console.WriteLine($"  [{string.Join(", ", s.Values.Select(v => v.ToString("F2")))}]");
```

`DataPoints.Take(8)` préserve les timestamps des 8 premiers matchs — les sous-séries restent datées.

---

## Étape 3 — Bracket de tournoi simplifié

```csharp
var series = DataSeries<double>.From(kdaLea.Smooth(1).DataPoints.Take(8));

// Ronde 1 : 4 fenêtres de 2 matchs
var round1 = series.Decompose(2).Select(s => s.Statistics().Mean).ToList();
Console.WriteLine("Ronde 1 (KDA moyen par paire) :");
round1.ForEach(m => Console.WriteLine($"  {m:F2}"));

// Ronde 2 : les moyennes de ronde 1 sont des valeurs synthétiques — timestamps arbitraires
var round2 = DataSeries<double>.From(
    round1.Select((v, i) => new DataPoint<double>(new DateTime(2024, 1, i + 1), v))
).Decompose(1).Select(s => s.Statistics().Mean);
Console.WriteLine("Ronde 2 (KDA moyen par quart) :");
foreach (var m in round2) Console.WriteLine($"  {m:F2}");
```

---

## Étape 4 — Interface CLI

Ajouter `--bracket <n>` — le dernier flag de l'application.

**Avant de coder :** Comment enchaîner `Decompose` avec un `Select` pour calculer
la moyenne de chaque sous-série ? Quelle taille de segment proposer si `n` représente
le nombre total de matchs du bracket ?

```
dotnet run -- --bracket 8
dotnet run -- --bracket 8 --player Raphaël
```

<details>
<summary>Voir la solution</summary>

```csharp
if (args.Contains("--bracket"))
{
    int n = int.Parse(args[Array.IndexOf(args, "--bracket") + 1]);
    var round = kdaLea
        .Decompose(n / 4)
        .Select(s => s.Statistics().Mean)
        .ToList();

    Console.WriteLine($"Bracket ({n} matchs, {round.Count} segments) :");
    round.ForEach(m => Console.WriteLine($"  KDA moy : {m:F2}"));
}
```

</details>

---

Récapitulatif de tous les flags reconnus :

```
--help  --game  --player  --filter  --stat  --window
--rank  --export  --audit  --generate  --bracket
```

---

## Vérification

- `Decompose(minSize: 1)` sur 8 éléments → 8 séries de 1 élément
- `Decompose(minSize: 8)` sur 8 éléments → 1 série (cas de base immédiat)
- `Decompose(minSize: 2)` sur 8 éléments → 4 séries de 2 éléments
- L'arbre de décomposition est tracé dans les commentaires du code
