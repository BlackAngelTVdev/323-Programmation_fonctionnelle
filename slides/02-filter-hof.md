---
theme: default
title: "Fonctions d'ordre supérieur & Filter"
info: "Action, Func, lambdas, captures, Where, évaluation paresseuse"
author: ETML
transition: slide-left
mdc: true
---

# Fonctions d'ordre supérieur & Filter

## Les fonctions comme valeurs

<div class="pt-12">
  <span class="px-2 py-1 rounded bg-blue-500 text-white">
    Thématique 02
  </span>
</div>

---

# Tu as 5 critères de filtre différents.
# Tu vas copier-coller la même boucle 5 fois ?

<v-clicks>

```csharp
// Pour chaque critère : réécrire toute la boucle
foreach (var p in players) if (p.Kda > 2)   result1.Add(p);
foreach (var p in players) if (p.Rank > 10)  result2.Add(p);
foreach (var p in players) if (p.Games > 50) result3.Add(p);
// ... etc.
```

```csharp
// Ou : passer le critère en paramètre — une seule boucle
players.Where(p => p.Kda > 2)    // [Léa, Raph]
players.Where(p => p.Rank > 10)  // [Raph, Sam]
players.Where(p => p.Games > 50) // [Léa]
```

<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Le filtre est **une valeur qu'on passe en paramètre** — c'est le principe des fonctions d'ordre supérieur.

</div>

</v-clicks>

---

# Plan

<v-clicks>

1. **Action et Func** — les types des fonctions en C#
2. **Lambdas** — créer une fonction à la volée avec `=>`
3. **Captures** — un lambda avec mémoire
4. **Filter / Where** — tamis sur une collection
5. **Évaluation paresseuse** — l'exécution différée
6. **Any / All** — prédicats booléens

</v-clicks>

---
layout: section
---

# Partie 1
## Action et Func

---

# Deux types pour les fonctions

<v-clicks>

```csharp {1-6}
// Action = fonction SANS valeur de retour
// Action<A,B,...> → les paramètres entre chevrons
void LogTo(Action<string> output)
{
    output("Match terminé !");  // → affiche ou écrit dans un fichier
}
```

```csharp {1-7}
// Func = fonction AVEC valeur de retour
// Func<A,B,...,TResult> → le DERNIER type est le retour
void Compute(int a, int b, Func<int, int, double> operation)
{
    Console.WriteLine(operation(a, b));
}
Compute(10, 2, (a, b) => (double)a / b); // → 5
```

<div class="mt-1 p-1 bg-blue-800 rounded text-blue-100">

`Action` = void | `Func` = retourne quelque chose (dernier type = retour)

</div>

</v-clicks>

---

# La puissance : changer de comportement sans changer le code

```csharp {1-3|5-7|9-11|all}
// Plusieurs implémentations du même contrat
void ToFile(string text)    => File.AppendAllText("log.txt", text);
void ToConsole(string text) => Console.WriteLine(text);

// Action<string> : n'importe quelle fonction qui prend un string
Action<string> Log = ToFile;      // ← changer ici seulement
// Action<string> Log = ToConsole;

// Partout dans l'app — même appel, comportement différent
Log("Démarrage");        // → dans le fichier
Log("Erreur critique !"); // → dans le fichier
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Changer d'implémentation sans toucher au code qui l'utilise : c'est la flexibilité des fonctions d'ordre supérieur.

</div>
</v-click>

---
layout: section
---

# Partie 2
## Lambdas

---

# Créer une fonction à la volée

> Paramètres **à gauche** de `=>` — corps **à droite**

<v-clicks>

```csharp {1|2|3|4|all}
Func<int>          one = () => 1;             // () → pas de param → 1
Func<int, int>     x2  = x => x * 2;          // x → x × 2
Func<int, int, int> add = (x, y) => x + y;   // x,y → x + y
Func<int, bool>    isE  = x => x % 2 == 0;   // x → true si pair
```

```csharp
// Corps multi-lignes avec accolades
Action<int> print = x =>
{
    Console.WriteLine($"Valeur : {x}");
};
print(42); // → Valeur : 42
```

</v-clicks>

---

# Lambdas + Where : le critère comme valeur

```csharp {1-4|6-8|all}
List<int> numbers = new() { 1, 2, 3, 4, 5 };

Func<int, bool> isEven = x => x % 2 == 0;
Func<int, bool> isBig  = x => x > 2;

numbers.Where(isEven).ToList(); // → [2, 4]
numbers.Where(isBig).ToList();  // → [3, 4, 5]
// La méthode Where est la même — seul le critère change
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

`Where` est une fonction d'ordre supérieur : elle reçoit une fonction (le prédicat) en argument.

</div>
</v-click>

---
layout: section
---

# Partie 3
## Captures — un lambda avec mémoire

---

# Un lambda peut utiliser des variables de son entourage

```csharp {1|3-4|6|all}
int minKda = 2;

Func<Player, bool> isGood = p => p.Kda >= minKda; // utilise minKda
// isGood ne reçoit pas minKda en paramètre — il le trouve autour de lui

players.Where(isGood).ToList(); // → [Léa(kda=2.4), Raph(kda=3.1)]
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

On dit que le lambda **capture** la variable `minKda`. Il ne reçoit pas une copie de sa valeur — il garde un **lien direct** vers la variable elle-même.

</div>
</v-click>

---

# Le lien, pas la copie — attention !

```
minKda ──── [boîte mémoire : 2]
               ↑
isGood ────────┘   ← le lambda pointe vers la boîte, pas vers "2"
```

<v-clicks>

```csharp
int minKda = 2;
Func<Player, bool> isGood = p => p.Kda >= minKda;

players.Where(isGood).ToList();
// → [Léa(2.4), Raph(3.1)]  ← filtré avec minKda = 2

minKda = 3; // la boîte mémoire change

players.Where(isGood).ToList();
// → [Raph(3.1)]  ← filtré avec minKda = 3 !
```

<div class="mt-1 p-1 bg-yellow-600 rounded text-yellow-200">

Si la variable capturée change **après** la création du lambda mais **avant** son exécution, le lambda voit la nouvelle valeur.

</div>

</v-clicks>

---

# Captures utiles : créer des règles à la demande

```csharp {1-3|5-8|10|all}
// Un générateur qui fabrique des règles différentes
Func<int, Func<Player, bool>> minKdaFilter =
    min => p => p.Kda >= min;

var topPlayers  = minKdaFilter(3);  // règle : kda >= 3, capture min=3
var goodPlayers = minKdaFilter(2);  // règle : kda >= 2, capture min=2
var anyPlayer   = minKdaFilter(0);  // règle : kda >= 0, capture min=0

players.Where(topPlayers).ToList();  // → [Raph(3.1)]
players.Where(goodPlayers).ToList(); // → [Léa(2.4), Raph(3.1)]
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Créer des fonctions paramétrées à la volée — sans écrire une nouvelle classe à chaque fois.

</div>
</v-click>

---
layout: section
---

# Partie 4
## Filter — Where

---

# Filter : un tamis sur la collection

```
Entrée   [Léa(2.4), Sam(1.2), Raph(3.1), Kai(0.8)]
              ↓        ✗          ↓         ✗       prédicat : kda > 2
Sortie        [Léa(2.4),          Raph(3.1)]
```

<v-click>

```csharp
var topPlayers = players.Where(p => p.Kda > 2).ToList();
// → [Léa(kda=2.4), Raph(kda=3.1)]
```

</v-click>

---

# Combiner plusieurs filtres

```csharp {1-3|5-7|9-13|all}
// Chaîner deux Where
players.Where(p => p.Kda > 2).Where(p => p.Games > 10).ToList();
// → [Raph]

// Condition logique inline
players.Where(p => p.Kda > 2 && p.Games > 10).ToList();
// → [Raph]

// Corps multi-lignes si logique complexe
players.Where(p => {
    return p.Kda > 2 && p.Games > 10;
}).ToList();
// → [Raph]
```

---
layout: section
---

# Partie 5
## Évaluation paresseuse

---

# Where ne filtre pas immédiatement

```csharp {1-3|5|7-8|all}
var players = new List<Player> { new("Léa", 2.4), new("Sam", 1.2) };

var query = players.Where(p => p.Kda > 2); // ← RIEN n'est filtré ici

players.Add(new Player("Raph", 3.1)); // ajout APRÈS construction de la query

var result = query.ToList(); // ← exécution ICI
// → [Léa(2.4), Raph(3.1)]  ← Raph est inclus même ajouté après !
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

`Where` **décrit** le filtre. L'exécution a lieu quand on **matérialise** :
`ToList()` · `ToArray()` · `Count()` · `First()` · `foreach`

</div>
</v-click>

---

# Paresseux vs immédiat

<div class="grid grid-cols-2 gap-6 mt-4">
<div>

### Paresseux — retourne `IEnumerable<T>`
- `Where`
- `Select`
- `OrderBy`
- `Skip` / `Take`

*Décrivent une transformation*

</div>
<v-click>
<div>

### Immédiat — force l'exécution
- `ToList()` / `ToArray()`
- `Count()`
- `Sum()` / `Max()`
- `First()` / `Any()`

*Consomment le pipeline*

</div>
</v-click>
</div>

<v-click>
<div class="mt-4 p-3 bg-yellow-600 rounded text-yellow-200">

⚠ Captures + évaluation paresseuse : la variable capturée est lue à l'**exécution** du pipeline, pas à sa construction.

</div>
</v-click>

---

# Any et All — agrégateurs booléens

```csharp {1-5|7-8|all}
var numbers = new List<int> { 1, 2, 3, 4, 5 };

bool hasEven = numbers.Any(n => n % 2 == 0); // → true  (2 et 4 existent)
bool hasNeg  = numbers.Any(n => n < 0);       // → false
bool notEmpty = numbers.Any();                 // → true  (liste non vide)

bool allPos  = numbers.All(n => n > 0);        // → true
bool allEven = numbers.All(n => n % 2 == 0);   // → false (1,3,5 ne sont pas pairs)
```

<v-click>
<div class="mt-4 p-3 bg-orange-100 rounded text-orange-900">

`Any` et `All` sont des **Fold** sur des booléens — avant-goût de la thématique 04.

</div>
</v-click>

---
layout: center
class: text-center
---

<v-click every=1>

Les fonctions sont des **valeurs** — on les passe, on les capture, on les compose

<div class="pt-12 mb-4">
  <span class="px-4 py-2 rounded bg-blue-500 text-white text-xl">
    Action · Func · Lambda · Capture · Where
  </span>
</div>

# Questions ?

<div class="mt-8 text-gray-500">

Prochaine étape : Map / Select — transformer chaque élément

</div>
</v-click>
