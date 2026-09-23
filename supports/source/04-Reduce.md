# Accumulateur | Aggrégateur | Reduce

![alt text](pile.jpg)

Cet aspect a déjà été aperçu avec les fonctions `Sum` et `Average` utilisées pour [l'exercice Epsilon](../../exos/words/README.md#partie-2--epsilon).

Voici un exemple basique pour calculer une somme et une moyenne:

```csharp
List<int> numbers = new(){1,2,3,4,5};
int sum = numbers.Sum(); // 15
double average = numbers.Average(); // 3.0
```

> Ces 2 fonctions effectuent une opération qui "aplatit" la liste en la **réduisant** (d'où le terme `Reduce`) à une seule valeur.

À cela s'ajoutent deux autres fonctions bien pratiques, `Min` et `Max`:

```csharp
int max = numbers.Max(); // 5
int min = numbers.Min(); // 1
```

### Où est la programmation fonctionnelle ici ?

Jusque là, on a l'impression d'avoir à faire à des fonctions standards, on va donc étudier un nouveau cas pour lequel on comprend bien l'intérêt d'avoir des fonctions de réduction d'ordre supérieur:

```csharp
class Person{
    public string Name{get;set;}
    public int Age{get;set;}
    public int Sisters{get;set;}
    public int Brothers{get;set;}
  }

List<Person> cid5d = new List<Person>(){
    new Person(){Name="Paul",Age=15,Sisters=2,Brothers=1},
    new Person(){Name="Lucie",Age=18,Sisters=1,Brothers=3},
    new Person(){Name="Claude",Age=16,Sisters=0,Brothers=0}
};

double averageAge = cid5d.Average(person=>person.Age);
double averageSiblings = cid5d.Average(person=>person.Sisters + person.Brothers);

int minAge = cid5d.Min(person=>person.Age);
```

### GroupBy

Les deux exemples ci-dessus réduisent la liste à une seule valeur numérique. C'est bien, mais c'est peut-être trop radical. Examinons le code suivant dans lequel nous allons réduire la liste en une liste de groupes de personnes appartenant à une fratrie de même nombre:

```csharp
List<Person> cid5d = new List<Person>(){
    new Person(){Name="Paul",Age=17,Sisters=2,Brothers=1},
    new Person(){Name="Lucie",Age=18,Sisters=1,Brothers=3},
    new Person(){Name="Helmut",Age=19,Sisters=2,Brothers=1},
    new Person(){Name="Germaine",Age=18,Sisters=1,Brothers=0},
    new Person(){Name="Pierre",Age=17,Sisters=0,Brothers=1},
    new Person(){Name="Sylvie",Age=18,Sisters=1,Brothers=0},
    new Person(){Name="Ernest",Age=14,Sisters=2,Brothers=1},
    new Person(){Name="Sidonie",Age=18,Sisters=1,Brothers=2},
    new Person(){Name="Claude",Age=17,Sisters=0,Brothers=0}
};

// Faire des groupes en fonction de la taille de la famille
var groups = cid5d
    .GroupBy(p => p.Sisters+p.Brothers)
    .OrderBy(g => g.Key)
    .Select(group => new {                  // Objet anonyme
        FamilySize = group.Key,
        Members = group.Select(p => p.Name)
    })
    .ToList();

// Affichage
groups.ForEach(group =>
{
    Console.WriteLine($"Famille de {group.FamilySize}: {String.Join(",", group.Members)}");
});
```

Résultat:

```
Famille de 0: Claude
Famille de 1: Germaine,Pierre,Sylvie
Famille de 3: Paul,Helmut,Ernest,Sidonie
Famille de 4: Lucie
```

**Agrégation par clé — la lecture FP.** `GroupBy` seul ne réduit rien : il réorganise.
La réduction arrive quand chaque groupe est agrégé — un `Fold` *par clé* :

```csharp
// Âge moyen par taille de fratrie — GroupBy + Aggregate en un pipeline
var averageAgeBySiblings = cid5d
    .GroupBy(p => p.Sisters + p.Brothers)
    .Select(g => new {
        FamilySize = g.Key,
        AverageAge = g.Aggregate(0.0, (acc, p) => acc + p.Age) / g.Count()
    });
```

Le motif `GroupBy(clé).Select(g => g.Aggregate(...))` est l'équivalent fonctionnel du
`GROUP BY` + fonction d'agrégat de SQL : partitionner, puis réduire chaque partition
à une valeur. Aucune boucle, aucun dictionnaire mutable rempli à la main.

### Aggrégateur générique

Outre les accumulateurs particuliers fournis par _LINQ_, il existe une fonction d'ordre supérieur générique pour la réduction nommée `Aggregate` dont voici un premier exemple:

```csharp
int sum = numbers.Aggregate((current,next)=>current+next)
```

Chaque élément est comparé à celui d'après et en résulte un seul élément défini par le lambda.
Ainsi, à la fin de l'opération, _il ne doit en rester qu'un_...

![](victory.webp)

### Réécriture de `Min`

Avec l'accumulateur générique `Aggregate`, on peut réécrire le `Min` ainsi:

```csharp
int min = numbers.Aggregate((a,b)=>Convert.ToInt32(Math.Min(a,b))); //1
```

Et ainsi de suite pour les autres opérateurs.

#### Variantes d’aggrégateurs
Hormis la fonction Aggregate présentée ci-dessus, il en existe 2 variantes.
Elles partent du principe qu’on va souvent vouloir transformer le résultat final et que l’élément de départ (seed)
n’est pas forcément inclus dans la liste. Au maximum elles contiennent 3 arguments:

1. Seed (valeur de départ à comparer avec le 1er élément)
2. Fonction d'aggrégation
3. Choix de la forme du résultat

#### Avec seed,fonction et transformation (1,2,3)
``` csharp
string sum = numbers.Aggregate(/*seed*/0,/**/(a,b)=>a+b,/*transformation*/number=>$"Somme:{number}");
// ^ !! type string à cause du typre retourné par la lambda en 3ème argument !!
```

#### Juste avec seed et fonction (1,2)
``` csharp
int sum = numbers.Aggregate(/*seed*/0,/**/(a,b)=>a+b);
```

### Aggrégateurs avec classes

Pour des types non primitifs, on doit justement utiliser une des variantes précédemment présentées:

```csharp
var min = cid5d.Aggregate(
    new Person(){Brothers=99}, //Seed
    (a,b)=>a.Brothers<b.Brothers?a:b, //Min logic
    person=>person.Name); //Result transformer
```

#### Que vaut min ?

- 0
- Paul
- Germaine
- Sylvie
- Claude
- 1
- ...

La fonction d'aggrégation sélectionne la personne avec le moins de frères et la forme du résultat est demandée sous forme du nom de la personne.

Le résultat est donc:

<details>
<summary>Cliquer ici pour voir/vérifier la réponse</summary>
Claude
</details>

## Fold — l'Agrégation Universelle

Le concept de **Fold** (aussi appelé *reduce*, *aggregate*, *foldl* selon les langages)
est l'une des idées les plus profondes de la programmation fonctionnelle.

**Fold est universel : toutes les agrégations sont des cas particuliers de Fold.**

Voici comment `Sum`, `Count`, `Max`, `Any` et `All` se réécrivent en Fold pur :

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };

// Sum — additionner toutes les valeurs
int sum = numbers.Aggregate(0, (acc, val) => acc + val);          // 15

// Count — compter les éléments
int count = numbers.Aggregate(0, (acc, _) => acc + 1);            // 5

// Max — trouver le plus grand
int max = numbers.Aggregate(int.MinValue,
    (acc, val) => val > acc ? val : acc);                          // 5

// Min — trouver le plus petit
int min = numbers.Aggregate(int.MaxValue,
    (acc, val) => val < acc ? val : acc);                          // 1

// Any(pred) — y a-t-il au moins un élément satisfaisant le prédicat ?
bool anyEven = numbers.Aggregate(false,
    (acc, val) => acc || val % 2 == 0);                            // true

// All(pred) — tous les éléments satisfont-ils le prédicat ?
bool allPos = numbers.Aggregate(true,
    (acc, val) => acc && val > 0);                                  // true
```

### Pourquoi comprendre Fold en profondeur ?

Parce que comprendre Fold, c'est comprendre que :

1. **L'abstraction est puissante** : une seule opération (`Aggregate`) remplace `Sum`, `Max`,
   `Count`, `Any`, `All` et des dizaines d'autres. Le schéma est toujours le même :
   *valeur de départ + règle de combinaison*.

2. **La récursion et Fold sont équivalents** : en Haskell, Fold *est* défini récursivement.
   En C#, `Aggregate` est une boucle optimisée qui évite les stack overflows, mais le concept
   est identique.

3. **Map est un Fold** : `Select` peut se réécrire en Fold qui accumule dans une nouvelle liste.

```csharp
// Select réécrit en Fold
var doubled = numbers.Aggregate(
    new List<int>(),                              // seed : liste vide
    (acc, val) => { acc.Add(val * 2); return acc; } // accumulation
);
// [2, 4, 6, 8, 10]
```

> Dans d'autres langages (Haskell, Scala, F#), Fold est souvent la première chose enseignée.
> En C#, `Aggregate` est sa traduction directe. Quand tu utilises `Sum()`, tu utilises un Fold.

### Schéma mental de Fold

```
seed → [elem1] → acc1 → [elem2] → acc2 → [elem3] → acc3 → ... → résultat final
         f(seed, elem1)    f(acc1, elem2)    f(acc2, elem3)
```

Commence avec le `seed`, applique la fonction `f` à chaque élément avec l'accumulateur courant,
et le résultat de `f` devient le nouvel accumulateur. À la fin, l'accumulateur EST le résultat.
