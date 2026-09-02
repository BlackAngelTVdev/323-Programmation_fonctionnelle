# Exercice 07 — Audit sécurité

> Partie 6 — Records + pureté + effets de bord + `.Snapshot()`

## Concepts théoriques

- [Thématique 06 — Pureté et immutabilité](../../../../thematiques/06-purete-immutabilite.md)
- [Pureté et effets de bord](../../../../supports/source/06-PureteImmutabilite.md)
- [Records C# — l'immutabilité par défaut](../../../../supports/source/06-PureteImmutabilite.md#records-c-—-l-immutabilite-par-defaut)
- [Transparence référentielle](../../../../supports/source/06-PureteImmutabilite.md#transparence-referentielle)

## Contexte

Avant les playoffs, l'organisation fait auditer le code de la bibliothèque.
Règle : toute méthode qui accède à un état externe ou modifie une donnée partagée
est un risque pour la fiabilité des analyses — un bug silencieux peut fausser le classement.

---

## Étape 1 — Convertir les classes en records

Depuis l'exercice 01, les modèles sont des classes immuables : propriétés get-only,
constructeur qui recopie chaque paramètre. Verbeux — et "modifier" un objet oblige
à le reconstruire entièrement à la main.

**Avant de coder :** combien de lignes fait `ValorantMatch` en classe ? Combien en record ?

<details>
<summary>Voir la conversion</summary>

```csharp
// Avant — classe immuable (~25 lignes)
public class ValorantMatch
{
    public string Player { get; }
    public string Agent { get; }
    // ... 6 autres propriétés + constructeur de 10 lignes
}

// Après — record (1 ligne, même immutabilité, même comportement)
public record ValorantMatch(
    string Player, string Agent, int Kills, int Deaths,
    int Assists, int Headshots, int RoundsWon, bool Won);
```

</details>

Convertir `DataPoint<T>`, `ValorantMatch`, `Cs2Match`, `LolMatch` et `SeriesStats` en records :

```csharp
public record DataPoint<T>(DateTime Timestamp, T Value);
public record SeriesStats(double Min, double Max, double Mean, double StdDev);
// ... etc.
```

Le record apporte en plus l'expression `with` — la "modification" fonctionnelle
que la classe rendait pénible :

```csharp
var match = new ValorantMatch("Léa", "Jett", 18, 6, 4, 8, 13, true);
// match.Kills = 20; // toujours une erreur de compilation — c'est voulu !
var corrected = match with { Kills = 20 }; // nouvel objet, l'original reste intact
```

Et l'égalité par valeur : deux records aux mêmes valeurs sont égaux (`==`),
là où deux instances de classe ne le sont pas.

→ [Records C# — l'immutabilité par défaut](../../../../supports/source/06-PureteImmutabilite.md#records-c-—-l-immutabilite-par-defaut)

> Vérifier que les parsers et générateurs des exercices précédents compilent toujours —
> la conversion est transparente pour le reste du code.

---

## Étape 2 — Tableau d'audit

Trois questions pour chaque méthode :

1. **Déterministe ?** Mêmes entrées → même sortie, toujours ?
2. **Sans effets de bord ?** Modifie-t-elle quoi que ce soit en dehors de son scope ?
3. **Transparence référentielle ?** Peut-on remplacer l'appel par son résultat sans changer le comportement ?

→ [Pureté](../../../../supports/source/06-PureteImmutabilite.md#purete)

Remplir le tableau pour chaque méthode de `DataSeries<T>` :

| Méthode | Déterministe ? | Sans effets de bord ? | Transparence réf. ? | Pure ? |
|---------|---------------|----------------------|---------------------|--------|
| `From(source)` | | | | |
| `FromCsv(path, parser)` | | | | |
| `Filter(predicate)` | | | | |
| `Transform(mapper)` | | | | |
| `Fold(seed, combiner)` | | | | |
| `Statistics()` | | | | |
| `SlidingWindow(size)` | | | | |
| `Normalize()` | | | | |
| `Smooth(windowSize)` | | | | |

<details>
<summary>Voir le tableau complété</summary>

| Méthode | Déterministe ? | Sans effets de bord ? | Transparence réf. ? | Pure ? |
|---------|---------------|----------------------|---------------------|--------|
| `From(source)` | oui | oui | oui | oui |
| `FromCsv(path, parser)` | oui | **non** (accès fichier) | **non** | **non** |
| `Filter(predicate)` | oui* | oui* | oui* | oui* |
| `Transform(mapper)` | oui* | oui* | oui* | oui* |
| `Fold(seed, combiner)` | oui* | oui* | oui* | oui* |
| `Statistics()` | oui | oui | oui | oui |
| `SlidingWindow(size)` | oui | oui | oui | oui |
| `Normalize()` | oui | oui | oui | oui |
| `Smooth(windowSize)` | oui | oui | oui | oui |

*La pureté dépend aussi du prédicat/mapper passé en argument.

</details>

---

## Étape 3 — Identifier et corriger une méthode impure

Voici une version impure de `Smooth` introduite par erreur :

```csharp
private static int _smoothCallCount = 0;

public DataSeries<double> SmoothImpure(int windowSize)
{
    _smoothCallCount++;
    Console.WriteLine($"Smooth appelé {_smoothCallCount} fois");
    // ...
}
```

Combien de violations des règles de pureté voit-on ici ?

<details>
<summary>Voir l'analyse</summary>

1. `_smoothCallCount++` — mutation d'un état externe
2. `Console.WriteLine` — effet de bord I/O
3. Résultat dépend du nombre d'appels précédents — non déterministe

La méthode pure existe déjà (exercice 04). Si le comptage est nécessaire pour le débogage,
le déléguer à l'appelant — la bibliothèque ne compte pas.

</details>

Deux autres candidates refusées à l'audit — identifier la violation dans chacune :

```csharp
// Impure : résultat différent à chaque appel
public DataSeries<T> Shuffle()
{
    return new DataSeries<T>(_data.OrderBy(_ => Random.Shared.Next())); // Non-déterministe !
}

// Impure : effet de bord (écriture fichier)
public DataSeries<T> LogAndFilter(Func<T, bool> predicate)
{
    File.AppendAllText("log.txt", $"Filtering {Count} elements"); // Effet de bord !
    return Filter(predicate);
}
```

---

## Étape 4 — `.Snapshot()` et l'importance de `ToList()`

La bibliothèque repose sur des pipelines paresseux (exercice 03). Que se passe-t-il si deux
consommateurs matérialisent la même query à des moments différents, alors que la source
a changé entre-temps ?

Tester le couplage caché :

```csharp
var source = new List<DataPoint<double>>
{
    new(new DateTime(2024, 1, 1), 1.0),
    new(new DateTime(2024, 1, 2), 2.0),
    new(new DateTime(2024, 1, 3), 3.0),
};
var series = DataSeries<double>.From(source);

source.Add(new DataPoint<double>(new DateTime(2024, 1, 4), 4.0));
Console.WriteLine(series.Count); // Combien ? Pourquoi ?
```

`Snapshot()` coupe ce couplage :

```csharp
public DataSeries<T> Snapshot()
    => DataSeries<T>.From(_data.ToList());
```

Vérifier :

```csharp
var series = DataSeries<double>.From(source).Snapshot();
source.Add(new DataPoint<double>(new DateTime(2024, 1, 4), 4.0));
Console.WriteLine(series.Count); // 3 — snapshot isolé de la source
```

---

## Étape 5 — Interface CLI

Ajouter `--audit` pour afficher le rapport de pureté de la bibliothèque dans la console.

**Avant de coder :** Que doit afficher `--audit` ? Comment structurer l'affichage
du tableau de pureté sans dépendance externe ? Pourquoi utiliser `return` après l'affichage ?

```
dotnet run -- --audit
```

<details>
<summary>Voir la solution</summary>

```csharp
if (args.Contains("--audit"))
{
    Console.WriteLine("Audit de pureté — DataSeries<T>");
    Console.WriteLine($"{"Méthode",-25} {"Déterministe",-15} {"Sans effet",-12} Pure");
    Console.WriteLine(new string('-', 60));

    var rows = new[]
    {
        ("From(source)",          "oui", "oui",  "oui"),
        ("FromCsv(path,parser)",  "oui", "non",  "non"),
        ("Filter(predicate)",     "oui*","oui*", "oui*"),
        ("Transform(mapper)",     "oui*","oui*", "oui*"),
        ("Fold(seed,combiner)",   "oui*","oui*", "oui*"),
        ("Statistics()",          "oui", "oui",  "oui"),
        ("SlidingWindow(size)",   "oui", "oui",  "oui"),
        ("Normalize()",           "oui", "oui",  "oui"),
        ("Smooth(windowSize)",    "oui", "oui",  "oui"),
    };
    foreach (var (m, d, e, p) in rows)
        Console.WriteLine($"{m,-25} {d,-15} {e,-12} {p}");

    Console.WriteLine("* dépend de la pureté de la fonction passée en argument");
    return;
}
```

</details>

---

> **Pourquoi la pureté est précieuse.** **Testable** — pas de mock, pas d'état à préparer.
> **Composable** — si `f` et `g` sont pures, `f(g(x))` l'est aussi. **Parallélisable** —
> sans état partagé, pas de race conditions.
> → [Pourquoi la pureté est précieuse](../../../../supports/source/06-PureteImmutabilite.md#pourquoi-la-purete-est-precieuse)

## Vérification

- Les modèles sont des records — `with` fonctionne, la mutation directe reste impossible
- Le tableau d'audit est complété — `FromCsv` identifiée comme impure
- `SmoothImpure` corrigée : mêmes entrées → même sortie, aucun état global modifié
- `Snapshot()` isole la série de la source
- Toutes les méthodes pures restent testables sans setup ni mock
