# Exercice 01 — L'équipe

> Partie 1 — `IMatchData` + `MatchSeries.From`

## Concepts théoriques

- [Thématique 01 — Paradigmes fonctionnels](../../../../thematiques/01-paradigmes-fonctionnels.md)
- [Impératif ou déclaratif](../../../../supports/source/01-paradigmes.md#imperatif-ou-declaratif)
- Immutabilité (propriétés get-only) — introduction, approfondie en exercice 07

## Contexte

Le manager de Team Helvetia modélise le roster pour le prochain tournoi.
L'équipe joue dans trois jeux différents — les matchs Valorant, CS2 et LoL n'ont pas
les mêmes caractéristiques. Un outil capable de traiter les trois formats est nécessaire.

C'est le rôle de `IMatchData` : une interface qui définit le **contrat commun** à tous les types
de matchs. `MatchSeries` stocke des `IMatchData` — le type concret (Valorant, CS2, LoL) est
inconnu de la bibliothèque.

Deux projets, deux responsabilités :

```
┌──────────────────┐   utilise    ┌─────────────────────────────┐
│    EsportApp     │ ───────────► │   MatchSeries               │
│  (console app)   │              │   (class library)           │
│ connaît l'esport │              │   connaît IMatchData        │
│ parse les args   │              │   ignore les types concrets │
└──────────────────┘              └─────────────────────────────┘
```

---

## Étape 1 — Définir le contrat `IMatchData`

**Avant de coder :** quels champs sont communs à Valorant, CS2 et LoL ?
Ouvrir [../esport/data/valorant.csv](../esport/data/valorant.csv) et
[../esport/data/cs2.csv](../esport/data/cs2.csv) : quels noms de colonnes apparaissent dans les deux ?

<details>
<summary>Voir les champs communs</summary>

- `Date` (DateTime) — quand le match a eu lieu
- `Player` (string) — nom du joueur
- `Kills` (int) — éliminations
- `Deaths` (int) — morts
- `Assists` (int) — assistances
- `Won` (bool) — victoire ou défaite

Chaque jeu a en plus des champs propres (`Agent` pour Valorant, `Map` pour CS2, `Champion` pour LoL).
Ces champs spécifiques ne font pas partie de l'interface.

</details>

Créer `DataSeries/IMatchData.cs` :

```csharp
public interface IMatchData
{
    DateTime Date    { get; }
    string   Player  { get; }
    int      Kills   { get; }
    int      Deaths  { get; }
    int      Assists { get; }
    bool     Won     { get; }
}
```

`IMatchData` définit QUOI tout match doit exposer — pas COMMENT le stocker.
Chaque jeu implémente l'interface à sa façon : Valorant a `Agent`, CS2 a `Map`, etc.

---

## Étape 2 — `ValorantMatch : IMatchData` (classe immuable)

**Avant de coder :** ouvrir [../esport/data/valorant.csv](../esport/data/valorant.csv).
Quels champs sont spécifiques à Valorant (absents de l'interface) ?

<details>
<summary>Voir les champs spécifiques à Valorant</summary>

- `Agent` (string) — personnage joué (Jett, Reyna, Neon...)
- `Headshots` (int) — tirs à la tête
- `RoundsWon` (int) — rounds remportés par l'équipe

</details>

Créer `EsportApp/ValorantMatch.cs` :

```csharp
public class ValorantMatch : IMatchData
{
    public DateTime Date      { get; }
    public string   Player    { get; }
    public string   Agent     { get; }
    public int      Kills     { get; }
    public int      Deaths    { get; }
    public int      Assists   { get; }
    public int      Headshots { get; }
    public int      RoundsWon { get; }
    public bool     Won       { get; }

    public ValorantMatch(DateTime date, string player, string agent, int kills,
                         int deaths, int assists, int headshots, int roundsWon, bool won)
    {
        Date      = date;
        Player    = player;
        Agent     = agent;
        Kills     = kills;
        Deaths    = deaths;
        Assists   = assists;
        Headshots = headshots;
        RoundsWon = roundsWon;
        Won       = won;
    }
}
```

Les propriétés sont **get-only** : une fois l'objet construit, rien ne peut plus changer.

Tenter de modifier une propriété après création — que se passe-t-il ?

```csharp
var match = new ValorantMatch(new DateTime(2024, 1, 15), "Léa", "Jett", 18, 6, 4, 8, 13, true);
match.Kills = 20; // ?
```

> Erreur de compilation — une propriété get-only n'est assignable que dans le constructeur.
> Pour "modifier" une valeur, il faut reconstruire l'objet entier. C'est verbeux —
> une meilleure solution sera vue en exercice 07. L'essentiel est là : l'original reste
> intact — premier principe FP appliqué.

---

## Étape 3 — `MatchSeries.From` (collection en mémoire)

**Avant de coder :** comment stocker une collection de `IMatchData` de façon à pouvoir
la parcourir sans en connaître le type concret ?

<details>
<summary>Indice sur le type à utiliser</summary>

`IEnumerable<IMatchData>` — une séquence parcourable d'objets implémentant l'interface.
Un champ `private readonly` empêche toute mutation ultérieure.

</details>

Créer `DataSeries/MatchSeries.cs` :

```csharp
public class MatchSeries
{
    private readonly IEnumerable<IMatchData> _data;

    private MatchSeries(IEnumerable<IMatchData> data) => _data = data;

    public static MatchSeries From(IEnumerable<IMatchData> source) => new(source);

    public int Count                       => _data.Count();
    public IEnumerable<IMatchData> Matches => _data;
}
```

> **Constructeur privé + `From` — pourquoi ce choix ?**
>
> Le constructeur est `private` : on ne peut pas écrire `new MatchSeries(...)` de l'extérieur.
> La seule entrée est la méthode statique nommée `From`.
>
> | Avantages | Inconvénients |
> |-----------|---------------|
> | Le nom exprime l'intention (`From` = "construire depuis une source") | Inhabituel pour un débutant — le constructeur privé surprend |
> | Cohérence avec `FromCsv` (exercice 02) : même API, deux origines | Un niveau d'indirection supplémentaire |
> | Contrôle total sur la construction | |

Vérifier avec trois matchs en dur :

```csharp
var valorantMatches = new IMatchData[]
{
    new ValorantMatch(new DateTime(2024, 1, 15), "Léa", "Jett",  18, 6, 4, 8,  13, true),
    new ValorantMatch(new DateTime(2024, 2,  3), "Léa", "Reyna", 22, 8, 2, 11,  9, false),
    new ValorantMatch(new DateTime(2024, 3, 10), "Léa", "Neon",  20, 7, 5,  9, 13, true),
};

var valorant = MatchSeries.From(valorantMatches);
Console.WriteLine(valorant.Count); // 3
```

---

## Étape 4 — Un seul `MatchSeries`, trois jeux (interface)

**Avant de coder :** CS2 et LoL n'ont pas les mêmes champs que Valorant.
Faut-il écrire un `MatchSeriesCs2` et un `MatchSeriesLol` ?

<details>
<summary>Indice</summary>

Non — `MatchSeries` stocke des `IMatchData` : il ignore tout du type concret.
Il suffit de définir `Cs2Match` et `LolMatch` dans `EsportApp` et de (ré)utiliser `From`.

</details>

Créer `EsportApp/Cs2Match.cs` et `EsportApp/LolMatch.cs` :

```csharp
public class Cs2Match : IMatchData
{
    public DateTime Date      { get; }
    public string   Player    { get; }
    public string   Map       { get; }
    public string   StartSide { get; }
    public int      Kills     { get; }
    public int      Deaths    { get; }
    public int      Assists   { get; }
    public int      Mvps      { get; }
    public bool     Won       { get; }

    public Cs2Match(DateTime date, string player, string map, string startSide,
                    int kills, int deaths, int assists, int mvps, bool won)
    { /* ... assignations ... */ }
}

public class LolMatch : IMatchData
{
    public DateTime Date        { get; }
    public string   Player      { get; }
    public string   Champion    { get; }
    public int      Kills       { get; }
    public int      Deaths      { get; }
    public int      Assists     { get; }
    public int      Cs          { get; }
    public int      VisionScore { get; }
    public bool     Won         { get; }

    public LolMatch(DateTime date, string player, string champion, int kills,
                    int deaths, int assists, int cs, int visionScore, bool won)
    { /* ... assignations ... */ }
}
```

Vérifier que le **même** `MatchSeries` accepte les trois types :

```csharp
var cs2 = MatchSeries.From(new IMatchData[]
{
    new Cs2Match(new DateTime(2024, 1, 20), "Raphaël", "Mirage",  "CT", 21, 14, 5, 2, true),
    new Cs2Match(new DateTime(2024, 2,  7), "Kiara",   "Dust2",   "T",  26, 11, 1, 4, true),
    new Cs2Match(new DateTime(2024, 3,  1), "Raphaël", "Inferno", "T",  14, 16, 6, 1, false),
});

var lol = MatchSeries.From(new IMatchData[]
{
    new LolMatch(new DateTime(2024, 1, 22), "Noé", "Thresh", 2, 4, 18, 42, 71, true),
    new LolMatch(new DateTime(2024, 2, 10), "Noé", "Thresh", 1, 6, 12, 35, 64, false),
});

Console.WriteLine($"CS2 : {cs2.Count} matchs, LoL : {lol.Count} matchs"); // 3 et 2
```

Une mini-requête déclarative sur les données en dur :

```csharp
var wins = valorant.Matches.Where(m => m.Won);
Console.WriteLine($"Victoires de Léa : {wins.Count()}"); // 2
```

Cette requête est déclarative : elle exprime QUOI faire — pas de boucle, pas de variable muable.
→ [Déclaratif vs Impératif — avec LINQ](../../../../supports/source/01-paradigmes.md#declaratif-vs-imperatif-—-avec-linq)

> **Comparaison avec esport :** Dans le fil rouge `esport`, le `T` de `DataSeries<T>`
> joue le même rôle que l'interface `IMatchData` ici. `DataSeries<ValorantMatch>`,
> `DataSeries<Cs2Match>` et `DataSeries<LolMatch>` sont trois instances du même type
> générique. Ici, `MatchSeries` est un type unique qui accepte tout `IMatchData`.
> Les deux approches visent le même objectif : **un seul pipeline pour plusieurs formats**.

---

## Étape 5 — Interface CLI

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

    // séries construites avec les matchs en dur des étapes précédentes
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
- `MatchSeries` ne connaît pas les types `ValorantMatch`, `Cs2Match`, `LolMatch` — interface validée

---

## Et les vrais CSV ?

Quelques matchs en dur ne suffiront pas au coaching staff — les 75 matchs de la saison
attendent dans `../esport/data/`. Mais charger un CSV pour trois formats différents sans dupliquer
le code nécessite de passer une **fonction** en paramètre — c'est l'objet de la thématique 2
et de l'exercice 02.
