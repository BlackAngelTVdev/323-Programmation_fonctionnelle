# Exercice 03 — Valider les stats

## Concepts théoriques

- [Thématique 02 — Filter et fonctions d'ordre supérieur](../../../../thematiques/02-filter-fonctions-sup.md)
- [Fonctions d'ordre supérieur](../../../../supports/source/02a-fonctions-sup.md)
- [Filter et prédicats](../../../../supports/source/02b-filter.md)
- [Closures](../../../../supports/source/02a-fonctions-sup.md#closures-captures-de-variables)
- [Évaluation paresseuse](../../../../supports/source/02b-filter.md#evaluation-paresseuse-deferred-execution)

## Contexte

Le dataset contient maintenant les données réelles (CSV) et les données générées (exercice 02).  
Avant toute analyse, il faut valider que les contraintes sont respectées dans les trois sources.

<hr>

## Étape 1 — Implémenter `.Outliers(predicate)`

On appelle "Outlier" une valeur aberrante, impossible dans une série. Si on a par exemple une série de mesures de la température du lac, la valeur "234" est un outlier.  
Le but de cette méthode est de montrer les outliers. On lui passe une fonction qui détermine si une valeur est "outlier" ou pas.

**Attention :** `Outliers` doit retourner une nouvelle `DataSeries<T>`, sans modifier la série.

<details>
<summary>Voir la solution</summary>

```csharp
public DataSeries<T> Outliers(Func<T, bool> predicate)
    => DataSeries<T>.From(_data.Where(predicate));
```

</details>

Observer que `Filter` retourne une **nouvelle** `DataSeries<T>` — la source `_data` n'est
jamais modifiée. C'est l'immuabilité : chaque appel produit un nouvel objet.

Vérifier dans `Program.cs` :

```csharp
var baaad = valorant.Outliers(m => m.Value.Kills < 0);
Console.WriteLine(valorant.Count); // 25 — inchangé
Console.WriteLine(baaad.Count);     // sous-ensemble
```

<hr>

## Étape 2 — Implémenter `.Sanitize(predicate)`

Cette méthode nettoie une série en enlevant les outliers.

Il y a plusieurs manières de réaliser `Sanitize`. Comparez vos solutions entre vous.  
Quelqu'un a-t-il utilisé `Outliers` pour coder `Sanitize` ? 

Appliquer à chaque jeu pour éliminer les valeurs impossibles :

```csharp
// Valorant : kills plausibles pour un match compétitif
valorant.Sanitize(m =>
    m.Kills   < 0 || m.Kills > 50 ||
    m.Deaths  < 0 || m.Deaths >30 ||
    m.Assists < 0
);

// CS2 : contraintes similaires
cs2.Sanitize(m =>
    m.Kills + m.Assists > 50 ||
    m.Deaths < 0
);

// LoL : le support a structurellement peu de kills
lol.Sanitize(m =>
    m.Kills   > 10 ||
    m.Deaths  < 1  ||
    m.Assists < 0  ||
    m.Cs      < 0
);
```
<hr>

## Étape 3 — Interface CLI

Ajouter les flags `--player <nom>` et `--filter wins|losses|all`.
Ces deux flags se combinent avec `--game` introduit en exercice 01.

Ajouter un flag `--error [strict | soft]` : en strict, on affiche les outliers et on s'arrête. En soft, on les élimine et on continue

**Avant de coder :** Si `--player` est absent, que filtrer ? Si `--filter` vaut `"all"`,
faut-il appliquer un prédicat ? Plutôt qu'un if/else par mode, que gagne-t-on à stocker
les prédicats dans un **dictionnaire** ? Que faut-il faire pour ajouter un critère
`--filter close` (matchs serrés) ?

<details>
<summary>Indice — dispatch fonctionnel</summary>

Une fonction est une valeur : elle peut être la *valeur* d'un dictionnaire.
`Dictionary<string, Func<ValorantMatch, bool>>` associe chaque mode CLI à son prédicat —
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
var filters = new Dictionary<string, Func<ValorantMatch, bool>>
{
    ["wins"]   = m => m.Won,
    ["losses"] = m => !m.Won,
    ["all"]    = m => true,
};

var result = valorant.Filter(filters[filterMode]);
```

Ajouter un critère = ajouter **une ligne dans la table, zéro if**. La fonction choisie
à l'exécution est une valeur comme une autre.

`--player` et `--filter` s'enchaînent naturellement : `Filter` retourne une `DataSeries` —
composabilité des flags = composabilité du pipeline.

</details>

---

## Vérification

- `valorant.Count` reste 25 après `Filter` (immuabilité)
- `RemoveOutliers` sur les données réelles ne retire aucun match (données déjà propres)
- `RemoveOutliers` sur les données générées (exercice 02) retire quelques matchs impossibles
- L'observation de la paresse confirme que le prédicat n'est pas appelé avant matérialisation
