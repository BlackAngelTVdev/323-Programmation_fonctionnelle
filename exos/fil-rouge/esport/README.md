# Fil rouge — Team Helvetia

> "Une bibliothèque fonctionnelle est pure par conception. Si `.Normalize()` modifiait
> la série en place, chaque consommateur de ces données se comporterait de manière
> imprévisible. L'immutabilité n'est pas une contrainte — c'est ce qui rend la
> composition possible."

**Team Helvetia** est une organisation esport suisse fictive participant à trois jeux compétitifs.
Le coaching staff analyse les performances de toute l'équipe pour préparer les playoffs.

Trois jeux, trois formats de données différents — même pipeline `DataSeries<T>`.
C'est exactement la raison d'être d'une bibliothèque générique : traiter des sources hétérogènes
avec les mêmes outils.

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
┌──────────────────┐   utilise    ┌───────────────────────┐
│    EsportApp     │ ───────────► │    DataSeries<T>      │
│  (console app)   │              │   (class library)     │
│ connaît l'esport │              │ générique — ignore    │
│ parse les args   │              │ tout du domaine       │
└──────────────────┘              └───────────────────────┘
```

```
Solution/
  DataSeries/                   ← Bibliothèque réutilisable (class library)
    DataPoint.cs                — classe immuable (convertie en record en exercice 07)
    DataSeries.cs               — la bibliothèque principale
    DataSeriesExtensions.cs     — ajouté en exercice 06
  EsportApp/                    ← Application principale (console app)
    Program.cs                  — charge les CSV, utilise DataSeries, affiche les résultats
    MatchGenerator.cs           — générateur de données aléatoires (exercice 02)
```

**Pourquoi cette séparation ?**

- `DataSeries` ignore tout de l'esport — il traite n'importe quel `T` (réutilisable dans P_FUN)
- `EsportApp` connaît le domaine et délègue le traitement
- Les méthodes de la bibliothèque doivent être pures — raison explorée en exercice 07

### Deux domaines, même pipeline

```csharp
// Dataset 1 : GameStats — Scores KDA de joueurs sur une saison
var gameSeries = DataSeries<double>.FromCsv("kda.csv", row => double.Parse(row[2]));

// Dataset 2 : Météo Open-Meteo — Températures sur un an
var weatherSeries = DataSeries<double>.FromCsv("weather.csv", row => double.Parse(row[1]));

// Même pipeline, domaines différents — c'est ça la puissance de l'abstraction fonctionnelle
var result = gameSeries
    .Filter(kda => kda > 2.0)
    .Transform(kda => Math.Round(kda, 2))
    .Statistics();
```

### Interface ligne de commande

L'application se lance avec des arguments — aucune librairie externe, juste des comparaisons
sur le tableau `args` :

```csharp
static void Main(string[] args)
{
    if (args.Length == 0 || args.Contains("--help"))
    {
        Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol] [--player <nom>]");
        Console.WriteLine("                 [--filter wins|losses|all] [--stat kda|kills|assists]");
        Console.WriteLine("                 [--normalize] [--smooth <n>]");
        Console.WriteLine("                 [--error strict|soft|hard] [--window <n>]");
        Console.WriteLine("                 [--rank] [--export <fichier>] [--audit]");
        Console.WriteLine("                 [--bracket <n>] [--generate <joueur|all>]");
        Console.WriteLine("                 [--help] [--version]");
        return;
    }
    // Lire les flags manuellement — args.Contains() et Array.IndexOf()
}
```

Chaque exercice introduit un ou deux nouveaux flags. À la fin de la partie 8,
l'application reconnaît une dizaine d'arguments construits progressivement.

Le texte affiché par `--help` grandit donc d'exercice en exercice : chaque fois qu'un flag
est ajouté, il est aussi documenté dans l'aide. C'est la règle du fil rouge — un flag qui
n'apparaît pas dans `--help` n'existe pas.

---

## Données disponibles

```
data/
  valorant.csv    — 25 matchs, joueurs : Léa + Dylan
  cs2.csv         — 25 matchs, joueurs : Raphaël + Kiara
  lol.csv         — 25 matchs, joueur : Noé
  roster.csv      — les 5 membres de Team Helvetia
```

Chaque CSV a un schéma différent (colonnes propres à chaque jeu).
Le parser est fourni dans la donnée de chaque exercice.

---

## Plan des activités

| Exercice | Thème | Concepts FP |
|----------|-------|-------------|
| [01-equipe-genericite](01-equipe-genericite/) | `DataPoint` + `DataSeries.From` | Immutabilité, généricité |
| [02-recrues-generation](02-recrues-generation/) | `FromCsv` + génération aléatoire | HOF, closures, paresse |
| [03-tri-filter](03-tri-filter/) | `.Filter()` + `.RemoveOutliers()` + paresse | HOF, closures, paresse |
| [04-performance-map](04-performance-map/) | `.Transform()` + `.Normalize()` + `.Smooth()` | Map, composition |
| [05-classement-fold](05-classement-fold/) | `.Fold()` + `.Statistics()` + `.SlidingWindow()` | Fold universel |
| [06-rapport-dsl](06-rapport-dsl/) | Extensions fluentes + DSL | Composition f ∘ g, DSL |
| [07-audit-purete](07-audit-purete/) | Audit de pureté + records + `with` + `.Snapshot()` | Pureté, transparence réf. |
| [08-tournoi-recursion](08-tournoi-recursion/) | `.Decompose()` récursif + bracket de tournoi | Récursion, lien avec Fold |

---

## Glossaire esport

### Général

- **KDA** = (Kills + Assists) / Deaths — mesure d'efficacité individuelle utilisée dans presque
  tous les jeux compétitifs. Un KDA de 3.0 signifie qu'on participe à 3 éliminations par mort.
- **Playoffs** — phase finale d'un tournoi en élimination directe. Une défaite = élimination.
  Contrairement à la saison régulière, chaque match compte double.

### League of Legends (LoL)

LoL est un jeu 5v5 où deux équipes s'affrontent sur une carte en forme de diagonale (la "Rift").
L'objectif est de détruire le Nexus adverse (la base) en progressant à travers trois couloirs (lanes).

- **Champion** — personnage contrôlé par un joueur, avec des capacités uniques. Il en existe
  plus de 160. Noé joue principalement Thresh : un spectre qui brandit une faux et une lanterne.
  Sa chaîne peut attraper un adversaire à distance (hook) ; sa lanterne permet à un allié de
  se téléporter jusqu'à lui pour le sauver ou engager un combat.
- **Support** — rôle dédié à l'assistance. Protège l'ADC (tireur) en bot lane, engage les
  combats, place des wards. Structurellement : peu de kills, beaucoup d'assists, peu de CS.
- **CS (Creep Score)** — nombre de sbires (vagues d'unités neutres) tués. Chaque sbire tué
  rapporte de l'or. Les supports farmant peu (ils laissent les kills aux alliés), leur CS
  est faible : 30–80 par match contre 200–400 pour un ADC ou un Mid.
- **Ward** — objet placé sur la carte qui révèle une zone cachée (buisson, couloir sombre)
  pendant quelques secondes à quelques minutes. Sans wards, l'équipe joue "à l'aveugle" et
  risque les embuscades. Placer et détruire les wards adverses est une compétence à part entière.
  Le `vision_score` mesure la contribution totale à la vision : wards posées, adverses détruites,
  zones révélées. Un support attentif vise 60–85 par match.
- **Gank** — attaque surprise d'un joueur supplémentaire (souvent le Jungler) qui sort de la
  forêt pour surprendre un adversaire sur sa lane. Les wards permettent de les anticiper.
- **ADC** — Attack Damage Carry, le tireur à distance principal, protégé par le Support.

### Counter-Strike 2 (CS2)

CS2 est un jeu de tir 5v5 en rounds. Deux équipes s'affrontent : les Terroristes (T) tentent
de poser une bombe (C4) sur un site A ou B ; les Contre-Terroristes (CT) doivent les en empêcher
ou désamorcer la bombe posée.

- **MR12** — format compétitif standard introduit avec CS2. Chaque équipe joue 12 rounds d'un
  côté, puis les équipes échangent. Premier à 13 rounds gagnés remporte le match.
  Maximum : 24 rounds sans prolongation.
- **`start_side`** — côté joué lors de la 1re mi-temps (CT ou T). Après 12 rounds, les équipes
  échangent. Les statistiques du CSV couvrent le match entier (les deux mi-temps combinées).
- **CT / T** — Contre-Terroriste (défense, désamorçage) / Terroriste (attaque, pose de bombe).
  Certaines maps favorisent un côté : Dust2 est réputée favorable au T, Nuke au CT.
- **AWPer** — joueur spécialisé avec l'AWP, sniper à élimination en 1 tir sur le corps.
  Style radicalement différent du rifler : positionnement statique, contrôle d'angles, impact
  immédiat mais risque élevé (l'AWP coûte très cher — une mort = argent perdu pour l'équipe).
  Kiara incarne ce rôle : peu de matchs "moyens", beaucoup de très bons ou de difficiles.
- **Rifler** — joueur polyvalent armé d'un fusil automatique (AK-47 côté T, M4 côté CT).
  Mobile, prend des duels dynamiques, joue dans les angles. Raphaël incarne ce rôle.
- **MVP** — Most Valuable Player du round. Décerné au joueur à l'impact décisif (kill final,
  désamorçage, pose de bombe qui déclenche la victoire). 0–5 par match pour un top fragger.
- **Clutch** — situation où un joueur se retrouve seul contre plusieurs adversaires et parvient
  à gagner le round. Moment spectaculaire, souvent décisif pour le moral de l'équipe.
- **Economy round** — round joué avec un budget réduit (armes secondaires, parfois couteaux)
  pour économiser et acheter un équipement complet au round suivant.

### Valorant

Valorant est un jeu de tir 5v5 par Riot Games, similaire à CS2 dans sa structure (rounds,
attaque/défense, switch de côté) mais avec des agents aux capacités spéciales.

- **First to 13** — même logique que CS2 : premier à 13 rounds gagnés l'emporte.
  Maximum 25 rounds en match standard, plus en overtime (alternance 1v1 par paire).
- **Spike** — l'objectif du jeu (équivalent de la bombe C4 en CS2). Les Attaquants la
  transportent et doivent la poser sur un site ; les Défenseurs l'empêchent ou la désamorcent.
- **Agent** — personnage aux capacités uniques (contrairement à CS2 où tous les joueurs sont
  identiques). Chaque agent appartient à une classe de rôle.
- **Duelist** — rôle offensif conçu pour prendre les duels 1v1 et ouvrir les sites en premier.
  Léa joue Jett (dash instantané, couteaux en ultime), Reyna (invulnérabilité et soin après kill)
  et Neon (sprint, éclairs). Kills élevés, style agressif.
- **Controller** — rôle de contrôle de zone. Place des fumigènes, ralentisseurs et barrières
  pour bloquer la vision adverse et diviser la carte. Dylan joue Omen (fumigènes longue portée
  et téléportation), Astra (fumigènes et aspiration à l'échelle de la carte) et Brimstone
  (fumigènes précis via tablette). Moins de kills que le Duelist, mais plus d'assists.
- **Headshot** — tir à la tête, élimine la plupart des agents en 1 balle. Indicateur de
  précision. Le taux headshots/kills distingue un joueur précis d'un "spray player".
- **Rounds Won** — nombre de rounds remportés par l'équipe dans le match. Si `won=true`, vaut
  toujours 13 (victoire standard). Si `won=false`, entre 0 et 12 (défaite).
- **Ace** — éliminer les 5 adversaires seul dans un même round. Moment rare et spectaculaire.
