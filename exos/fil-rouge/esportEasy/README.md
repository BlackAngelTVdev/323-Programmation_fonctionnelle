# Fil rouge — Team Helvetia (version Interface)

> "Une bibliothèque fonctionnelle est pure par conception. Si `.Normalize()` modifiait
> la série en place, chaque consommateur de ces données se comporterait de manière
> imprévisible. L'immutabilité n'est pas une contrainte — c'est ce qui rend la
> composition possible."

**Team Helvetia** est une organisation esport suisse fictive participant à trois jeux compétitifs.
Le coaching staff analyse les performances de toute l'équipe pour préparer les playoffs.

Ce fil rouge couvre exactement les **mêmes concepts FP** que le fil rouge [esport](../esport/)
— sans la généricité. L'interface `IMatchData` joue le rôle que `<T>` joue dans `DataSeries<T>` :
c'est le point de variation, le contrat commun à tous les types de matchs.

---

## Pourquoi cette version ?

Dans le fil rouge `esport`, deux abstractions sont introduites simultanément :

- la **généricité** (`DataSeries<T>`, `DataPoint<T>`, `Transform<TResult>`)
- les **concepts FP** (HOF, closures, paresse, pureté, récursion)

`esportEasy` supprime la couche générique : les types sont concrets et lisibles.
La même progression pédagogique, avec moins de bruit syntaxique au départ.

---

## L'équipe

Le **roster** désigne la liste officielle des joueurs alignés par une organisation esport.

| Joueur  | Jeu      | Rôle       | Ville    |
|---------|----------|------------|----------|
| Léa     | Valorant | Duelist    | Lausanne |
| Raphaël | CS2      | Rifler     | Genève   |
| Noé     | LoL      | Support    | Berne    |
| Dylan   | Valorant | Controller | Fribourg |
| Kiara   | CS2      | AWPer      | Zurich   |

Les données de Léa couvrent toute la saison. Raphaël, Noé, Dylan et Kiara n'ont rejoint
l'équipe qu'en cours de saison — leurs matchs précédents seront simulés en exercice 02.

---

## Architecture du projet

```
┌──────────────────┐   utilise    ┌─────────────────────────────┐
│    EsportApp     │ ───────────► │   MatchSeries               │
│  (console app)   │              │ + StatSeries                │
│ connaît l'esport │              │   (class library)           │
│ parse les args   │              │   contrat : IMatchData      │
└──────────────────┘              └─────────────────────────────┘
```

```
Solution/
  DataSeries/                   ← Bibliothèque réutilisable (class library)
    IMatchData.cs               — contrat commun à tous les types de matchs
    MatchSeries.cs              — série de IMatchData (remplace DataSeries<Match>)
    StatSeries.cs               — série de doubles (remplace DataSeries<double>)
    StatSeriesExtensions.cs     — ajouté en exercice 06
  EsportApp/                    ← Application principale (console app)
    Program.cs                  — charge les CSV, utilise MatchSeries, affiche les résultats
    ValorantMatch.cs            — implémente IMatchData
    Cs2Match.cs                 — implémente IMatchData
    LolMatch.cs                 — implémente IMatchData
    MatchGenerator.cs           — générateur de données aléatoires (exercice 02)
```

**Pourquoi cette séparation ?**

- `MatchSeries` ne connaît que `IMatchData` — les types concrets restent dans `EsportApp`
- `EsportApp` connaît le domaine et délègue le traitement
- Les méthodes de la bibliothèque doivent être pures — raison explorée en exercice 07

### Deux jeux, même pipeline

```csharp
// Dataset 1 : Valorant — matchs de Léa et Dylan
var valorant = MatchSeries.FromCsv("valorant.csv", cols => new ValorantMatch(...));

// Dataset 2 : CS2 — matchs de Raphaël et Kiara
var cs2 = MatchSeries.FromCsv("cs2.csv", cols => new Cs2Match(...));

// Même pipeline, types différents — c'est le rôle de l'interface
var result = valorant
    .Filter(m => m.Won)
    .Extract(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths))
    .Statistics();
```

---

## Comparaison avec esport

| esport | esportEasy | Trade-off |
|--------|-----------|-----------|
| `DataPoint<T>` — valeur + date dans un wrapper | Date est un champ de `IMatchData` | Plus simple, mais `IMatchData` doit toujours avoir `Date` |
| `DataSeries<T>` — fonctionne pour tout `T` | `MatchSeries` — uniquement `IMatchData` | Pas de réutilisation cross-domaine (météo, bourse...) |
| `Transform<TResult>(Func<T,TResult>)` → `DataSeries<TResult>` | `Extract(Func<IMatchData,double>)` → `StatSeries` | Seulement des extractions vers `double`, pas vers un type arbitraire |
| `Fold<TResult>(seed, combiner)` — accumulateur générique | `Fold(double, Func<double,double,double>)` — doubles seulement | Plus simple, mais ne peut pas accumuler vers un type non-double |
| Réutilisable pour météo (même `DataSeries<double>`) | Météo nécessiterait une interface séparée | Moindre réutilisabilité |

---

## Données disponibles

Les données sont partagées avec le fil rouge esport — aucune duplication :

```
../esport/data/
  valorant.csv    — 25 matchs, joueurs : Léa + Dylan
  cs2.csv         — 25 matchs, joueurs : Raphaël + Kiara
  lol.csv         — 25 matchs, joueur : Noé
  roster.csv      — les 5 membres de Team Helvetia
```

---

## Plan des activités

| Exercice | Thème | Concepts FP |
|----------|-------|-------------|
| [01-equipe-interface](01-equipe-interface/) | `IMatchData` + `MatchSeries.From` | Immutabilité, interface |
| [02-recrues-generation](02-recrues-generation/) | `FromCsv` + génération aléatoire | HOF, closures, paresse |
| [03-tri-filter](03-tri-filter/) | `.Filter()` + `.RemoveOutliers()` + paresse | HOF, closures, paresse |
| [04-performance-map](04-performance-map/) | `.Extract()` + `StatSeries.Normalize()` + `.Smooth()` | Map, composition |
| [05-classement-fold](05-classement-fold/) | `.Fold()` + `.Statistics()` + `.SlidingWindow()` | Fold universel |
| [06-rapport-dsl](06-rapport-dsl/) | Extensions fluentes + DSL | Composition f ∘ g, DSL |
| [07-audit-purete](07-audit-purete/) | Audit de pureté + records + `with` + `.Snapshot()` | Pureté, transparence réf. |
| [08-tournoi-recursion](08-tournoi-recursion/) | `.Decompose()` récursif + bracket de tournoi | Récursion, lien avec Fold |

---

Pour la version complète avec généricité → [esport](../esport/)
