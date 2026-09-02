---
theme: default
title: "Extensions & DSL"
info: "Méthodes d'extension, chaînage, DSL, Zip, composition de fonctions"
author: ETML
transition: slide-left
mdc: true
---

# Extensions & DSL

## Enrichir le langage, composer les fonctions

<div class="pt-12">
  <span class="px-2 py-1 rounded bg-blue-500 text-white">
    Thématique 05
  </span>
</div>

---

# Et si ton code se lisait comme une phrase ?

<v-clicks>

```csharp
// Version classique — l'action choisie en premier, données au fond
Console.WriteLine(
    String.Join(", ", Filter(GetMatches(), m => m.Won)));
```

```csharp
// Version extensions — les données d'abord, lecture naturelle
GetMatches()
    .Where(m => m.Won)
    .Select(m => m.ToString())
    .JoinWith(", ")
    .ToConsole();
// → "cs2 ✓, val ✓"
```

<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Les méthodes d'extension permettent d'**ajouter des méthodes à un type existant**
et de faire du chaînage fluide — les données d'abord, les transformations ensuite.

</div>

</v-clicks>

---

# Plan

<v-clicks>

1. **Méthodes d'extension** — la syntaxe
2. **Chaînage fluide** — lire de gauche à droite
3. **DSL** — Domain Specific Language
4. **Zip** — combiner deux séquences en parallèle
5. **Composition de fonctions** — `f ∘ g` en C#

</v-clicks>

---
layout: section
---

# Partie 1
## Méthodes d'extension

---

# La recette : classe statique + `this`

```csharp {1-7|9-10|all}
// Classe publique statique + méthode publique statique
// + mot-clé "this" devant le PREMIER paramètre
public static class StringExtensions
{
    public static string Greetings(this string name)
        => $"Hello {name}";
}

// Utilisation : comme si c'était une méthode native de string
Console.WriteLine("Bob".Greetings()); // → "Hello Bob"
```

<v-click>
<div class="mt-4 p-3 bg-blue-700 rounded text-blue-300">

Le mot-clé `this` devant le premier paramètre : c'est toute la magie.
La classe doit être `public static`.

</div>
</v-click>

---

# LINQ est 100% construit sur les extensions

```csharp {1-4|6-11|all}
// La vraie signature de Where (simplifiée) :
public static IEnumerable<T> Where<T>(
    this IEnumerable<T> source,
    Func<T, bool> predicate) { ... }

// Ce que LINQ permet d'écrire grâce à cette extension :
numbers
    .Where(n => n > 0)   // extension sur IEnumerable<T>
    .Select(n => n * 2)  // extension sur IEnumerable<T>
    .ToList();
// → [2, 4, 6, 8, 10]
```

<v-click>
<div class="mt-4 p-3 bg-green-700 rounded text-green-200">

Les extensions permettent d'écrire `source.Op()` au lieu de `Op(source)`.
C'est ce qui rend le chaînage naturel.

</div>
</v-click>

---

# Chaîner avec retour de valeur

```csharp {1-7|9-15|all}
public static IEnumerable<string> ToLower(
    this IEnumerable<string> source, bool random = false)
{
    return source.Select(text =>
        random ? (new Random().Next(2) == 1 ? text.ToLower() : text)
               : text.ToLower());
}

var data = new[] { "BoB", "Max", "jOelLe", "NadiA" };
data
    .Where(name => name.StartsWith("j"))
    .ToLower(random: true)
    .ToList()
    .ForEach(Console.WriteLine);
// → "jOelle"  ou  "joelle"  (50/50)
```

---
layout: section
---

# Partie 2
## DSL — Domain Specific Language

---

# Un langage qui parle le métier

Un DSL est un pseudo-langage plus proche du domaine que du langage hôte.
L'objectif : lire le code sans connaître les détails techniques.

<v-clicks>

```csharp {1-8}
// FluentAssertions — les tests se lisent en anglais
[Fact]
public void TestIsMatch()
{
    bool result = cmd.IsMatch(Program.OptHelp);
    result.Should().BeTrue("la commande contient --help");
}
// Sans connaître FluentAssertions, le sens est évident
```

```csharp {1-7}
// Cosmos — tester un interpréteur de langage custom
[Fact]
public void TestDifferentNumber()
{
    TestBoolean("5".IsDifferentThan("6"), true);
}
// Sans connaître Cosmos, on comprend ce qui est testé
```

</v-clicks>

---
layout: section
---

# Partie 3
## Zip — deux séquences en parallèle

---

# Marier deux listes position par position

```
dates  : ["01.03", "08.03", "15.03"]
kda    : [  2.4,     3.1,    1.8,   2.9]  ← 4 valeurs
          ───┬───   ───┬───  ───┬───  ✗ ignoré
             │         │        │
résultat : ["01.03→2.4", "08.03→3.1", "15.03→1.8"]
```

<v-click>

```csharp {1-5|7-8|all}
var dates = new[] { "01.03", "08.03", "15.03" };
var kda   = new[] { 2.4, 3.1, 1.8, 2.9 };

var timeline = dates.Zip(kda, (d, k) => $"{d} → KDA {k}");
// → ["01.03 → KDA 2.4", "08.03 → KDA 3.1", "15.03 → KDA 1.8"]

// Sans combineur : tuples
var pairs = dates.Zip(kda); // → [("01.03",2.4), ("08.03",3.1), ("15.03",1.8)]
```

</v-click>

---

# Propriétés de Zip

<v-clicks>

<div class="mt-1 p-1 bg-blue-800 rounded text-blue-100">

**Appariement par position** : le 1er avec le 1er, le 2e avec le 2e.
Aucune clé, aucun tri — seule la position compte.

</div>

<div class="mt-1 p-1 bg-yellow-600 rounded text-yellow-200">

**Arrêt sur la plus courte** : 3 dates pour 4 scores → 3 résultats.
L'élément en trop est ignoré silencieusement.

</div>

<div class="mt-1 p-1 bg-green-700 rounded text-green-200">

Utile pour comparer deux séries côte à côte :
performances de deux semaines consécutives, deux joueurs, deux versions.

</div>

</v-clicks>

---
layout: section
---

# Partie 4
## Composition de fonctions

---

# f ∘ g : la sortie de g devient l'entrée de f

```
Maths  :  (f ∘ g)(x) = f(g(x))
C# LINQ :  x.g().f()   ← même chose, lecture gauche → droite
```

<v-click>

```csharp {1-2|4-8|all}
// Sans extensions — lecture de droite à gauche, difficile à suivre
var r = ToUpperCase(RemoveSpaces(Trim(input)));

// Avec extensions — même chose, lecture naturelle
var r = input
    .Trim()           // → "  hello  " → "hello"
    .RemoveSpaces()   // → "hello"     → "hello"
    .ToUpperCase();   // → "hello"     → "HELLO"
```

</v-click>

---

# Le contrat : retourner le même type

```csharp {1-9|11-15|all}
// Chaque méthode reçoit IEnumerable<T> et retourne IEnumerable<T>
// → le chaînage est possible
numbers
    .Where(n => n > 0)       // IEnumerable<int> → IEnumerable<int>
    .Select(n => n * 2)      // IEnumerable<int> → IEnumerable<int>
    .OrderBy(n => n)         // IEnumerable<int> → IOrderedEnumerable<int>
    .Take(3)                 // → IEnumerable<int>
    .ToList();               // → List<int>  (matérialisation)
// → [2, 4, 6]

// Composition explicite avec Func
Func<string, string> trim    = s => s.Trim();
Func<string, string> toLower = s => s.ToLower();
Func<string, string> normalize = s => toLower(trim(s));
normalize("  HELLO  "); // → "hello"
```

---

# Pourquoi composition et immutabilité sont liées

<div class="grid grid-cols-2 gap-6 mt-4">
<div>

### Sans immutabilité
```
Filter(data) modifie data
Select(data) voit data altéré
→ résultat imprévisible
```

Composition **impossible en toute sécurité**.

</div>
<v-click>
<div>

### Avec immutabilité
```
Filter(data) → nouvelle liste
Select(...)  → nouvelle liste
data intact tout au long
→ résultat prévisible
```

Composition **sûre et garantie**.

</div>
</v-click>
</div>

<v-click>
<div class="mt-4 p-3 bg-orange-100 rounded text-orange-900">

**Triangle indissociable :** immutabilité + pureté + composition — chacun rend les deux autres possibles.

</div>
</v-click>

---
layout: center
class: text-center
---

<v-click every=1>

Enrichir le langage et lire le code **comme une phrase**

<div class="pt-12 mb-4">
  <span class="px-4 py-2 rounded bg-blue-500 text-white text-xl">
    Extensions · Chaînage · DSL · Zip · f ∘ g
  </span>
</div>

# Questions ?

<div class="mt-8 text-gray-500">

Prochaine étape : Pureté & Immutabilité — code sans effets de bord

</div>
</v-click>
