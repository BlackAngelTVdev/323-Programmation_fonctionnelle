---
theme: default
title: "Récursivité"
info: "Récursion, cas de base, schéma, Fold vs récursion, TCO"
author: ETML
transition: slide-left
mdc: true
---

# Récursivité

## La boucle fonctionnelle

<div class="pt-12">
  <span class="px-2 py-1 rounded bg-blue-500 text-white">
    Thématique 08
  </span>
</div>

---

# Calculer la taille d'un répertoire.
# Facile — sauf qu'il contient des sous-répertoires,
# qui contiennent eux-mêmes des sous-répertoires…

<v-clicks>

```
📁 projet/          ← combien d'octets ici ?
├── 📁 src/
│   ├── 📁 utils/
│   │   └── 📄 helper.cs   (2 ko)
│   └── 📄 main.cs         (5 ko)
└── 📄 README.md            (1 ko)
```

```csharp
// La structure se répète à chaque niveau — même problème, plus petit
long Size(Directory dir)
{
    if (dir.IsEmpty) return 0;                         // cas trivial
    return dir.Files.Sum(f => f.Size)                  // fichiers du niveau
         + dir.SubDirs.Sum(sub => Size(sub));           // + récursion
}
// Size(projet/) → 8 ko
```

<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Quand la **structure du problème se répète**, la récursion s'impose naturellement.

</div>

</v-clicks>

---

# Plan

<v-clicks>

1. **Définition** — une fonction qui se divise
2. **Schéma universel** — cas de base + règle de combinaison
3. **Exemples** — factorielle, Fibonacci, somme récursive
4. **Récursion vs Fold** — deux faces de la même pièce
5. **Performances** — Tail Call Optimisation

</v-clicks>

---
layout: section
---

# Partie 1
## Une fonction qui se divise

---

# Métaphore : les poupées russes

```
Ouvrir une poupée :
  ├── Elle est vide ?  → STOP  ← cas de base
  └── Sinon → sortir la poupée intérieure, recommencer  ← appel récursif
```

<v-clicks>

```csharp
// Compter les poupées dans une poupée
int Count(Matryoshka doll)
{
    if (doll.IsEmpty) return 0;            // cas de base
    return 1 + Count(doll.Inner);          // règle : 1 + count du reste
}

Count(grande) // → 4  (si 4 poupées imbriquées)
```

<div class="mt-1 p-1 bg-blue-700 rounded text-blue-300">

Il est **essentiel** que la décomposition mène vers un cas trivial.
Sinon : boucle infinie → `StackOverflowException`.

</div>

</v-clicks>

---
layout: section
---

# Partie 2
## Schéma universel

---

# Toute récursion a exactement 2 parties

```
Fonction F(problème P) :
  ├── P est trivial ?  → retourner la valeur directement   CAS DE BASE
  └── Sinon           → diviser P en sous-problèmes        APPEL RÉCURSIF
                         appeler F sur chaque sous-problème
                         combiner les résultats
```

<v-click>

```csharp
// Template universel
T Solve(Input input)
{
    if (IsBaseCase(input)) return BaseResult;            // 1. trivial
    return Combine(Solve(Smaller(input)));               // 2. récursion
}
```

</v-click>

---
layout: section
---

# Partie 3
## Exemples

---

# Factorielle : itérative vs récursive

```
6! = 6 × 5 × 4 × 3 × 2 × 1 = 720
   = 6 × 5!
```

<v-clicks>

```csharp {1-8}
// Version itérative — compteur mutable
int Factorial(int x)
{
    int res = 1;
    for (int i = x; i > 1; i--) res *= i;
    return res;
}
// Factorial(6) → 720
```

```csharp {1-7}
// Version récursive — aucune variable mutable
int Factorial(int x)
{
    if (x == 1) return 1;          // cas de base
    return x * Factorial(x - 1);  // règle : n × (n-1)!
}
// Factorial(6) → 720
```

</v-clicks>

---

# Trace d'exécution : Factorial(5)

```
Factorial(5)
  = 5 × Factorial(4)
          = 4 × Factorial(3)
                  = 3 × Factorial(2)
                          = 2 × Factorial(1)
                                  = 1    ← cas de base atteint
                          = 2 × 1 = 2   ← remontée
                  = 3 × 2 = 6
          = 4 × 6 = 24
  = 5 × 24 = 120
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

La pile grandit jusqu'au cas de base, puis se **déplie** en remontant.
Chaque retour combine son résultat avec l'appelant.

</div>
</v-click>

---

# Somme récursive et Fibonacci

```csharp {1-7|9-11|all}
// Somme d'une liste — cas de base : liste vide
int Sum(IEnumerable<int> list)
{
    if (!list.Any()) return 0;               // → 0
    return list.First() + Sum(list.Skip(1)); // → premier + somme du reste
}
// Sum([1,2,3,4,5]) → 15

// Fibonacci : chaque terme = somme des deux précédents
long Fib(int n) => n <= 1 ? n : Fib(n - 1) + Fib(n - 2);
// Fib(7) → 13
```

<v-click>
<div class="mt-4 p-3 bg-orange-100 rounded text-orange-900">

Toutes les variables sont **immutables** — pas de compteur, pas de `res` qui change.
C'est la signature du style fonctionnel.

</div>
</v-click>

---
layout: section
---

# Partie 4
## Récursion vs Fold

---

# Deux faces de la même pièce

```csharp {1-6|8-11|all}
// Récursion explicite — structure visible
int SumRecursive(IEnumerable<int> list)
    => list.Any()
        ? list.First() + SumRecursive(list.Skip(1))
        : 0;
// SumRecursive([1,2,3]) → 6

// Fold — récursion généralisée et optimisée
int SumFold(IEnumerable<int> list)
    => list.Aggregate(0, (acc, val) => acc + val);
// SumFold([1,2,3]) → 6
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

`Aggregate` **est** la récursion, rendue itérative pour éviter les stack overflows.
Conceptuellement identiques — syntaxe différente.

</div>
</v-click>

---

# Quand choisir l'un ou l'autre ?

<div class="grid grid-cols-2 gap-6 mt-4">
<div>

### Fold (Aggregate)
Structure **plate** : listes, séquences

```csharp
list.Aggregate(0, (a, v) => a + v)
// → 15
```

Code concis, optimisé par le runtime.

</div>
<v-click>
<div>

### Récursion explicite
Structure **arborescente** : arbres, répertoires

```csharp
int Tree(Node n) =>
    n == null ? 0 :
    n.Value + Tree(n.Left)
             + Tree(n.Right);
// → taille totale de l'arbre
```

Lisibilité suit la forme du problème.

</div>
</v-click>
</div>

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Choisir selon la **forme du problème** : liste plate → Fold, structure imbriquée → récursion.

</div>
</v-click>

---
layout: section
---

# Partie 5
## Performances & TCO

---

# Sans optimisation : la pile déborde

```
Factorial(100 000)
  → Factorial(99 999)
    → Factorial(99 998)
      → ...
        → Factorial(1)   ← StackOverflowException !
```

<v-click>

```csharp {1-6|8-13|all}
// Version NON tail-call : doit attendre le retour pour multiplier
int Factorial(int x)
{
    if (x == 1) return 1;
    return x * Factorial(x - 1); // x est en attente → reste sur la pile
}

// Version tail-call : l'appel récursif est la DERNIÈRE opération
int FactTail(int x, int acc = 1)
{
    if (x == 1) return acc;
    return FactTail(x - 1, x * acc); // rien en attente → pile réutilisable
}
```

</v-click>

---

# Tail Call Optimisation (TCO)

```
Version normale :       Version tail-call :
frame 1 : x=5           frame 1 → réutilisé → réutilisé → réutilisé
frame 2 : x=4
frame 3 : x=3           Un seul frame — O(1) mémoire
frame 4 : x=2
frame 5 : x=1
  ↑ O(n) mémoire
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Avec TCO le compilateur réutilise le même stack frame.
`Aggregate` en C# applique cette optimisation — c'est pourquoi il est préférable à la récursion explicite sur de grandes listes.

</div>
</v-click>

---
layout: center
class: text-center
---

<v-click every=1>

Diviser pour mieux **conquérir** — sans boucle, sans mutation

<div class="pt-12 mb-4">
  <span class="px-4 py-2 rounded bg-blue-500 text-white text-xl">
    Cas de base · Appel récursif · Fold · TCO
  </span>
</div>

# Questions ?

<div class="mt-8 text-gray-500">

Fin du cours — bon courage pour le projet P_FUN !

</div>
</v-click>
