# Exercice 03 — Valider les stats

> Partie 2 — `.Filter()` + `.RemoveOutliers()` + `.HasAny()` / `.AllMatch()` + évaluation paresseuse

## Concepts théoriques

- [Thématique 02 — Filter et fonctions d'ordre supérieur](../../../../thematiques/02-filter-fonctions-sup.md)
- [Fonctions d'ordre supérieur](../../../../supports/source/02a-fonctions-sup.md)
- [Filter et prédicats](../../../../supports/source/02b-filter.md)
- [Closures](../../../../supports/source/02a-fonctions-sup.md#closures-captures-de-variables)
- [Évaluation paresseuse](../../../../supports/source/02b-filter.md#evaluation-paresseuse-deferred-execution)

## Contexte

Le dataset contient maintenant les données réelles (CSV) et les données générées (exercice 02).
Avant toute analyse, il faut valider que les contraintes sont respectées dans les trois sources.

---

## Étape 1 — Implémenter `.Filter(predicate)`

**Avant de coder :** `Filter` doit retourner un nouveau `MatchSeries`, pas modifier l'existant.
Quelle méthode LINQ applique un prédicat à une séquence ?

<details>
<summary>Indice</summary>

`Where(predicate)` filtre une `IEnumerable<T>` sans modifier la source.
Il suffit d'envelopper le résultat dans un nouveau `MatchSeries`.

</details>

```csharp
public MatchSeries Filter(Func<IMatchData, bool> predicate)
{
    // retourner un nouveau MatchSeries contenant seulement les éléments qui satisfont le prédicat
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public MatchSeries Filter(Func<IMatchData, bool> predicate)
    => new MatchSeries(_data.Where(predicate));
```

</details>

Observer que `Filter` retourne un **nouveau** `MatchSeries` — la source `_data` n'est
jamais modifiée. C'est l'immuabilité : chaque appel produit un nouvel objet.

Vérifier dans `Program.cs` :

```csharp
var wins = valorant.Filter(m => m.Won);
Console.WriteLine(valorant.Count); // 25 — inchangé
Console.WriteLine(wins.Count);     // sous-ensemble
```

**Les prédicats sont des valeurs.** Plutôt que d'écrire les lambdas en ligne, les déclarer,
les nommer et les combiner comme n'importe quelle variable :

```csharp
Func<IMatchData, bool> isWin       = m => m.Won;
Func<IMatchData, bool> isHighScore = m => m.Kills > 20;

// Combinaison : un nouveau prédicat construit à partir des deux autres
Func<IMatchData, bool> isCrushingWin = m => isWin(m) && isHighScore(m);

var top = valorant.Filter(isCrushingWin);
```

Une fonction stockée dans une variable se passe, se combine, se réutilise —
→ [Fonctions comme valeurs](../../../../supports/source/02a-fonctions-sup.md)

---

## Étape 2 — Implémenter `.RemoveOutliers(isValid)`

**Avant de coder :** quelle est la différence entre `Filter` et `RemoveOutliers` ?
Peut-on éviter de dupliquer du code ?

<details>
<summary>Indice</summary>

`RemoveOutliers(isValid)` garde les éléments valides — c'est exactement `Filter(isValid)`.
Une méthode peut déléguer à une autre méthode de la même classe.

</details>

```csharp
public MatchSeries RemoveOutliers(Func<IMatchData, bool> isValid)
{
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public MatchSeries RemoveOutliers(Func<IMatchData, bool> isValid)
    => Filter(isValid);
```

</details>

Appliquer à chaque jeu pour éliminer les valeurs impossibles :

```csharp
// Valorant : kills plausibles pour un match compétitif
var valorantValid = valorant.RemoveOutliers(m =>
    m.Kills   >= 0 && m.Kills   <= 50 &&
    m.Deaths  >= 1 && m.Deaths  <= 30 &&
    m.Assists >= 0
);

// CS2 : contraintes similaires
var cs2Valid = cs2.RemoveOutliers(m =>
    m.Kills + m.Assists <= 50 &&
    m.Deaths >= 1
);

// LoL : le support a structurellement peu de kills
var lolValid = lol.RemoveOutliers(m =>
    m.Kills   <= 10 &&
    m.Deaths  >= 1  &&
    m.Assists >= 0
);
```

---

## Étape 3 — `.HasAny()` et `.AllMatch()`

**Avant de coder :** quelles méthodes LINQ répondent à "au moins un" et "tous" ?

<details>
<summary>Indice</summary>

`Any(predicate)` et `All(predicate)` — elles retournent un `bool`.

</details>

```csharp
public bool HasAny(Func<IMatchData, bool> predicate)  => // ...
public bool AllMatch(Func<IMatchData, bool> predicate) => // ...
```

<details>
<summary>Voir la solution</summary>

```csharp
public bool HasAny(Func<IMatchData, bool> predicate)
    => _data.Any(predicate);

public bool AllMatch(Func<IMatchData, bool> predicate)
    => _data.All(predicate);
```

</details>

Utilisation :

```csharp
Console.WriteLine(valorantValid.HasAny(m => m.Kills > 20));
// → Léa a-t-elle au moins un match avec plus de 20 kills ?

Console.WriteLine(lolValid.AllMatch(m => m.Deaths >= 1));
// → Tous les matchs de Noé ont-ils au moins 1 mort ?
```

→ Théorie : [Any et All — prédicats HOF](../../../../supports/source/02b-filter.md#any-et-all-—-predicats-hof-booleens)

---

## Évaluation paresseuse — observation

`Where` (LINQ) est **paresseux** : il ne parcourt rien à l'appel, il décrit l'opération.
Mais `MatchSeries` modifie ce comportement. Deux expériences pour l'observer.

### Expérience 1 — Quand le prédicat s'exécute-t-il ?

```csharp
var filtered = valorant.Filter(m =>
{
    Console.WriteLine($"Évaluation de {m.Player}");
    return m.Won;
});
Console.WriteLine("--- après Filter ---");
_ = filtered.Count;
Console.WriteLine("--- après Count ---");
```

**Résultat attendu (intuition LINQ pure) :** les lignes "Évaluation de…" apparaissent après `Count`.

<details>
<summary>Résultat réel — pourquoi ?</summary>

Les lignes s'affichent **avant** "--- après Filter ---".
Le constructeur de `MatchSeries` appelle `ToList()` en interne : la matérialisation est immédiate.
`filtered.Count` n'exécute rien de plus — la série est déjà calculée.

</details>

### Expérience 2 — Mutation de la source après construction

```csharp
var source = valorant.ToList();
var series = MatchSeries.From(source);
var filtered = series.Filter(m => m.Won);

int countBefore = filtered.Count;
source.Clear();
int countAfter = filtered.Count;

Console.WriteLine(countBefore == countAfter); // vrai ou faux ?
```

**Résultat attendu (intuition LINQ pure) :** `false` — la série refléterait la source vidée.

<details>
<summary>Résultat réel — pourquoi ?</summary>

`true` — `filtered` est un **instantané** figé à sa création.
La mutation de `source` est sans effet sur la série.
C'est un choix délibéré : l'immuabilité est garantie dès la construction.

</details>

### Comment préserver l'exécution différée ?

<details>
<summary>Voir</summary>

Stocker `IEnumerable<IMatchData>` au lieu de `List<IMatchData>` en interne suffirait :

```csharp
// Version paresseuse — chaque accès réévalue le prédicat
private readonly IEnumerable<IMatchData> _data; // au lieu de List<IMatchData>
```

Le pipeline resterait lazy mais la série ne serait plus un instantané :
toute mutation de la source se répercuterait à chaque accès.
Deux stratégies légitimes selon le contexte.

</details>

---

## Étape 4 — Interface CLI

Ajouter les flags `--player <nom>` et `--filter wins|losses|all`.
Ces deux flags se combinent avec `--game` introduit en exercice 01.

**Avant de coder :** Si `--player` est absent, que filtrer ? Si `--filter` vaut `"all"`,
faut-il appliquer un prédicat ? Plutôt qu'un if/else par mode, que gagne-t-on à stocker
les prédicats dans un **dictionnaire** ?

<details>
<summary>Indice — dispatch fonctionnel</summary>

Une fonction est une valeur : elle peut être la *valeur* d'un dictionnaire.
`Dictionary<string, Func<IMatchData, bool>>` associe chaque mode CLI à son prédicat —
le if/else disparaît.

</details>

<details>
<summary>Voir la solution</summary>

```csharp
string? player = args.Contains("--player")
    ? args[Array.IndexOf(args, "--player") + 1]
    : null;

string filterMode = args.Contains("--filter")
    ? args[Array.IndexOf(args, "--filter") + 1]
    : "all";

// Table de prédicats — le mode CLI sélectionne une fonction
var filters = new Dictionary<string, Func<IMatchData, bool>>
{
    ["wins"]   = m => m.Won,
    ["losses"] = m => !m.Won,
    ["all"]    = m => true,
};

var series = player == null ? valorant : valorant.Filter(m => m.Player == player);
var result = series.Filter(filters[filterMode]);
Console.WriteLine($"Résultats : {result.Count} matchs");
```

Ajouter un critère = ajouter **une ligne dans la table, zéro if**. La fonction choisie
à l'exécution est une valeur comme une autre.

`--player` et `--filter` s'enchaînent naturellement : `Filter` retourne un `MatchSeries` —
composabilité des flags = composabilité du pipeline.

</details>

---

## Vérification

- `valorant.Count` reste 25 après `Filter` (immuabilité)
- `RemoveOutliers` sur les données réelles ne retire aucun match (données déjà propres)
- `RemoveOutliers` sur les données générées (exercice 02) retire quelques matchs impossibles
- Les deux expériences confirment que `MatchSeries` matérialise immédiatement (snapshot) — contrairement à un pipeline LINQ pur
