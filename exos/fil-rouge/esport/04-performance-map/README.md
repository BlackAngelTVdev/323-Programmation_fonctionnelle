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
La source n'est jamais modifiée.

```
[m1, m2, m3] → Transform(f) → [f(m1), f(m2), f(m3)]
```

---

## 4.1 — Calculer les KDA (ou tout autre indicateur) `.Transform(mapper)`

Nous voulons que notre librairie `DataSeries` offre la possibilité d'appliquer une transformation à tous les éléments d'une série, tout en conservant leurs timestamps.
Nous tenons au fait que la librairie reste générale (générique!) pour pouvoir être utilisée dans des domaines d'application différents les uns des autres.
Nous devons donc être capable de faire la transformation de ... n'importe quoi en ... n'importe quoi !
C'est mission impossible! Sauf si on nous fournit l'outil (la fonction) qui sait faire cette transformation.

**Avant de coder :**

<details>
<summary>Quelle méthode d'ordre supérieur de LINQ applique une fonction à chaque élément ?</summary>

`.Select(mapper)`

</details>
<details>
<summary>De quel type partons-nous et quel type obtient-on quand on s'intéresse au KDA ?</summary>

`ValorantMatch` -> `double`  
`Cs2Match` -> `double`  
`LolMatch` -> `double`

</details>
<details>
<summary>De quelle(s) manière(s) peut-on définir le mapper ?</summary>
Avec une lambda ou une fonction nommée
</details>

Avec les réponses en tête, on peut s'attaquer à

```csharp
public DataSeries<TResult> Transform<TResult>(Func<T, TResult> mapper)
{
    // appliquer mapper à chaque élément de _data et retourner une nouvelle DataSeries
    // ...
}
```

Allez-y et Calculez le KDA de tous les matches Valorant.
Chainez ça avec d'autres fonctions pour obtenir le KDA de tous les matches de Léa, de tous les matches gagnés, ...

Reproduire pour CS2 (Raphaël, Kiara) et LoL (Noé).

> Observation : `Transform` change le type — `DataSeries<ValorantMatch>` devient
> `DataSeries<double>`. La bibliothèque reste générique, le domaine est dans `EsportApp`.

> Le chaînage `Where(...).Transform(...)` est possible _uniquement_ parce que chaque méthode
> retourne un _nouvel_ objet au lieu de modifier la source. Immutabilité → composition.
> → [Composition de pipelines](../../../../supports/source/03-Map.md#composition-de-pipelines)

---

## 4.2 — Comparer entre différents jeux avec `.Normalize()`

On ne peut pas comparer des pommes et des poires, c'est bien connu.
Tout comme on ne peut pas comparer les stats de Valorant avec celles de Cs2 ou de Lol. Une valeur qui représent un super KDA dans Valorant peut paraître ridicule dans Cs2.
On veut donc que notre librairie soit capable de **normaliser** une série.

**Avant de coder :**

<details>
<summary>Que signifie normaliser une série entre 0 et 1 ?</summary>
Faire en sorte que la plus grande valeur de la série soit 1 et la plus petite 0
</details>

<details>
<summary>Quelle formule permet de ramener n'importe quelle valeur dans `[0, 1]` ?</summary>

`(valeur - min) / (max - min)` — le minimum devient 0, le maximum devient 1.

Cas particulier : si `max == min` (toutes les valeurs identiques), retourner 0 pour éviter une division par zéro.

</details>

On est prêts pour coder:

```csharp
// Evalue chaque objet de la série avec l'outil (fonction) d'évaluation fourni,
// et retourne une série de valeurs entre 0 et 1
public DataSeries<double> Normalize(Func<T, double> evaluator)
{
    // Evalue tous les éléments
    var values = ...;
    // Prend les valeurs extrêmes
    var min = ...;
    var max = ...;
    // et les utilise pour normaliser
    return new DataSeries<double> ...;
}
```

Comparer les KDA normalisés dans le programme :

```csharp
var kdaLeaNorm     = kdaLea.Normalize(...);
var kdaRaphaelNorm = kdaRaphael.Normalize(...);
var kdaNoeNorm     = kdaNoe.Normalize(...);
// Toutes les valeurs sont maintenant dans [0, 1]
```

---

## 4.3 — Lisser une courbe avec `.Smooth(windowSize)` et la closure

**Avant de coder :**

> la moyenne glissante d'indice `i` avec une fenêtre de taille `w` utilise les éléments aux indices `[i-w+1 .. i]`.

<details>
<summary>Comment générer tous les indices pertinents avec LINQ ?</summary>

`Range(w, _data.Count()-w)`

</details>

Encore un

<details>
<summary>Indice sur la structure</summary>
Vous aurez certainement recours à :

```
Enumerable.Range(x,y)
Skip(n)
Take(n)
Average()
```

Allez les voir dans la cheatsheet.

</details>

Et c'est à vous de jouer...

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

## 4.4 — Interface CLI

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
collection _imbriquée_ — `Select` produirait une séquence de séquences.

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
