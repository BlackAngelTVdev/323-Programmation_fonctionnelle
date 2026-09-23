# Exercice 02 — Les recrues

> Partie 2 — `MatchSeries.FromCsv` + `Enumerable.Range` + `Select` (map sur un range) + seed fixe

## Concepts théoriques

- [Thématique 02 — Filter et fonctions d'ordre supérieur](../../../../thematiques/02-filter-fonctions-sup.md)
- [Fonctions d'ordre supérieur](../../../../supports/source/02a-fonctions-sup.md)
- [Closures](../../../../supports/source/02a-fonctions-sup.md#closures-captures-de-variables)
- [Évaluation paresseuse](../../../../supports/source/02b-filter.md#evaluation-paresseuse-deferred-execution)

## Contexte

Les matchs en dur de l'exercice 01 ne suffisent plus : les 75 matchs de la saison attendent
dans `../esport/data/`. Ensuite, place aux recrues : Raphaël, Noé, Dylan et Kiara n'ont rejoint
Team Helvetia qu'en cours de saison — leurs matchs précédents sont perdus. Le data analyst
doit simuler des données plausibles pour compléter l'historique.

---

## Étape 1 — `MatchSeries.FromCsv` (import fichier)

Première **fonction d'ordre supérieur** concrète du cours : une méthode qui reçoit une *fonction*
en paramètre.

**Avant de coder :** `FromCsv` doit fonctionner pour Valorant, CS2 et LoL — trois formats CSV
différents. Comment éviter d'écrire trois méthodes différentes ?
Quel paramètre permet de déléguer la logique de parsing à l'appelant ?

<details>
<summary>Indice sur la signature</summary>

Le parser est une fonction `Func<string[], IMatchData>` — elle reçoit les colonnes d'une ligne
CSV et retourne un `IMatchData` complet (date incluse). `FromCsv` reste dans la bibliothèque,
le domaine reste dans `EsportApp`.
→ [Func et Action](../../../../supports/source/02a-fonctions-sup.md)

</details>

Implémenter la méthode dans `MatchSeries.cs` :

```csharp
public static MatchSeries FromCsv(string path, Func<string[], IMatchData> parser)
{
    // lire le fichier, ignorer la première ligne (en-tête)
    // parser chaque ligne en la découpant par ','
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public static MatchSeries FromCsv(string path, Func<string[], IMatchData> parser)
{
    var lines = File.ReadAllLines(path).Skip(1); // ignorer l'en-tête
    return new MatchSeries(lines.Select(line => parser(line.Split(','))));
}
```

Le parser reçoit toutes les colonnes — il est responsable de la date (cols[0]) et de tous les autres champs.
La bibliothèque ignore complètement le format CSV : tout est délégué à la fonction passée en paramètre.

> **Différence avec esport :** Dans `DataSeries<T>.FromCsv`, la bibliothèque parsait automatiquement
> `cols[0]` en `DateTime` et le stockait dans un `DataPoint<T>`. Ici, la date fait partie de
> `IMatchData` — c'est le parser qui la gère. La bibliothèque ne suppose rien sur la structure du CSV.

</details>

**Avant de coder les parsers :** pour chaque jeu, identifier l'indice de chaque colonne.
Ouvrir les CSV de `../esport/data/` et vérifier les en-têtes.

```csharp
// Valorant : date(0), player(1), agent(2), kills(3), deaths(4), assists(5), headshots(6), rounds_won(7), won(8)
IMatchData ParseValorant(string[] cols) => new ValorantMatch(
    DateTime.Parse(cols[0]),  // date
    cols[1],                  // player
    cols[2],                  // agent
    int.Parse(cols[3]),       // kills
    // ...
);
```

<details>
<summary>Voir les parsers complets (Valorant, CS2, LoL)</summary>

```csharp
IMatchData ParseValorant(string[] cols) => new ValorantMatch(
    DateTime.Parse(cols[0]),  // date
    cols[1],                  // player
    cols[2],                  // agent
    int.Parse(cols[3]),       // kills
    int.Parse(cols[4]),       // deaths
    int.Parse(cols[5]),       // assists
    int.Parse(cols[6]),       // headshots
    int.Parse(cols[7]),       // roundsWon
    bool.Parse(cols[8])       // won
);

IMatchData ParseCs2(string[] cols) => new Cs2Match(
    DateTime.Parse(cols[0]),  // date
    cols[1],                  // player
    cols[2],                  // map
    cols[3],                  // startSide (côté joué en 1re mi-temps — CT ou T)
    int.Parse(cols[4]),       // kills
    int.Parse(cols[5]),       // deaths
    int.Parse(cols[6]),       // assists
    int.Parse(cols[7]),       // mvps
    bool.Parse(cols[8])       // won
);

IMatchData ParseLol(string[] cols) => new LolMatch(
    DateTime.Parse(cols[0]),  // date
    cols[1],                  // player
    cols[2],                  // champion
    int.Parse(cols[4]),       // kills
    int.Parse(cols[5]),       // deaths
    int.Parse(cols[6]),       // assists
    int.Parse(cols[7]),       // cs
    int.Parse(cols[8]),       // visionScore
    bool.Parse(cols[9])       // won
);
```

</details>

Ajouter `FilterByDate` dans `MatchSeries.cs` pour filtrer par période :

```csharp
public MatchSeries FilterByDate(Func<DateTime, bool> predicate)
    => new MatchSeries(_data.Where(m => predicate(m.Date)));
```

Charger les trois fichiers et vérifier le total :

```csharp
var valorant = MatchSeries.FromCsv("../esport/data/valorant.csv", ParseValorant);
var cs2      = MatchSeries.FromCsv("../esport/data/cs2.csv",      ParseCs2);
var lol      = MatchSeries.FromCsv("../esport/data/lol.csv",      ParseLol);

Console.WriteLine($"Valorant : {valorant.Count} matchs");
Console.WriteLine($"CS2      : {cs2.Count} matchs");
Console.WriteLine($"LoL      : {lol.Count} matchs");
// Total : 75 matchs
```

Un seul `FromCsv` pour trois formats différents : la logique de parsing est une **valeur**
passée en paramètre. C'est exactement ce qui manquait à l'exercice 01.

Requêtes chronologiques disponibles grâce à `Date` dans `IMatchData` :

```csharp
// Matchs du premier trimestre
var q1 = valorant.FilterByDate(d => d.Month <= 3);
Console.WriteLine($"Matchs jan–mars : {q1.Count}");

// Combiner filtre temporel et accès direct — composition naturelle
var q1Wins = valorant.FilterByDate(d => d.Month <= 3).Matches.Where(m => m.Won);
Console.WriteLine($"Victoires jan–mars : {q1Wins.Count()}");
```

---

## Étape 2 — Générer 20 matchs CS2 pour Raphaël

`Enumerable.Range` génère une séquence d'entiers. Combiné avec `Select`, il devient un
**générateur fonctionnel** — l'équivalent d'une boucle for, mais déclaratif :

```csharp
// Impératif
var list = new List<int>();
for (int i = 1; i <= 20; i++)
    list.Add(i * 2);

// Fonctionnel — même résultat
var list = Enumerable.Range(1, 20).Select(i => i * 2);
```

La seed fixe (`new Random(42)`) garantit un résultat **reproductible** — propriété essentielle
pour des données de test : tout le monde obtient les mêmes valeurs.

**Avant de coder :** quelles informations faut-il générer aléatoirement pour un match CS2 ?
Quelles valeurs sont réalistes en MR12 (maximum 24 rounds par match) ?

<details>
<summary>Voir les plages de valeurs réalistes</summary>

- `Map` : tirer au sort parmi `{ "Dust2", "Mirage", "Inferno", "Nuke", "Ancient" }`
- `StartSide` : `"CT"` ou `"T"` (côté de la première mi-temps)
- `Kills` : entre 10 et 27
- `Deaths` : entre 6 et 17
- `Assists` : entre 0 et 7
- `Mvps` : entre 0 et 4
- `Won` : aléatoire 50/50

</details>

Créer `EsportApp/MatchGenerator.cs` avec la méthode `GenerateCs2` :

```csharp
public static class MatchGenerator
{
    public static MatchSeries GenerateCs2(string player, int count, int seed = 42)
    {
        var rng   = new Random(seed);
        var maps  = new[] { "Dust2", "Mirage", /* ... */ };
        var sides = new[] { "CT", "T" };
        var start = new DateTime(2023, 9, 1); // début de la pré-saison

        return MatchSeries.From(
            Enumerable.Range(1, count)
                .Select(i => (IMatchData)new Cs2Match(
                    start.AddDays(i),
                    player,
                    maps[rng.Next(maps.Length)],
                    /* ... */
                ))
        );
    }
}
```

> La lambda passée à `Select` utilise `rng`, `maps`, `sides` et `start` déclarés *en dehors* d'elle :
> c'est une **closure** — la fonction capture les variables de son environnement.
> → [Closures](../../../../supports/source/02a-fonctions-sup.md#closures-captures-de-variables)

<details>
<summary>Voir la solution complète</summary>

```csharp
public static class MatchGenerator
{
    public static MatchSeries GenerateCs2(string player, int count, int seed = 42)
    {
        var rng   = new Random(seed);
        var maps  = new[] { "Dust2", "Mirage", "Inferno", "Nuke", "Ancient" };
        var sides = new[] { "CT", "T" };
        var start = new DateTime(2023, 9, 1);

        return MatchSeries.From(
            Enumerable.Range(1, count)
                .Select(i => (IMatchData)new Cs2Match(
                    start.AddDays(i),
                    player,
                    maps[rng.Next(maps.Length)],
                    sides[rng.Next(2)],
                    rng.Next(10, 28),   // kills
                    rng.Next(6, 18),    // deaths
                    rng.Next(0, 8),     // assists
                    rng.Next(0, 5),     // mvps
                    rng.Next(2) == 0    // won
                ))
        );
    }
}
```

> Le cast `(IMatchData)` est nécessaire car `MatchSeries.From` attend `IEnumerable<IMatchData>`.
> Alternativement, déclarer le tableau avec le type explicite :
> `Enumerable.Range(1, count).Select<int, IMatchData>(i => new Cs2Match(...))`.

</details>

Dans `Program.cs` :

```csharp
var raphaelGenerated = MatchGenerator.GenerateCs2("Raphaël", 20);
Console.WriteLine(raphaelGenerated.Count); // 20
```

**Ajouter des contraintes réalistes.** Quelles combinaisons générées sont physiquement
impossibles dans CS2 ?

<details>
<summary>Voir les contraintes à appliquer</summary>

- `kills + assists > 50` : impossible dans un match CS2
- `deaths == 0` : n'arrive jamais sur une série complète de matchs

</details>

Le prédicat de validation est une **valeur** : stockée dans une variable, nommée,
réutilisable pour les 4 joueurs.

```csharp
Func<IMatchData, bool> isValid = m =>
    m.Kills + m.Assists <= 50 &&
    m.Deaths >= 1;

var raphaelValid = raphaelGenerated.Filter(isValid);
Console.WriteLine($"Avant : {raphaelGenerated.Count}, après : {raphaelValid.Count}");
```

> `Filter` sera implémenté dans la bibliothèque à l'exercice 03. Pour l'instant,
> anticiper son usage : il reçoit un `Func<IMatchData, bool>` et retourne un nouveau `MatchSeries`.

---

## Étape 3 — Interface CLI

Ajouter le flag `--generate <joueur|all>` pour déclencher la génération depuis la ligne de commande.

**Avant de coder :** Si `--generate all` est passé, comment obtenir la liste des quatre joueurs ?
Comment structurer le code pour que `--generate Raphaël` ne génère que ce joueur ?

<details>
<summary>Voir la solution</summary>

```csharp
if (args.Contains("--generate"))
{
    var target = args[Array.IndexOf(args, "--generate") + 1];

    var players = target == "all"
        ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
        : new[] { target };

    Func<IMatchData, bool> isValid = m =>
        m.Kills + m.Assists <= 50 && m.Deaths >= 1;

    foreach (var player in players)
    {
        var series = MatchGenerator.GenerateCs2(player, 20);
        Console.WriteLine($"{player} : {series.Count} matchs générés");
    }
    return;
}
```

</details>

---

## Vérification

- 75 matchs chargés depuis les CSV : `valorant.Count` = 25, `cs2.Count` = 25, `lol.Count` = 25
- `valorant.FilterByDate(d => d.Month <= 3).Count` : sous-ensemble du premier trimestre
- `raphaelGenerated.Count` = 20, après validation : quelques matchs éliminés
- Changer la seed → valeurs différentes ; même seed → résultat identique à chaque exécution
