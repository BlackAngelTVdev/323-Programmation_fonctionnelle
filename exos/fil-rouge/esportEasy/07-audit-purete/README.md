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

## Étape 1 — Convertir les classes en records implémentant `IMatchData`

Depuis l'exercice 01, les modèles sont des classes immuables : propriétés get-only,
constructeur qui recopie chaque paramètre. Verbeux — et "modifier" un objet oblige
à le reconstruire entièrement à la main.

**Avant de coder :** combien de lignes fait `ValorantMatch` en classe ? Combien en record ?
Est-il possible d'utiliser la syntaxe de record positionnel (`record ValorantMatch(...)`)
tout en implémentant `IMatchData` ?

<details>
<summary>Voir la conversion et la contrainte d'interface</summary>

```csharp
// Avant — classe immuable (~25 lignes)
public class ValorantMatch : IMatchData
{
    public DateTime Date { get; }
    public string Player { get; }
    // ... 7 autres propriétés + constructeur de 12 lignes
}

// Après — record positionnel avec interface (les noms DOIVENT correspondre aux membres de IMatchData)
public record ValorantMatch(
    DateTime Date, string Player, string Agent,
    int Kills, int Deaths, int Assists,
    int Headshots, int RoundsWon, bool Won) : IMatchData;
```

Le record positionnel génère automatiquement les propriétés `{ get; init; }` — compatibles
avec les propriétés `{ get; }` de l'interface, car une propriété `init` satisfait le contrat
de lecture seule.

> **Attention :** si l'interface définit `int Kills { get; }` et que le record déclare
> `int Kills` en paramètre positionnel, C# génère `int Kills { get; init; }` — cela
> satisfait `IMatchData`. Le compilateur accepte.

</details>

Convertir `ValorantMatch`, `Cs2Match`, `LolMatch` et `SeriesStats` en records :

```csharp
public record ValorantMatch(
    DateTime Date, string Player, string Agent,
    int Kills, int Deaths, int Assists,
    int Headshots, int RoundsWon, bool Won) : IMatchData;

public record Cs2Match(
    DateTime Date, string Player, string Map, string StartSide,
    int Kills, int Deaths, int Assists, int Mvps, bool Won) : IMatchData;

public record LolMatch(
    DateTime Date, string Player, string Champion,
    int Kills, int Deaths, int Assists,
    int Cs, int VisionScore, bool Won) : IMatchData;

public record SeriesStats(double Min, double Max, double Mean, double StdDev);
```

Le record apporte en plus l'expression `with` — la "modification" fonctionnelle
que la classe rendait pénible :

```csharp
var match = new ValorantMatch(new DateTime(2024, 1, 15), "Léa", "Jett", 18, 6, 4, 8, 13, true);
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

Remplir le tableau pour chaque méthode de `MatchSeries` et `StatSeries` :

| Méthode | Déterministe ? | Sans effets de bord ? | Transparence réf. ? | Pure ? |
|---------|---------------|----------------------|---------------------|--------|
| `MatchSeries.From(source)` | | | | |
| `MatchSeries.FromCsv(path, parser)` | | | | |
| `MatchSeries.Filter(predicate)` | | | | |
| `MatchSeries.Extract(selector)` | | | | |
| `StatSeries.Filter(predicate)` | | | | |
| `StatSeries.Fold(seed, combiner)` | | | | |
| `StatSeries.Statistics()` | | | | |
| `StatSeries.SlidingWindow(size)` | | | | |
| `StatSeries.Normalize()` | | | | |
| `StatSeries.Smooth(windowSize)` | | | | |

<details>
<summary>Voir le tableau complété</summary>

| Méthode | Déterministe ? | Sans effets de bord ? | Transparence réf. ? | Pure ? |
|---------|---------------|----------------------|---------------------|--------|
| `MatchSeries.From(source)` | oui | oui | oui | oui |
| `MatchSeries.FromCsv(path, parser)` | oui | **non** (accès fichier) | **non** | **non** |
| `MatchSeries.Filter(predicate)` | oui* | oui* | oui* | oui* |
| `MatchSeries.Extract(selector)` | oui* | oui* | oui* | oui* |
| `StatSeries.Filter(predicate)` | oui* | oui* | oui* | oui* |
| `StatSeries.Fold(seed, combiner)` | oui* | oui* | oui* | oui* |
| `StatSeries.Statistics()` | oui | oui | oui | oui |
| `StatSeries.SlidingWindow(size)` | oui | oui | oui | oui |
| `StatSeries.Normalize()` | oui | oui | oui | oui |
| `StatSeries.Smooth(windowSize)` | oui | oui | oui | oui |

*La pureté dépend aussi de la pureté de la fonction passée en argument.

</details>

---

## Étape 3 — Identifier et corriger une méthode impure

Voici une version impure de `Smooth` introduite par erreur :

```csharp
private static int _smoothCallCount = 0;

public StatSeries SmoothImpure(int windowSize)
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
public StatSeries Shuffle()
{
    return StatSeries.From(_data.OrderBy(_ => Random.Shared.Next())
        .Select(d => (d.Date, d.Value))); // Non-déterministe !
}

// Impure : effet de bord (écriture fichier)
public StatSeries LogAndFilter(Func<double, bool> predicate)
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
var source = new List<IMatchData>
{
    new ValorantMatch(new DateTime(2024, 1, 1), "Léa", "Jett",  18, 6, 4, 8, 13, true),
    new ValorantMatch(new DateTime(2024, 1, 2), "Léa", "Reyna", 22, 8, 2, 11,  9, false),
};
var series = MatchSeries.From(source);

source.Add(new ValorantMatch(new DateTime(2024, 1, 3), "Léa", "Neon", 20, 7, 5, 9, 13, true));
Console.WriteLine(series.Count); // Combien ? Pourquoi ?
```

Ajouter `Snapshot()` dans `StatSeries.cs` :

```csharp
public StatSeries Snapshot()
    => new StatSeries(_data.ToList());
```

Vérifier :

```csharp
var kdaLea = valorant.Filter(m => m.Player == "Léa")
                     .Extract(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths))
                     .Snapshot(); // fige la série

// Modifier la source d'origine n'affecte plus kdaLea
Console.WriteLine(kdaLea.Count); // toujours 13
```

---

## Étape 5 — Interface CLI

Ajouter `--audit` pour afficher le rapport de pureté de la bibliothèque dans la console.

```
dotnet run -- --audit
```

<details>
<summary>Voir la solution</summary>

```csharp
if (args.Contains("--audit"))
{
    Console.WriteLine("Audit de pureté — MatchSeries / StatSeries");
    Console.WriteLine($"{"Méthode",-35} {"Déterministe",-15} {"Sans effet",-12} Pure");
    Console.WriteLine(new string('-', 70));

    var rows = new[]
    {
        ("MatchSeries.From(source)",        "oui", "oui",  "oui"),
        ("MatchSeries.FromCsv(path,parser)","oui", "non",  "non"),
        ("MatchSeries.Filter(predicate)",   "oui*","oui*", "oui*"),
        ("MatchSeries.Extract(selector)",   "oui*","oui*", "oui*"),
        ("StatSeries.Fold(seed,combiner)",  "oui*","oui*", "oui*"),
        ("StatSeries.Statistics()",         "oui", "oui",  "oui"),
        ("StatSeries.SlidingWindow(size)",  "oui", "oui",  "oui"),
        ("StatSeries.Normalize()",          "oui", "oui",  "oui"),
        ("StatSeries.Smooth(windowSize)",   "oui", "oui",  "oui"),
    };
    foreach (var (m, d, e, p) in rows)
        Console.WriteLine($"{m,-35} {d,-15} {e,-12} {p}");

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
- Les records implémentent correctement `IMatchData` — les parsers compilent sans modification
- Le tableau d'audit est complété — `FromCsv` identifiée comme impure
- `SmoothImpure` corrigée : mêmes entrées → même sortie, aucun état global modifié
- `Snapshot()` isole la série de la source
- Toutes les méthodes pures restent testables sans setup ni mock
