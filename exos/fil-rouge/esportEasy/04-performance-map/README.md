# Exercice 04 — Calculer le KDA par joueur

> Partie 3 — `.Extract()` → `StatSeries` + `.Normalize()` + `.Smooth()`

## Concepts théoriques

- [Thématique 03 — Map et transformation](../../../../thematiques/03-map-transformation.md)
- [Map / Select](../../../../supports/source/03-Map.md)
- [Composition de pipelines](../../../../supports/source/03-Map.md#composition-de-pipelines)
- [Closures dans les transformations](../../../../supports/source/03-Map.md#closures-dans-les-transformations)

## Contexte

Comparer les 5 joueurs de Team Helvetia sur un même indicateur est complexe :
chaque jeu a son propre système de scoring. Le KDA (Kills + Assists) / Deaths est
la métrique commune qui permet la comparaison inter-jeux.

Normaliser le KDA permet ensuite de le comparer à des séries d'autres natures
(ex. : vision score de Noé vs headshots de Léa).

---

## Concept FP : Map = transformer sans modifier

`.Extract()` applique une fonction à **chaque match** et retourne une **nouvelle série de nombres**.
La source n'est jamais modifiée — même principe que `.Filter()`.

```
[m1, m2, m3] → Extract(selector) → [selector(m1), selector(m2), selector(m3)]
```

---

## Étape 1 — Créer `StatSeries` et implémenter `.Extract(selector)`

**Avant de coder :** après extraction, on travaille sur des doubles (KDA, kills, assists...).
Il faut un type séparé pour ces séries numériques. Pourquoi ne pas réutiliser `MatchSeries` ?

<details>
<summary>Indice</summary>

`MatchSeries` stocke des `IMatchData` — des objets. Après extraction, on a des `double`
avec leurs dates associées. Un type dédié, `StatSeries`, représente cette réalité sans ambiguïté.
En esport, `DataSeries<double>` remplissait ce rôle grâce à la généricité.

</details>

Créer `DataSeries/StatSeries.cs` :

```csharp
public class StatSeries
{
    private readonly IEnumerable<(DateTime Date, double Value)> _data;

    private StatSeries(IEnumerable<(DateTime, double)> data) => _data = data;

    public static StatSeries From(IEnumerable<(DateTime, double)> source) => new(source);

    public int Count                                          => _data.Count();
    public IEnumerable<double> Values                        => _data.Select(d => d.Value);
    public IEnumerable<(DateTime Date, double Value)> DataPoints => _data;

    public StatSeries Filter(Func<double, bool> predicate)
        => new StatSeries(_data.Where(d => predicate(d.Value)));
}
```

Ajouter la méthode `Extract` dans `MatchSeries.cs` :

```csharp
public StatSeries Extract(Func<IMatchData, double> selector)
{
    // appliquer selector à chaque match et conserver la date
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public StatSeries Extract(Func<IMatchData, double> selector)
    => StatSeries.From(_data.Select(m => (m.Date, selector(m))));
```

La date vient directement de `m.Date` — pas besoin d'un `DataPoint<T>` wrapper.
Le sélecteur produit toujours un `double` — c'est la limite d'`Extract` vs `Transform<TResult>`.

</details>

Calculer le KDA pour Valorant et chaîner avec Filter :

```csharp
var kdaLea = valorant
    .Filter(m => m.Player == "Léa")
    .Extract(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths));

Console.WriteLine(string.Join(", ", kdaLea.Values.Select(v => v.ToString("F2"))));
```

Reproduire pour CS2 (Raphaël, Kiara) et LoL (Noé).

> **Observation :** `Extract` change le type — `MatchSeries` devient `StatSeries`.
> La bibliothèque reste dans son domaine (dates + doubles), le calcul reste dans `EsportApp`.

> **Limitation par rapport à esport :** `Extract` ne peut produire que des `double`.
> `Transform<TResult>` dans le fil rouge esport peut produire n'importe quel type.
> Par exemple, `Transform(m => m.Player)` → `DataSeries<string>` est possible en esport ;
> ce n'est pas faisable avec `Extract`. C'est le trade-off choisi : moins flexible,
> mais types plus lisibles.

> Le chaînage `Filter(...).Extract(...)` est possible *uniquement* parce que chaque méthode
> retourne un *nouvel* objet au lieu de modifier la source. Immutabilité → composition.
> → [Composition de pipelines](../../../../supports/source/03-Map.md#composition-de-pipelines)

---

## Étape 2 — `.Normalize()` — comparer entre jeux

**Avant de coder :** que signifie normaliser une série entre 0 et 1 ?
Quelle formule permet de ramener n'importe quelle valeur dans `[0, 1]` ?

<details>
<summary>Indice sur la formule</summary>

`(valeur - min) / (max - min)` — le minimum devient 0, le maximum devient 1.
Cas particulier : si `max == min` (toutes les valeurs identiques), retourner 0 pour éviter une division par zéro.

</details>

```csharp
public StatSeries Normalize()
{
    var points = // ...
    var min    = // ...
    var max    = // ...
    var range  = // ...
    return StatSeries.From(
        points.Select(d => (d.Date, /* formule de normalisation */))
    );
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public StatSeries Normalize()
{
    var points = _data.ToList();
    var min    = points.Min(d => d.Value);
    var max    = points.Max(d => d.Value);
    var range  = max - min;
    return StatSeries.From(
        points.Select(d => (d.Date, range == 0 ? 0.0 : (d.Value - min) / range))
    );
}
```

Les dates sont préservées — la série normalisée reste temporelle.

</details>

Comparer les KDA normalisés :

```csharp
var kdaLeaNorm     = kdaLea.Normalize();
var kdaRaphaelNorm = kdaRaphael.Normalize();
var kdaNoeNorm     = kdaNoe.Normalize();
// Toutes les valeurs sont maintenant dans [0, 1]
```

---

## Étape 3 — `.Smooth(windowSize)` et la closure

**Avant de coder :** la moyenne glissante d'indice `i` avec une fenêtre de taille `w`
utilise les éléments aux indices `[i-w+1 .. i]`. Comment générer tous les indices avec LINQ ?

<details>
<summary>Indice sur la structure</summary>

`Enumerable.Range(0, points.Count)` génère tous les indices.
Pour chaque indice `i`, prendre `points.Skip(Max(0, i - w + 1)).Take(w)` puis `.Average(d => d.Value)`.
La variable `windowSize` capturée par le lambda est une **closure** — observer ce que ça implique.
→ [Closures dans les transformations](../../../../supports/source/03-Map.md#closures-dans-les-transformations)

</details>

```csharp
public StatSeries Smooth(int windowSize)
{
    var points = _data.ToList();
    return StatSeries.From(
        Enumerable.Range(0, points.Count)
            .Select(i =>
            {
                // extraire la fenêtre autour de i et calculer la moyenne
                // ...
            })
    );
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public StatSeries Smooth(int windowSize)
{
    var points = _data.ToList();
    return StatSeries.From(
        Enumerable.Range(0, points.Count)
            .Select(i =>
            {
                var window = points.Skip(Math.Max(0, i - windowSize + 1)).Take(windowSize);
                return (points[i].Date, window.Average(d => d.Value));
            })
    );
}
```

</details>

Observer la closure :

```csharp
int window = 3;
var smoothed = kdaLea.Smooth(window);
window = 10; // Sans effet — window a été copiée à l'appel de Smooth (passage d'argument)
```

> Attention à la nuance : une variable **capturée** par un lambda l'est **par référence** —
> sa modification ultérieure serait visible. Ici `window` n'est pas capturée : elle est
> passée en argument à `Smooth`, donc copiée. C'est `windowSize` (le paramètre) que le
> lambda capture, et il ne change plus.
> → [Closures](../../../../supports/source/02a-fonctions-sup.md#closures-captures-de-variables)

---

## Étape 4 — Interface CLI

Ajouter `--stat kda|kills|assists` pour choisir la transformation à afficher.

**Avant de coder :** Comment mapper une valeur de flag (`"kda"`, `"kills"`, `"assists"`) à une
transformation différente ? Plutôt qu'une chaîne de `if/else`, que permet un **dictionnaire de fonctions** ?

<details>
<summary>Indice — table de sélecteurs</summary>

Comme la table de prédicats de l'exercice 03 : un sélecteur `Func<IMatchData, double>`
est une valeur — il peut être stocké dans un `Dictionary` et choisi à l'exécution.
→ [Fonctions comme valeurs](../../../../supports/source/02a-fonctions-sup.md)

</details>

<details>
<summary>Voir la solution</summary>

```csharp
string stat = args.Contains("--stat")
    ? args[Array.IndexOf(args, "--stat") + 1]
    : "kda";

// Table de sélecteurs — le flag CLI choisit la fonction d'extraction
var selectors = new Dictionary<string, Func<IMatchData, double>>
{
    ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
    ["kills"]   = m => m.Kills,
    ["assists"] = m => m.Assists,
};

if (!selectors.ContainsKey(stat))
    throw new ArgumentException($"Stat inconnue : {stat}");

StatSeries values = valorant.Filter(m => m.Player == "Léa").Extract(selectors[stat]);
```

La fonction choisie à l'exécution est une valeur comme une autre — ajouter une stat =
une ligne dans la table, et l'appel à `Extract` ne change pas.

</details>

---

## Étape bonus (avancé) — SelectMany

> Étape optionnelle — pour aller plus loin.

Les KDA sont calculés par jeu, mais le coaching staff veut la liste **plate** de tous les
KDA de l'équipe, tous jeux confondus.

```csharp
var allSeries = new[] { kdaLea, kdaRaphael, kdaNoe, kdaDylan, kdaKiara };

// SelectMany aplatit la collection de séries en une séquence de doubles
var allKda = allSeries.SelectMany(s => s.Values);

Console.WriteLine($"KDA de l'équipe entière : {allKda.Count()} valeurs");
```

→ [SelectMany — le flatMap](../../../../supports/source/03-Map.md#selectmany-—-le-flatmap)

---

## Vérification

- `kdaLea.Count` = 13 (matchs de Léa uniquement)
- Valeurs normalisées dans [0.0, 1.0] — min = 0.0, max = 1.0 exactement
- `Smooth(1)` ne change rien (fenêtre = 1 = identité)
- `Smooth(3)` réduit les écarts entre valeurs consécutives
- `valorant.Count` reste 25 après toutes les transformations (immuabilité)
