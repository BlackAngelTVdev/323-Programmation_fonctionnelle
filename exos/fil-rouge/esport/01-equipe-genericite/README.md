# Exercice 01 — L'équipe

> Partie 1 — `DataPoint` + `DataSeries.From`

## Concepts théoriques

- [Thématique 01 — Paradigmes fonctionnels](../../../../thematiques/01-paradigmes-fonctionnels.md)
- [Impératif ou déclaratif](../../../../supports/source/01-paradigmes.md#imperatif-ou-declaratif)
- [Généricité — abstraire les types](../../../../supports/source/01b-genericite.md)
- Immutabilité (propriétés get-only) — introduction, approfondie en exercice 07

## Contexte

Le manager de Team Helvetia modélise le roster pour le prochain tournoi.
L'équipe joue dans trois jeux différents — les matchs Valorant, CS2 et LoL n'ont pas
les mêmes caractéristiques. Un outil capable de s'adapter aux trois formats est nécessaire.

C'est le rôle de `DataSeries<T>` : le type `T` change selon le jeu, le pipeline reste identique.

Deux projets, deux responsabilités :

```
┌──────────────────┐   utilise    ┌───────────────────────┐
│    EsportApp     │ ───────────► │    DataSeries<T>      │
│  (console app)   │              │   (class library)     │
│ connaît l'esport │              │ générique — ignore    │
│ parse les args   │              │ tout du domaine       │
└──────────────────┘              └───────────────────────┘
```

---

## Étape 1 — Modéliser un match (classe immuable)

**Avant de coder :** ouvrir [../data/valorant.csv](../data/valorant.csv) et identifier les colonnes.
Quels champs faut-il pour représenter un match Valorant ?

<details>
<summary>Voir les champs à modéliser</summary>

- `Player` (string) — nom du joueur
- `Agent` (string) — personnage joué
- `Kills` (int)
- `Deaths` (int)
- `Assists` (int)
- `Headshots` (int)
- `RoundsWon` (int)
- `Won` (bool)

</details>

Définir la classe `DataPoint<T>` dans `DataSeries/DataPoint.cs`, puis la classe `ValorantMatch`
dans `EsportApp/Program.cs`. Les propriétés sont **get-only** : une fois l'objet construit,
rien ne peut plus changer.

```csharp
public class DataPoint<T>
{
    public DateTime Timestamp { get; }
    public T Value { get; }

    public DataPoint(DateTime timestamp, T value)
    {
        Timestamp = timestamp;
        Value = value;
    }
}
```

`DataPoint<T>` est l'unité élémentaire d'une série temporelle : `Timestamp` indique **quand**,
`Value` indique **quoi**. `DataSeries<T>` stocke des `DataPoint<T>` — chaque donnée chargée
depuis CSV (exercice 02) sera enveloppée avec la date du match.

```csharp
public class ValorantMatch
{
    public string Player { get; }
    public string Agent { get; }
    public int Kills { get; }
    // ... les autres propriétés + le constructeur
}
```

<details>
<summary>Voir la solution complète</summary>

```csharp
public class ValorantMatch
{
    public string Player { get; }
    public string Agent { get; }
    public int Kills { get; }
    public int Deaths { get; }
    public int Assists { get; }
    public int Headshots { get; }
    public int RoundsWon { get; }
    public bool Won { get; }

    public ValorantMatch(string player, string agent, int kills, int deaths,
                         int assists, int headshots, int roundsWon, bool won)
    {
        Player = player;
        Agent = agent;
        Kills = kills;
        Deaths = deaths;
        Assists = assists;
        Headshots = headshots;
        RoundsWon = roundsWon;
        Won = won;
    }
}
```

</details>

Tenter de modifier une propriété après création — que se passe-t-il ?

```csharp
var match = new ValorantMatch("Léa", "Jett", 18, 6, 4, 8, 13, true);
match.Kills = 20; // ?
```

> Erreur de compilation — une propriété get-only n'est assignable que dans le constructeur.
> Pour "modifier" une valeur, il faut **reconstruire l'objet entier** en recopiant toutes les
> autres propriétés. C'est verbeux — une meilleure solution sera vue en exercice 07.
> L'essentiel est là : l'original reste intact — premier principe FP appliqué.

---

## Étape 2 — `DataSeries<T>.From` (collection en mémoire)

**Avant de coder :** comment stocker une collection de façon à pouvoir la parcourir ?
Quel type C# représente "une séquence dont on ne connaît pas encore le contenu" ?

<details>
<summary>Indice sur le type à utiliser</summary>

`IEnumerable<DataPoint<T>>` — une séquence de points datés, parcourable sans en connaître
la taille ni le type concret. Un champ `private readonly` empêche toute mutation ultérieure.

</details>

Implémenter dans `DataSeries/DataSeries.cs` :

```csharp
public class DataSeries<T>
{
    private readonly IEnumerable<DataPoint<T>> _data;

    private DataSeries(IEnumerable<DataPoint<T>> data) => // ...

    public static DataSeries<T> From(IEnumerable<DataPoint<T>> source) => // ...

    public int Count => // ...
    public IEnumerable<T> Values             => // ...  (valeurs sans date)
    public IEnumerable<DataPoint<T>> DataPoints => // ...  (valeurs avec date)
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public class DataSeries<T>
{
    private readonly IEnumerable<DataPoint<T>> _data;

    private DataSeries(IEnumerable<DataPoint<T>> data) => _data = data;

    public static DataSeries<T> From(IEnumerable<DataPoint<T>> source)
        => new DataSeries<T>(source);

    public int Count => _data.Count();
    public IEnumerable<T> Values             => _data.Select(dp => dp.Value);
    public IEnumerable<DataPoint<T>> DataPoints => _data;
}
```

> `Values` retourne les valeurs sans date — adapté pour les analyses statistiques.
> `DataPoints` retourne les `DataPoint<T>` complets — adapté pour les requêtes temporelles (exercice 02+).
> Ne pas faire `ToList()` dans le constructeur : la raison sera expliquée en exercice 03 (paresse).

</details>

> **Constructeur privé + `From` — pourquoi ce choix ?**
>
> Le constructeur est `private` : on ne peut pas écrire `new DataSeries<T>(...)` de l'extérieur.
> La seule entrée est la méthode statique nommée `From`.
>
> | Avantages | Inconvénients |
> |-----------|---------------|
> | Le nom exprime l'intention (`From` = "construire depuis une source") | Inhabituel pour un débutant — le constructeur privé surprend |
> | Cohérence avec `FromCsv` (exercice 02) : même API, deux origines | Un niveau d'indirection supplémentaire sans gain immédiat ici |
> | Contrôle total sur la construction (validation, sous-types futurs) | |
>
> **Alternative plus simple** — constructeur public, pas de factory :
>
> ```csharp
> public class DataSeries<T>
> {
>     private readonly IEnumerable<DataPoint<T>> _data;
>
>     public DataSeries(IEnumerable<DataPoint<T>> data) => _data = data;
>
>     public int Count => _data.Count();
>     public IEnumerable<T> Values             => _data.Select(dp => dp.Value);
>     public IEnumerable<DataPoint<T>> DataPoints => _data;
> }
>
> // Utilisation :
> var valorant = new DataSeries<ValorantMatch>(valorantMatches);
> ```
>
> Les deux approches sont valides. Le constructeur privé + `From` sera maintenu dans le fil rouge
> pour la cohérence avec `FromCsv`, mais passer par un constructeur public est tout à fait acceptable.

Vérifier avec trois matchs en dur :

```csharp
var valorantMatches = new[]
{
    new DataPoint<ValorantMatch>(new DateTime(2024, 1, 15), new ValorantMatch("Léa", "Jett",  18, 6, 4, 8,  13, true)),
    new DataPoint<ValorantMatch>(new DateTime(2024, 2,  3), new ValorantMatch("Léa", "Reyna", 22, 8, 2, 11,  9, false)),
    new DataPoint<ValorantMatch>(new DateTime(2024, 3, 10), new ValorantMatch("Léa", "Neon",  20, 7, 5,  9, 13, true)),
};

var valorant = DataSeries<ValorantMatch>.From(valorantMatches);
Console.WriteLine(valorant.Count); // 3
```

---

## Étape 3 — Un seul `DataSeries<T>`, trois jeux (généricité)

**Avant de coder :** CS2 et LoL n'ont pas les mêmes champs que Valorant
(map et côté de départ pour CS2, champion et vision score pour LoL).
Faut-il écrire un `DataSeriesCs2` et un `DataSeriesLol` ?

<details>
<summary>Indice</summary>

Non — `DataSeries<T>` est **générique** : il ignore tout du contenu de `T`.
Il suffit de définir les classes du domaine (`Cs2Match`, `LolMatch`) dans `EsportApp`
et de (ré)utiliser `From`.
→ [Généricité — abstraire les types](../../../../supports/source/01b-genericite.md)

</details>

Définir `Cs2Match` et `LolMatch` (classes immuables, comme `ValorantMatch`) :

```csharp
public class Cs2Match
{
    public string Player { get; }
    public string Map { get; }
    public string StartSide { get; }  // côté joué en 1re mi-temps (CT ou T)
    public int Kills { get; }
    public int Deaths { get; }
    public int Assists { get; }
    public int Mvps { get; }
    public bool Won { get; }

    public Cs2Match(string player, string map, string startSide, int kills,
                    int deaths, int assists, int mvps, bool won)
    { /* ... assignations ... */ }
}

public class LolMatch
{
    public string Player { get; }
    public string Champion { get; }
    public int Kills { get; }
    public int Deaths { get; }
    public int Assists { get; }
    public int Cs { get; }
    public int VisionScore { get; }
    public bool Won { get; }

    public LolMatch(string player, string champion, int kills, int deaths,
                    int assists, int cs, int visionScore, bool won)
    { /* ... assignations ... */ }
}
```

Créer deux ou trois matchs en dur pour chaque jeu et vérifier que le **même** `DataSeries<T>` les accepte :

```csharp
var cs2 = DataSeries<Cs2Match>.From(new[]
{
    new DataPoint<Cs2Match>(new DateTime(2024, 1, 20), new Cs2Match("Raphaël", "Mirage",  "CT", 21, 14, 5, 2, true)),
    new DataPoint<Cs2Match>(new DateTime(2024, 2,  7), new Cs2Match("Kiara",   "Dust2",   "T",  26, 11, 1, 4, true)),
    new DataPoint<Cs2Match>(new DateTime(2024, 3,  1), new Cs2Match("Raphaël", "Inferno", "T",  14, 16, 6, 1, false)),
});

var lol = DataSeries<LolMatch>.From(new[]
{
    new DataPoint<LolMatch>(new DateTime(2024, 1, 22), new LolMatch("Noé", "Thresh", 2, 4, 18, 42, 71, true)),
    new DataPoint<LolMatch>(new DateTime(2024, 2, 10), new LolMatch("Noé", "Thresh", 1, 6, 12, 35, 64, false)),
});

Console.WriteLine($"CS2 : {cs2.Count} matchs, LoL : {lol.Count} matchs"); // 3 et 2
```

Une mini-requête déclarative sur les données en dur :

```csharp
var wins = valorant.Values.Where(m => m.Won);
Console.WriteLine($"Victoires de Léa : {wins.Count()}"); // 2
```

Cette requête est déclarative : elle exprime QUOI faire — pas de boucle, pas de variable muable.
→ [Déclaratif vs Impératif — avec LINQ](../../../../supports/source/01-paradigmes.md#declaratif-vs-imperatif-—-avec-linq)

---

## Étape 4 — Interface CLI

Ajouter dans `Program.cs` la gestion des flags `--help` et `--game`.

**Avant de coder :** Comment détecter la présence d'un flag dans `args` sans librairie externe ?
Comment récupérer la valeur qui suit immédiatement (`--game valorant`) ?
Que doit afficher l'application si aucun argument n'est fourni ?

<details>
<summary>Voir la solution</summary>

```csharp
static void Main(string[] args)
{
    if (args.Length == 0 || args.Contains("--help"))
    {
        Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol]");
        return;
    }

    string? game = null;
    if (args.Contains("--game"))
        game = args[Array.IndexOf(args, "--game") + 1];

    // séries construites avec les matchs en dur des étapes 2 et 3
    if (game == null || game == "valorant")
        Console.WriteLine($"Valorant : {valorant.Count} matchs");
    if (game == null || game == "cs2")
        Console.WriteLine($"CS2      : {cs2.Count} matchs");
    if (game == null || game == "lol")
        Console.WriteLine($"LoL      : {lol.Count} matchs");
}
```

</details>

---

## Vérification

- `valorant.Count` = 3, `cs2.Count` = 3, `lol.Count` = 2 (données en dur)
- Tenter de modifier une propriété → erreur de compilation attendue (immutabilité)
- `DataSeries<T>` ne connaît pas les types `ValorantMatch`, `Cs2Match`, `LolMatch` — généricité validée

---

## Et les vrais CSV ?

Quelques matchs en dur ne suffiront pas au coaching staff — les 75 matchs de la saison
attendent dans `data/`. Mais charger un CSV pour trois formats différents sans dupliquer le code
nécessite de passer une **fonction** en paramètre — c'est l'objet de la thématique 2
et de l'exercice 02.
