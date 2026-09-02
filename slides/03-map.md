---
theme: default
title: "Map / Select — Transformation"
info: "Select, projection, tuple, SelectMany, composition de pipelines"
author: ETML
transition: slide-left
mdc: true
---

# Map / Select

## Transformer chaque élément

<div class="pt-12">
  <span class="px-2 py-1 rounded bg-blue-500 text-white">
    Thématique 03
  </span>
</div>

---

# 1000 matchs avec kills et deaths bruts.
# Tu veux 1000 ratios KDA.
# Même structure — contenu transformé, originaux intacts.

<v-clicks>

```csharp
var matches = new[] {
    new Match("Léa",  kills: 18, deaths: 4, assists: 6),
    new Match("Raph", kills: 12, deaths: 5, assists: 9),
};
```

```csharp
// Transformer chaque Match en un ratio KDA — sans modifier les originaux
var kdas = matches.Select(m => (m.Kills + m.Assists) / (double)m.Deaths);
// → [6.0,  4.2]

// matches est intact — aucun objet n'a été modifié
```

<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

`Select` applique une transformation à chaque élément et retourne une **nouvelle** séquence.
La source ne change pas.

</div>

</v-clicks>

---

# Plan

<v-clicks>

1. **Map vs Filter** — transformer vs sélectionner
2. **Select** — projection d'attributs et calculs
3. **Tuple & classe anonyme** — structures légères
4. **SelectMany** — aplatir des collections imbriquées
5. **Composition** — Where + Select enchaînés

</v-clicks>

---
layout: section
---

# Partie 1
## Map vs Filter

---

# Deux opérations complémentaires

<div class="grid grid-cols-2 gap-6 mt-4">
<div>

### Filter (Where)
Garde ou rejette — **même type**

```
[1, 2, 3, 4, 5]
      ↓ n > 2
      [3, 4, 5]
```

N éléments → M ≤ N éléments

</div>
<v-click>
<div>

### Map (Select)
Transforme — **type peut changer**

```
[1,  2,  3,  4,  5 ]
      ↓ n × 2
[2,  4,  6,  8,  10]
```

N éléments → **toujours** N éléments

</div>
</v-click>
</div>

---
layout: section
---

# Partie 2
## Select — projections et calculs

---

# Extraire un attribut

```csharp {1-10|12-14|16-18|all}
class Person {
    public string Name    { get; set; }
    public int    Age     { get; set; }
    public int    Sisters { get; set; }
    public int    Brothers{ get; set; }
}
var people = new List<Person> {
    new() { Name = "Paul",  Age = 15, Sisters = 2, Brothers = 1 },
    new() { Name = "Lucie", Age = 18, Sisters = 1, Brothers = 3 },
};

// Extraire les noms
IEnumerable<string> names = people.Select(p => p.Name);
// → ["Paul", "Lucie"]

// Calculer une valeur dérivée
IEnumerable<int> siblings = people.Select(p => p.Sisters + p.Brothers);
// → [3, 4]
```

---

# Changer le type de sortie

```csharp {1-3|5-10|12-14|all}
// double → double (Celsius → Kelvin)
var kelvin = temps.Select(t => t + 273.15).ToList();
// → [298.15, 310.15, ...]

// Person → objet anonyme (sous-ensemble d'attributs)
var compact = people.Select(p => new {
    p.Name,
    Siblings = p.Sisters + p.Brothers
});
// → [{ Name="Paul", Siblings=3 }, { Name="Lucie", Siblings=4 }]

// Person → classe existante
var members = people.Select(p => new Member(p.Name, p.Age)).ToList();
// → [Member("Paul",15), Member("Lucie",18)]
```

---
layout: section
---

# Partie 3
## Tuple & classe anonyme

---

# Structures légères pour les projections

<div class="grid grid-cols-2 gap-6 mt-4">
<div>

### Tuple — accès par position
```csharp
var adults = people
    .Select(p => (p.Name, p.Age))
    .Where(t => t.Age >= 18);

adults.First().Name // → "Lucie"
```

⚠ `ToList()` efface les noms : `Item1`, `Item2`…

</div>
<v-click>
<div>

### Classe anonyme — noms conservés
```csharp
var compact = people.Select(p => new {
    p.Name,
    Siblings = p.Sisters + p.Brothers
});

compact.First().Name     // → "Paul"
compact.First().Siblings // → 3
```

Noms préservés sur toute la chaîne.

</div>
</v-click>
</div>

---
layout: section
---

# Partie 4
## SelectMany — aplatir

---

# Select → imbriqué | SelectMany → aplati

```csharp {1-8|10-12|all}
var teams = new[] {
    new { Player = "Léa",     Matches = new[] { "m1", "m2", "m3" } },
    new { Player = "Raphaël", Matches = new[] { "m4", "m5" } },
};

// Select → séquence DE séquences
IEnumerable<string[]> nested = teams.Select(t => t.Matches);
// → { ["m1","m2","m3"], ["m4","m5"] }   ← imbriqué

// SelectMany → une seule séquence aplatie
IEnumerable<string> flat = teams.SelectMany(t => t.Matches);
// → { "m1","m2","m3","m4","m5" }   ← aplati
```

<v-click>

| | Sélecteur | Résultat |
|---|---|---|
| `Select` | `T → TResult` | N → N |
| `SelectMany` | `T → IEnumerable<TResult>` | N → tout concaténé |

</v-click>

---
layout: section
---

# Partie 5
## Composition de pipelines

---

# Enchaîner Where et Select

```csharp {1-7|9-10|all}
// f(g(x)) en maths = x.g().f() en LINQ
var result = matches
    .Where(m => m.Deaths > 0)          // Filter → IEnumerable<Match>
    .Select(m => (m.Kills + m.Assists)
                 / (double)m.Deaths)   // Map    → IEnumerable<double>
    .ToList();
// → [6.0, 4.2, 2.8]

// Possible UNIQUEMENT parce que Where retourne une NOUVELLE séquence
// (la source n'est pas modifiée → immutabilité → composition possible)
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Chaque méthode retourne une **nouvelle** valeur sans modifier la source.
L'immutabilité rend la composition possible.

</div>
</v-click>

---

# Captures dans Select — même règle que Where

```csharp {1-3|5|7-8|all}
double factor = 1.5; // capturé par le lambda ci-dessous

var boosted = players.Select(p => p.Kda * factor); // lien vers factor

factor = 2.0; // la variable change AVANT l'exécution

var result = boosted.ToList();
// → KDA × 2.0, pas × 1.5  ← la valeur lue à l'exécution, pas à la construction
```

<v-click>
<div class="mt-4 p-3 bg-yellow-600 rounded text-yellow-200">

Même comportement que pour `Where` : la valeur capturée est lue au moment de l'**exécution** du pipeline, pas au moment de sa construction.

</div>
</v-click>

---
layout: center
class: text-center
---

<v-click every=1>

Transformer chaque élément **sans toucher à la source**

<div class="pt-12 mb-4">
  <span class="px-4 py-2 rounded bg-blue-500 text-white text-xl">
    Select · Projection · SelectMany · Pipeline
  </span>
</div>

# Questions ?

<div class="mt-8 text-gray-500">

Prochaine étape : Fold / Aggregate — réduire à une seule valeur

</div>
</v-click>
