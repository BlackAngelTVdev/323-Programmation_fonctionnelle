---
theme: default
title: "Pureté & Immutabilité"
info: "Fonctions pures, transparence référentielle, immutabilité, records C#"
author: ETML
transition: slide-left
mdc: true
---

# Pureté & Immutabilité

## Code prévisible, testable, composable

<div class="pt-12">
  <span class="px-2 py-1 rounded bg-blue-500 text-white">
    Thématique 06
  </span>
</div>

---

# Ton test passe en local, échoue sur le serveur CI.
# Parfois. Pas toujours.

<v-clicks>

```csharp
// La fonction calcule un KDA… avec un peu de bruit aléatoire
double KDAWithNoise(int kills, int deaths, int assists)
    => (kills + assists) / (double)deaths * Random.Shared.NextDouble();

// Test : KDA(10, 2, 5) devrait valoir 7.5
Assert.Equal(7.5, KDAWithNoise(10, 2, 5)); // ← parfois vrai, souvent faux
```

<div class="mt-1 p-1 bg-red-700 rounded text-red-100">

La fonction retourne un résultat différent à chaque appel. Elle est **impure**.
Tests aléatoires, debugging cauchemardesque, résultats non reproductibles.

</div>

```csharp
// Version pure : même entrées → même sortie, toujours
double KDA(int kills, int deaths, int assists)
    => (kills + assists) / (double)(deaths == 0 ? 1 : deaths);

Assert.Equal(7.5, KDA(10, 2, 5)); // → passe. Toujours.
```

</v-clicks>

---

# Plan

<v-clicks>

1. **Fonction pure** — définition et règles
2. **Transparence référentielle** — remplacer par la valeur
3. **Pourquoi la pureté est précieuse** — test, composition, parallélisme
4. **Immutabilité** — données figées dès la création
5. **Records C#** — immutabilité par défaut en une ligne

</v-clicks>

---
layout: section
---

# Partie 1
## Fonctions pures

---

# La boîte noire mathématique

```
          f(x) = x × 2
              ┌───────┐
   3 ─────────►  × 2  ├──────── 6
              └───────┘

Aujourd'hui, demain, dans 10 ans : f(3) = 6. Toujours.
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

**Définition :** une fonction pure est une fonction qui, pour les **mêmes arguments en entrée**, retourne toujours le **même résultat**, sans provoquer d'**effets secondaires**.

</div>
</v-click>

---

# Les 3 règles d'une fonction pure

<v-clicks>

<div class="mt-1 p-1 bg-blue-800 rounded text-blue-100">

**1. Déterministe** : même entrée → même sortie, toujours.
`KDA(10, 2, 5)` retourne toujours `7.5`.

</div>

<div class="mt-1 p-1 bg-blue-700 rounded text-blue-300">

**2. Sans effets de bord** : pas de modification de variable globale,
pas d'écriture fichier/BDD, pas d'affichage console.

</div>

<div class="mt-1 p-1 bg-blue-800 rounded text-blue-100">

**3. Indépendante du contexte** : le résultat ne dépend **que** des arguments.
Pas de réseau, pas d'heure système, pas d'état externe.

</div>

</v-clicks>

---

# Pure vs Impure — les trois cas

```csharp {1-3|5-7|9-15|all}
// ✅ PURE
double KDA(int kills, int deaths, int assists)
    => (kills + assists) / (double)(deaths == 0 ? 1 : deaths);

// ❌ IMPURE — non-déterministe (résultat différent à chaque appel)
double KDAWithNoise(int k, int d, int a)
    => KDA(k, d, a) * Random.Shared.NextDouble();

// ❌ IMPURE — effet de bord (modifie un état externe)
int _callCount = 0;
double KDATracked(int k, int d, int a)
{
    _callCount++; // ← modifie l'état global
    return KDA(k, d, a);
}
```

---
layout: section
---

# Partie 2
## Transparence référentielle

---

# Remplacer un appel par sa valeur

```csharp {1-2|4-5|all}
// Fonction pure — référentiellement transparente
double KDA(int k, int d, int a) => (k + a) / (double)(d == 0 ? 1 : d);

double result = KDA(10, 2, 5); // → 7.5
double result = 7.5;           // strictement équivalent — le compilateur peut le faire
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

La transparence référentielle permet au compilateur d'**optimiser**, de **mettre en cache**, et de **paralléliser** automatiquement — parce qu'il sait que le résultat ne changera pas.

</div>
</v-click>

---

# Le cache gratuit (mémoïsation)

```python
# Python : @cache transforme automatiquement la fonction
@cache
def factorial(n):
    return n * factorial(n-1) if n else 1

# Premier appel  : calcul  → factorial(10) = 3628800  (mis en cache)
# Deuxième appel : cache   → 3628800  (instantané)
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Possible **uniquement** avec une fonction pure.
Si le résultat peut changer entre deux appels (Random, état), le cache retournerait une valeur incorrecte.

</div>
</v-click>

---
layout: section
---

# Partie 3
## Pourquoi la pureté est précieuse

---

# 3 super-pouvoirs

<v-clicks>

<div class="mt-1 p-1 bg-green-700 rounded text-green-200">

**Testable** : pas de mock, pas d'état à préparer.
`Assert.Equal(7.5, KDA(10, 2, 5))` — appel direct, résultat prévisible.

</div>

<div class="mt-1 p-1 bg-blue-700 rounded text-blue-300">

**Composable** : si `f` et `g` sont pures, `f(g(x))` l'est aussi.
La composition sûre est **garantie** — pas de surprises cachées.

</div>

<div class="mt-1 p-1 bg-orange-100 rounded text-orange-900">

**Parallélisable** : sans état partagé → pas de race conditions.
Calculs en parallèle sans `lock`, sans `deadlock`.

</div>

</v-clicks>

---
layout: section
---

# Partie 4
## Immutabilité

---

# Une donnée figée dès sa création

```
Création : x = 5
┌──────────────────┐
│  boîte mémoire   │  ← figée
│  x = 5           │
└──────────────────┘

Vouloir "modifier" x ?
→ Créer une NOUVELLE variable : y = x + 3 → y = 8
→ x reste 5, intact
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

En FP : jamais `x = x + 3`.
On ne modifie pas — on crée une nouvelle valeur à partir de l'ancienne.

</div>
</v-click>

---

# Fausse immutabilité — piège courant

```csharp {1-7|9-13|all}
static class ImmutableIntList
{
    static readonly List<int> items = new();

    public static void Add(int x) { items.Add(x); }  // ← mutation du contenu !
    public static void Reset()    { items.Clear(); }  // ← mutation du contenu !
}

// Ce qui est readonly : l'ADRESSE de la liste en mémoire
// items = new List<int>(); ← erreur de compilation ✓

// Ce qui reste MUTABLE : le CONTENU pointé par cette adresse
items.Add(1); // ← autorisé même avec readonly ✗
```

<v-click>
<div class="mt-4 p-3 bg-red-700 rounded text-red-100">

`readonly` protège la **référence**, pas les **données**. Ce n'est pas l'immutabilité FP.

</div>
</v-click>

---
layout: section
---

# Partie 5
## Records C# — immutabilité par défaut

---

# Le problème : verbosité des classes immuables

```csharp {1-12|all}
// Classe immuable à la main — ~10 lignes de plomberie
public class Point
{
    public double X { get; }
    public double Y { get; }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}
// Et l'égalité par valeur ? Il faut aussi override Equals et GetHashCode...
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Le mot-clé `record` (C# 9) génère tout cela automatiquement — en **une ligne**.

</div>
</v-click>

---

# Record : une ligne, tout inclus

```csharp {1|3-5|7-9|11-13|all}
public record Point(double X, double Y);

// Immutabilité
var p1 = new Point(1.0, 2.0);
// p1.X = 5.0; ← erreur de compilation ✓

// "Modifier" → créer une copie avec le changement voulu
var p2 = p1 with { X = 5.0 };  // → Point(5.0, 2.0)
// p1 reste Point(1.0, 2.0) — intact

// Égalité par valeur (contrairement aux classes)
Console.WriteLine(new Point(1, 2) == new Point(1, 2)); // → true  (record)
// Pour une classe sans override : → false (compare les adresses)
```

---

# Le triangle indissociable

```
         Pureté
        /       \
       /         \
      /           \
Immutabilité ─── Composition

Chacun rend les deux autres possibles et utiles.
```

<v-clicks>

<div class="mt-1 p-1 bg-blue-800 rounded text-blue-100">

**Pureté → Composition** : si `f` et `g` sont pures, `f(g(x))` l'est aussi.

</div>

<div class="mt-1 p-1 bg-blue-700 rounded text-blue-300">

**Immutabilité → Composition** : les données ne changent pas — le pipeline est prévisible.

</div>

<div class="mt-1 p-1 bg-green-700 rounded text-green-200">

**Composition → Pureté** : pour composer sans surprises, les fonctions doivent être pures.

</div>

</v-clicks>

---
layout: center
class: text-center
---

<v-click every=1>

Code **prévisible**, **testable** et **composable**

<div class="pt-12 mb-4">
  <span class="px-4 py-2 rounded bg-blue-500 text-white text-xl">
    Pureté · Transparence référentielle · Immutabilité · Record
  </span>
</div>

# Questions ?

<div class="mt-8 text-gray-500">

Prochaine étape : Récursivité — la boucle fonctionnelle

</div>
</v-click>
