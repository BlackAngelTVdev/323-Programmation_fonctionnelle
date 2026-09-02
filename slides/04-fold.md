---
theme: default
title: "Fold / Aggregate — Agrégation"
info: "Reduce, Aggregate, GroupBy, Fold universel"
author: ETML
transition: slide-left
mdc: true
---

# Fold / Aggregate

## Réduire une collection à une valeur

<div class="pt-12">
  <span class="px-2 py-1 rounded bg-blue-500 text-white">
    Thématique 04
  </span>
</div>

---

# Sum, Average, Max, Count, Any…
# Et si toutes ces opérations étaient une seule et même idée ?

<v-clicks>

```csharp
numbers.Sum()              // → 15
numbers.Average()          // → 3.0
numbers.Max()              // → 5
numbers.Count()            // → 5
numbers.Any(n => n > 3)   // → true
```

```csharp
// Toutes réécrites avec une seule opération — Aggregate :
numbers.Aggregate(0,   (acc, n) => acc + n)           // → 15   (Sum)
numbers.Aggregate(0,   (acc, _) => acc + 1)           // → 5    (Count)
numbers.Aggregate(int.MinValue, (acc,n) => n>acc?n:acc) // → 5  (Max)
numbers.Aggregate(false, (acc,n) => acc || n > 3)     // → true (Any)
```

<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

`Sum`, `Max`, `Count`, `Any`… ne sont pas des opérations "spéciales" — ce sont toutes des **instanciations du même Fold universel**.

</div>

</v-clicks>

---

# Plan

<v-clicks>

1. **Agrégateurs courants** — Sum, Average, Min, Max
2. **Aggregate** — le Fold universel
3. **Fold universel** — tout est un Fold
4. **GroupBy** — réduire par clé (SQL GROUP BY)

</v-clicks>

---
layout: section
---

# Partie 1
## Agrégateurs courants

---

# N valeurs → 1 valeur

```
[1, 2, 3, 4, 5]  →  Sum()  →  15
[1, 2, 3, 4, 5]  →  Max()  →  5
[1, 2, 3, 4, 5]  →  Count() → 5
```

<v-click>

```csharp {1-6|8-11|all}
List<int> numbers = new() { 1, 2, 3, 4, 5 };

int    sum = numbers.Sum();     // → 15
double avg = numbers.Average(); // → 3.0
int    max = numbers.Max();     // → 5
int    min = numbers.Min();     // → 1

// Sur des types complexes : sélecteur (HOF !)
double avgAge      = people.Average(p => p.Age);      // → 16.33
double avgSiblings = people.Average(p => p.Sisters + p.Brothers); // → 2.0
int    minAge      = people.Min(p => p.Age);           // → 15
```

</v-click>

---
layout: section
---

# Partie 2
## Aggregate — le schéma

---

# Métaphore : la pile de crêpes

```
Assiette vide (seed)
  + crêpe 1  →  assiette avec 1 crêpe  (acc intermédiaire)
  + crêpe 2  →  assiette avec 2 crêpes
  + crêpe 3  →  assiette avec 3 crêpes
  ...
  + crêpe N  →  résultat final
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Fold = **accumuler** chaque élément dans un résultat intermédiaire,
jusqu'à ce qu'il ne reste qu'**un seul** résultat.

</div>
</v-click>

---

# Schéma mental de Fold

```
seed → [e1] → acc1 → [e2] → acc2 → [e3] → ... → résultat
        f(seed,e1)    f(acc1,e2)   f(acc2,e3)
```

<v-clicks>

```csharp {1-6}
// Aggregate(seed=0, f=(acc,val)=>acc+val)  sur [1,2,3,4,5]
// acc= 0 + 1 → 1
// acc= 1 + 2 → 3
// acc= 3 + 3 → 6
// acc= 6 + 4 → 10
// acc=10 + 5 → 15  ← résultat final
```

```csharp
numbers.Aggregate(0, (acc, val) => acc + val) // → 15
```

</v-clicks>

---

# Trois variantes d'Aggregate

```csharp {1-2|4-5|7-12|all}
// 1. Sans seed (premier élément = seed)
numbers.Aggregate((acc, val) => acc + val); // → 15

// 2. Avec seed
numbers.Aggregate(0, (acc, val) => acc + val); // → 15

// 3. Seed + transformateur du résultat final
numbers.Aggregate(
    0,
    (acc, val) => acc + val,
    total => $"Somme : {total}"
); // → "Somme : 15"
```

---
layout: section
---

# Partie 3
## Fold universel

---

# Tout est un Fold

```csharp {1-4|6-11|13-15|all}
var numbers = new[] { 1, 2, 3, 4, 5 };

// Sum → accumuler avec +
int sum = numbers.Aggregate(0, (acc, n) => acc + n); // → 15

// Count → compter les passages
int count = numbers.Aggregate(0, (acc, _) => acc + 1); // → 5

// Max → garder le plus grand à chaque étape
int max = numbers.Aggregate(int.MinValue,
    (acc, n) => n > acc ? n : acc); // → 5

// Any → OR booléen
bool anyEven = numbers.Aggregate(false,
    (acc, n) => acc || n % 2 == 0); // → true
```

---

# Map est aussi un Fold

```csharp {1-5|7-8|all}
// Select réécrit en Fold — pour comprendre la mécanique
var doubled = numbers.Aggregate(
    new List<int>(),                              // seed : liste vide
    (acc, val) => { acc.Add(val * 2); return acc; }
); // → [2, 4, 6, 8, 10]

// La vraie version — à utiliser en pratique
var doubled2 = numbers.Select(n => n * 2).ToList(); // → [2, 4, 6, 8, 10]
```

<v-click>
<div class="mt-4 p-3 bg-orange-100 rounded text-orange-900">

`Select` est un `Aggregate` qui accumule dans une nouvelle liste.
Fold est l'opération primitive dont les autres sont dérivées.

</div>
</v-click>

---
layout: section
---

# Partie 4
## GroupBy — réduire par clé

---

# Grouper, puis réduire chaque groupe

```csharp {1-10|12-19|all}
// Grouper par taille de fratrie — GroupBy seul ne réduit pas
var groups = people
    .GroupBy(p => p.Sisters + p.Brothers)
    .OrderBy(g => g.Key)
    .Select(g => new {
        FamilySize = g.Key,
        Members    = g.Select(p => p.Name)
    });
// → { FamilySize=0, Members=["Claude"] }
// → { FamilySize=1, Members=["Germaine","Pierre","Sylvie"] }

// Fold PAR CLÉ — âge moyen par taille de fratrie
var avgAgeByGroup = people
    .GroupBy(p => p.Sisters + p.Brothers)
    .Select(g => new {
        FamilySize = g.Key,
        AvgAge = g.Aggregate(0.0, (acc, p) => acc + p.Age) / g.Count()
    });
// → { FamilySize=0, AvgAge=17.0 }, { FamilySize=1, AvgAge=18.0 }, ...
```

---

# Le triptyque Filter → Map → Fold

```
Collection [e1, e2, e3, e4, e5]
     │
     ▼  Filter (Where)    — ne garder que ce qu'on veut
     [e2, e4]
     │
     ▼  Map (Select)      — transformer la forme
     [f(e2), f(e4)]
     │
     ▼  Fold (Aggregate)  — réduire à une valeur
     résultat unique
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Ce triptyque est le pipeline fondamental de la programmation fonctionnelle — on le retrouve dans tout traitement de données.

</div>
</v-click>

---
layout: center
class: text-center
---

<v-click every=1>

Une seule opération pour **réduire** n'importe quelle collection

<div class="pt-12 mb-4">
  <span class="px-4 py-2 rounded bg-blue-500 text-white text-xl">
    Aggregate · seed · accumulation · GroupBy
  </span>
</div>

# Questions ?

<div class="mt-8 text-gray-500">

Prochaine étape : Extensions & DSL — enrichir le langage

</div>
</v-click>
