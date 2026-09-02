# Exercice 04 — Calculer le KDA par joueur

> Partie 3 — `.Transform()` + `.Normalize()` + `.Smooth()`

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

`.Transform()` applique une fonction à **chaque élément** et retourne une **nouvelle série**.
La source n'est jamais modifiée — même principe que `.Filter()`.

```
[m1, m2, m3] → Transform(f) → [f(m1), f(m2), f(m3)]
```

---

## Étape 1 — Implémenter `.Transform(mapper)`

**Avant de coder :** `Transform` doit changer le type — passer de `DataSeries<ValorantMatch>`
à `DataSeries<double>`. Quelle méthode LINQ applique une fonction à chaque élément ?

<details>
<summary>Indice</summary>

`Select(mapper)` applique une fonction à chaque élément et retourne une nouvelle séquence.
La méthode doit être générique : `Transform<TResult>(Func<T, TResult> mapper)`.

</details>

```csharp
public DataSeries<TResult> Transform<TResult>(Func<T, TResult> mapper)
{
    // appliquer mapper à chaque élément de _data et retourner une nouvelle DataSeries
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public DataSeries<TResult> Transform<TResult>(Func<T, TResult> mapper)
    => DataSeries<TResult>.From(_data.Select(mapper));
```

</details>

Calculer le KDA pour Valorant et chaîner avec Filter :

```csharp
var kdaLea = valorant
    .Filter(m => m.Player == "Léa")
    .Transform(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths));

Console.WriteLine(string.Join(", ", kdaLea.Values.Select(v => v.ToString("F2"))));
```

Reproduire pour CS2 (Raphaël, Kiara) et LoL (Noé).

> Observation : `Transform` change le type — `DataSeries<ValorantMatch>` devient
> `DataSeries<double>`. La bibliothèque reste générique, le domaine est dans `EsportApp`.

> Le chaînage `Filter(...).Transform(...)` est possible *uniquement* parce que chaque méthode
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
public DataSeries<double> Normalize()
{
    var values = _data.Cast<double>().ToList();
    var min    = // ...
    var max    = // ...
    var range  = // ...
    return DataSeries<double>.From(
        values.Select(v => /* formule de normalisation */)
    );
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public DataSeries<double> Normalize()
{
    var values = _data.Cast<double>().ToList();
    var min    = values.Min();
    var max    = values.Max();
    var range  = max - min;
    return DataSeries<double>.From(
        values.Select(v => range == 0 ? 0.0 : (v - min) / range)
    );
}
```

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

`Enumerable.Range(0, values.Count)` génère tous les indices.
Pour chaque indice `i`, prendre `values.Skip(Max(0, i - w + 1)).Take(w)` puis `.Average()`.
La variable `windowSize` capturée par le lambda est une **closure** — observer ce que ça implique.
→ [Closures dans les transformations](../../../../supports/source/03-Map.md#closures-dans-les-transformations)

</details>

```csharp
public DataSeries<double> Smooth(int windowSize)
{
    var values = _data.Cast<double>().ToList();
    return DataSeries<double>.From(
        Enumerable.Range(0, values.Count)
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
public DataSeries<double> Smooth(int windowSize)
{
    var values = _data.Cast<double>().ToList();
    return DataSeries<double>.From(
        Enumerable.Range(0, values.Count)
            .Select(i =>
            {
                var window = values.Skip(Math.Max(0, i - windowSize + 1)).Take(windowSize);
                return window.Average();
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
transformation différente ? Plutôt qu'une chaîne de `if/else` (ou même un `switch`),
que permet un **dictionnaire de fonctions** ? Que faire si la valeur passée est inconnue ?

<details>
<summary>Indice — table de sélecteurs</summary>

Comme la table de prédicats de l'exercice 03 : un sélecteur `Func<ValorantMatch, double>`
est une valeur — il peut être stocké dans un `Dictionary` et choisi à l'exécution.
→ [Fonctions comme valeurs](../../../../supports/source/02a-fonctions-sup.md)

</details>

<details>
<summary>Voir la solution</summary>

```csharp
string stat = args.Contains("--stat")
    ? args[Array.IndexOf(args, "--stat") + 1]
    : "kda";

// Table de sélecteurs — le flag CLI choisit la fonction de transformation
var selectors = new Dictionary<string, Func<ValorantMatch, double>>
{
    ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
    ["kills"]   = m => m.Kills,
    ["assists"] = m => m.Assists,
};

if (!selectors.ContainsKey(stat))
    throw new ArgumentException($"Stat inconnue : {stat}");

DataSeries<double> values = valorantSeries.Transform(selectors[stat]);
```

La fonction choisie à l'exécution est une valeur comme une autre — ajouter une stat =
une ligne dans la table, et l'appel à `Transform` ne change pas.

</details>

---

## Étape bonus (avancé) — SelectMany

> Étape optionnelle — pour aller plus loin.

Les KDA sont calculés par jeu, mais le coaching staff veut la liste **plate** de tous les
KDA de l'équipe, tous jeux confondus. Le problème : une collection de séries est une
collection *imbriquée* — `Select` produirait une séquence de séquences.

```csharp
var allSeries = new[] { kdaLea, kdaRaphael, kdaNoe, kdaDylan, kdaKiara };

// Select → IEnumerable<IEnumerable<double>> (imbriqué)
// SelectMany → IEnumerable<double> (aplati)
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
