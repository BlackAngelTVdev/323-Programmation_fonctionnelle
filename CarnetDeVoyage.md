# Carnet de voyage

Du 24 août au 30 octobre 2026, la classe CMID3b et moi avons parcouru un chemin d'apprentissage de la programmation fonctionnelle.
Ce document relate les péripéties de ce voyage.

<hr>

## Etape 1

### Lundi 24 août

- On a découvert le thème du projet Plot Those Lines, chacun a choisi un domaine
- Tout le monde a référencé son repo dans MarketPlace. Certains doivent encore ajouter un Readme.
- On a passé en revue le [Project Handbook](https://github.com/XCarrel/Project-Handbook/tree/T1_2026-2027_P_FUN_P_OO)

### Mardi

- J'ai publié et annoncé la version 1.3 de gistodoc, pour importer les issues Github dans un document Word

### Mercredi 26 août

Notre but c'est qu'au bout de cette étape:

- On a vu les [objectifs formels ICT](https://www.modulbaukasten.ch/module/323/1/fr-FR?title=Programmer-de-mani%C3%A8re-fonctionnelle)
- On a survolé le [parcours](https://roadmap.sh/r/embed?id=66b88565b64402e0526d8ebc) qui nous attend
- On est d'accord sur les [modalités d&#39;évaluation](evaluation/DEP.md) du module
- On a vérifié nos paquetage de départ, en [révisant les concepts OO](exos/consolidation-OO/).
  - La terminologie : c'était une bonne chose qu'on le fasse, plusieurs termes n'étaient pas vraiment maîtrisés
  - Un Dojo pratique : on n'a pas été au bout du programme, mais plein de questions importantes ont été remontées et discutées. On part avec une bonne base.

- On s'est attaqué à une thématiques: les [paradigmes de programmation](https://github.com/XCarrel/323-Programmation_fonctionnelle/blob/main/supports/source/01-paradigmes.md)
- On a revu la [manière de suivre un cours](USEME.md) avec Github et chacun mis en place son fork de [mon repo](https://github.com/XCarrel/323-Programmation_fonctionnelle), dans lequel il a créé son espace personnel. J'ai les références de tous les forks. @Albert, @Snehan : je ne vois pas votre espace personnel dans le repo
- On a étudié une deuxième thématique: [généricité](https://etml-inf.github.io/323-Programmation_fonctionnelle/supports/source/01b-genericite.html) (en matière de programmation). C'est complexe, la pente d'apprentissage est raide.
- On a commencé la mise en oeuvre la généricité avec [l&#39;exercice 01](https://etml-inf.github.io/323-Programmation_fonctionnelle/exos/fil-rouge/esport/01-equipe-genericite/) du fil rouge. On a vu comment
  - Structurer une application console avec deux projet (un programme et une librairie)
  - Déclarer notre première classe générique

On n'est pas arrivé au bout de l'exercice, on reprendra ça la semaine prochaine

<hr>

## Etape 2

### Lundi 31 août

La mission du jour était: finaliser l'analyse fonctionnelle et la planification initiale. Idéalement, il aurait dû être possible de faire la livraison de vendredi dès aujourd'hui en fin de matinée.

J'ai passé vers à peu près tout le monde. Le constat est que la rédaction de User Stories n'est pas encore maîtrisée, personne n'ayant pu me montrer une user story bien formulée du premier coup.Je tiens à ce que le codage ne commence qu'à partir du moment où au moins une US est bien formulée. Je préfère avoir un petit nombre de US de bonne qualité que beaucoup de US de mauvaise qualité. Conséquence:

> Pour la livraison du 4 septembre, je ne damande pas une analyse fonctionnelle complète. Je n'attends que une ou deux US totalisant au minimum 5 bons tests d'acceptance

N'ayant reçu aucune livraison pour l'instant (mardi), j'en déduis que tout le monde a encore du travail à fournir pour arriver à ce stade.

### Mercredi 2 septembre

On a fait le checkpoint #1. Les résultats sont ... moyens. En même temps, très peu étaient ceux qui avaient révisé.

On a vu ensemble la solution du début de l'étape 1 de l'application ESportApp, dans laquelle on sépare clairement les aspects métiers et les "logistique":

- Le projet `ESportApp` contient des classes propres au domaine (métier): `CS2Match`, `LolMatch`, `ValorantMatch`)
- Le projet `Dataseries` contient les moyens de gérer des séries de `<un_certain_type>`

On a vu qu'avec C#, on peut aussi **stocker une fonction dans une variable** .

Et on a vu comment **donner une fonction comme argument à une autre fonction**. on a vu la théorie des [fonctions d&#39;ordre supérieur](./supports/source/02a-fonctions-sup.md)

On s'est attaqué à l'étape 1 de l'[exercice 2](./exos/fil-rouge/esport/02-recrues-generation/README.md) du fil rouge, dans laquelle on importe les données à partir de fichiers CSV en s'appuyant sur des fonctions spécialisées pour parser les données.

C'est dur de passer de la théorie au code. Je mets [ma solution](./exos/fil-rouge/esport/ESportApp/) à disposition comme aide/tuteur.

On a expliqué rapidement les concepts de lazy/eager loading et classes d'extension.

On a commencé à jouer avec nos statistiques grâce aux premières méthodes LinQ de la [cheatsheet](./supports/linq-cheatsheet.pdf): combien de victoires ? A quand remonte la dernière défaite de Léa ? Quelles sont les stats du troisième match de Dylan ? ...

On ne comprenait pas pourquoi on n'arrivait pas à compter les victoires de Léa.
Ce mystère est élucidé: le premier test (`cols[8] == 'TRUE'`), tout comme le deuxième (`cols[8] == 'VRAI'`) échouaient parce que la valeur dans le fichier est `true` en minuscule. Nous avons en plus été mystifié par Excel qui se croit toujours plus malin que nous: il lit `true` dans le fichier, mais il affiche `VRAI` parce qu'il est configuré en français.
`bool.Parse(cols[8])` suggéré par Tony est une meilleure approche, mais attention car il ne tient pas compte de la locale et ça se crashe si on met `VRAI` dans le fichier.

Les dix dernières minutes se passent "en roue libre". J'ai l'impression qu'il y a saturation.

<hr>

## Etape 3

### Lundi 7 septembre

Rappel de l'une des valeurs Agile:

> L'adaptation plutôt que l'exécution d'un plan

Mon plan était: évaluer de menière définitive le critère "Expression" après la première livraison. Je m'attendais à ce que seulement un ou deux n'aient pas atteint le niveau attendu et j'aurais envisagé une possibilité de remédiation en fin de projet.
Mais là, si j'exécute ce plan, la majorité de la classe serait déjà en non-acquis. Cela n'est pas productif. Je réévaluerai le début du rapport.

J'ai ajouté des commentaires sur MarketPlace, prenez-en connaissance.

Faire un projet en méthodologie Agile ne veut pas dire avoir carte blanche pour faire comme on veut.
J'attire votre attention sur la formulation "plutôt que" dans les valeurs agiles. "Un logiciel fonctionnel plutôt qu'une documentation exhaustive" ne veut pas dire que seul le résultat compte.

Tenez compte de ces retours, parce que:

> Ne pas tenir compte de consignes et feedback répétés peut coûter un joker

J'ai présenté [gistodoc](https://github.com/ETML-INF/gistodoc), pour éviter que du travail soit fait à double, dans Github et dans le rapport.

J'ai pu discuter 1-1 avec chacun. Malheureusement, MarketPlace était instable, et je n'ai pas pu tout noter ce qui a été discuté

Mais tout le monde a une story en cours de réalisation.

### Mercredi 9 septembre

On a fait le checkpoint #2 sur les fonctions d'ordre supérieur et des première méthodes d'extension LinQ.

On a grimpé une pente bien raide la semaine passée. Cette semaine, on n'a pas abordé de sujet théorique fondamental.
L'objectif général de la semaine: savoir **transformer** les objets d'une liste.

Mais d'abord, synchronisation ...

J'observe des usages du repo du cours très variables: de "aucune activité" pour certains à "plusieurs coups d'avance" pour d'autres. J'ai de la peine à savoir où vous en êtes. Chacun a créé un fichier `PointDeSituation.md` dans son dossier perso, commit/push.

On a revu ensemble encore une fois la solution fonctionnelle avec l'import des fichiers CSV. Je considère que ce chapitre là est clos.

On a discuté des méthodes de génération (`Range`) et de transformation (`Select`) sur la base la cheatsheet.

On discute des méthodes de génération (`Range` dans la cheatsheet) et de transformation (`Select` dans les [slides](./slides/03-map.md)) .
La mission donnée était:

- Vous utilisez `Range` et `Random` (bien connu) pour réaliser l'[étape 2](https://github.com/XCarrel/323-Programmation_fonctionnelle/tree/main/exos/fil-rouge/esport/02-recrues-generation).

- Vous utilisez `Select` pour refaire des lignes de CSV à partir de chaque objet et enregistrer les données générées dans vos fichiers CSV ([étape 3](https://github.com/XCarrel/323-Programmation_fonctionnelle/tree/main/exos/fil-rouge/esport/02-recrues-generation)).

- Exercez les fonctions de tri avec l'[exercice 3](./exos/fil-rouge/esport/03-tri-filter/README.md)

- N'oubliez pas de semer des petits cailloux blancs le long de votre chemin (commits), plus précisément:
  - feat(ESportApp): générer 20 matchs pour Raphael
  - feat(ESportApp): sauver les fichiers, CSV
  - feat(ESportApp): ajouter les commandes CLI pour générer
  - feat(ESportApp): détecter les outliers
  - feat(ESportApp): traiter les cas d'erreurs
  - feat(ESportApp): ajouter les commandes CLI pour le traitement des erreurs

Au final, une petite moitié de la classe a commencé à faire le traitement d'erreur.  
À moins d'évaluer cela un petit peu plus finement à partir des commits effectués

<hr>

## Etape 4

### Lundi 14 septembre

"Discussion" au sujet de l'absence de mise à jour du journal de travail suite au constat suivant:

```
albert          2026-09-04 10:26     feat(readme): ajout du rapport
damienc         2026-09-07 09:26     feat(P_FUN): Ajout structure du projet
damienr         2026-09-07 11:09     WIP chore(scripts): ajoute le script get_commit
erdem           2026-09-04 23:12     doc(jdt) Ajout Jdt
gianmarco       2026-09-07 11:25     fix(Importation): Importation des données
gillian         2026-09-07 10:59     feat(front): Ajout d'un frontend de base pour l'application
kiril           2026-09-07 11:24     feat : ajout de composant pour import des data [WIP]
sacha           2026-09-07 10:29     feat(PROJET) Création du library DataPoint et DataSerie
snehan          2026-09-04 09:12     docs(Rapport, jdt): Correction du rapport en accord avec les remarques de l'enseignant
theophile       2026-09-07 10:06     add base avalonia template
tony            2026-09-07 09:27     Auto-generate files via JDT-Generator
zidane          2026-09-07 11:27     doc(jdt): Mise à jour du jdt et création du doc
```

"Discussion", entre guillemets, parce que ça ne va que dans un sens: personne ne réagit quand je dis que je ne trouve pas cela normal. Personne ne réagit non plus quand je demande si quelqu'un trouve que je suis trop exigeant.

Malheureusement, je dois constater dans l'après-midi que le message n'a toujours pas passé auprès de certains (3) élèves.

J'ai ajouté des remarques dans MarketPlace.  
Ceux qui n'en n'ont pas ou peu = bonne nouvelle, ça se passe bien.  
Les autres peuvent - s'ils le désirent - réagir à la suite de mes commentaires.

Un constat général : maintenant que le code a commencé, je veux pouvoir l'exécuter. Plusieurs ont choisi de partir sur Avalonia et je n'ai pas suffisamment d'informations à ma disposition pour savoir comment faire.

### Mercredi 16 septembre

On est en effectif réduit aujourd'hui: huit sur douze.

On a fait le checkpoint #3

Ensuite on a fait le point sur les exercices proposés la semaine passée:

- L'exercice 2 porte sur la génération et la transformation
  - 2.0 `DataSeries<T>` comme vraie série temporelle
  - 2.1 Parser les fichiers CSV
  - 2.1 Générer des matches (Range)
  - 2.2 Sauver en CSV (Select)
  - 2.3 CLI pour demander la génération
- L'exercice 3 porte sur le filtrage
  - 3.1 Détecter les erreurs (Where)
  - 3.2 Supprimer les erreurs (Where)
  - 3.3 CLI pour définir le comportement face aux erreurs

La synthèse est validée (pour les présents):

|           | Présent |  2.1  |  2.2  |  2.3  |  2.4  |  3.1  |  3.2  |  3.3  |
| --------- | :-----: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| albert    |   \*    |  OK   |       |       |       |       |       |       |
| damienc   |   \*    |  OK   |  OK   |  OK   |       |       |       |       |
| damienr   |   \*    |  OK   |  OK   |  OK   |  OK   |       |       |       |
| erdem     |         |  OK   |  OK   |  OK   |       |       |       |       |
| gianmarco |   \*    |  OK   |  OK   |  OK   |       |       |       |       |
| gillian   |         |  OK   |  OK   |       |       |       |       |       |
| kiril     |   \*    |  OK   |  OK   |  OK   |       |       |       |       |
| sacha     |   \*    |  OK   |  OK   |       |  OK   |       |       |       |
| snehan    |   \*    |  OK   |  OK   |  OK   |       |       |       |       |
| theophile |         |  OK   |  OK   |       |  OK   |       |       |       |
| tony      |   \*    |  OK   |  OK   |  OK   |  OK   |       |       |       |
| zidane    |         |  OK   |  OK   |       |       |       |       |       |

On a une petite heure (jusqu'à la pause) pour finir ces exercices.

Mais avant cela, certains ont dû défaire quelques changements demandés précédemment 😤.  
En effet, au début de l'étape 2, nous avions voulu intégrer un Timestamp pour avoir de vraies Timeseries. Nous avions passé de `private readonly IEnumerable<T> _data;` à `private readonly IEnumerable<DataPoint<T>> _data;`. Cela a introduit un degré de complexité supplémentaire significatif pour la suite.  
Le retour en arrière se fait ainsi:

- Revenir à `private readonly IEnumerable<T> _data;`
- Supprimer la classe `DataPoint<T>`
- Ajouter une propriété `public DateTime Timestamp { get; }` aux trois types de matches
- Corriger toutes les erreurs que cela cause

Rappel au moment de s'y mettre: faire apparaître le numéro de l'étape dans le nom des commits, p.ex.: `feat(ESportApp): Réaliser l'étape 2.2 (Sauver en CSV)`

Après la pause on a regardé l'énoncé de l'[exercice 4](./exos/fil-rouge/esport/04-performance-map/), qui a pour but de consolider le concept de transformation en générant des **indicateurs** à partir de nos données.

Petit moment d'explication sur les paramètre CLI:

- Comment les passer depuis Visual Studio (Déboguer > Propriété de débogage > Paramètres de ligne de commande)
- Comment les traiter `static void Main(string[] args)`

Situation en fin de matinée:
|           | Présent |  2.1  |  2.2  |  2.3   |  2.4   |  3.1   |  3.2   |  3.3   |  4.1   |  4.2   |  4.3   |  4.4   |
| --------- | :-----: | :---: | :---: | :----: | :----: | :----: | :----: | :----: | :----: | :----: | :----: | :----: |
| albert    |    _    |  ok   |       | **OK** |        |        |   KO   |        |        |        |        |        |
| damienc   |    _    |  ok   |  ok   |   ok   | **OK** | **OK** |        |        |        |        |        |        |
| damienr   |    _    |  ok   |  ok   |   ok   |   ok   | **OK** | **OK** | **OK** | **OK** | **OK** | **OK** | **OK** |
| erdem     |         |  ok   |  ok   |   ok   |        |        |        |        |        |        |        |        |
| gianmarco |    _    |  ok   |  ok   |   ok   |        | **OK** | **OK** |        |        |        |        |        |
| gillian   |         |  ok   |  ok   |        |        |        |        |        |        |        |        |        |
| kiril     |    _    |  ok   |  ok   |   ok   | **OK** | **OK** | **OK** |        |        |        |        |        |
| sacha     |    _    |  ok   |  ok   |        |   ok   | **OK** | **OK** | **OK** |        |        |        |        |
| snehan    |    _    |  ok   |  ok   |   ok   | **OK** | **OK** | **OK** |        |        |        |        |        |
| theophile |         |  ok   |  ok   |        |   ok   |        |        |        |        |        |        |        |
| tony      |    _    |  ok   |  ok   |   ok   |   ok   | **OK** | **OK** | **OK** | **OK** |        |        |        |
| zidane    |         |  ok   |  ok   |        |        |        |        |        |        |        |        |        |

Cette étape conclut la thématique "Transformer". Le checkpoint #4 portera sur les même contenus théorique, mais devrait être plus facile pour vous avec l'expérience acquise ce matin.

La semaine prochaine, on abordera la thématique "Réduire".

## Etape 5

### Lundi 21 septembre

C'est un jour de repos (Jeûne Fédéral)

### Mercredi 23 septembre

On fait le checkpoint #4, qui n'aborde aucun thème nouveau. On reste sur le filtrage et la projection.

Le thème de la semaine, par contre, est un gros morceau: réduire.

Jusqu'ici, avec `Select`, les transformations de données étaient un pour un, c'est-à-dire que chaque élément de notre collection était transformé en un élément de type différent:

- Objet en nombre (KDA)
- Objet en objet (OrderSummary)
- Nombre en objet (Génération à partir de `Range`).

Dans tous ces cas, la collection obtenue après `Select` a la même longueur que la collection originale.

Nous allons voir maintenant des opérations qui synthétisent plusieurs éléments pour produire un résultat plus compact:

| Catégorie            | Entrée     | Sortie                | Exemples                                |
| -------------------- | ---------- | --------------------- | --------------------------------------- |
| Test                 | collection | bool                  | `Any`, `All`, `Contains`                |
| Agrégation numérique | collection | valeur                | `Sum`, `Min`, `Max`, `Average`, `Count` |
| Réduction générale   | collection | valeur/objet          | `Aggregate`                             |
| Regroupement         | collection | collection de groupes | `GroupBy`                               |
| Indexation           | collection | autre structure       | `ToDictionary`, `ToHashSet`             |

Pour commencer, on regarde la [théorie de l'agrégation](./supports/source/04-Reduce.md).

Et ensuite, on met tout cela en pratique avec [l'exercice 5](./exos/fil-rouge/esport/05-classement-fold/) du fil rouge.

Situation en fin de matinée:
|           | Présent |  5.1  |  5.2  |  5.3  |
| --------- | :-----: | :---: | :---: | :---: |
| albert    |         |       |       |       |
| damienc   |         |       |       |       |
| damienr   |         |       |       |       |
| erdem     |         |       |       |       |
| gianmarco |         |       |       |       |
| gillian   |         |       |       |       |
| kiril     |         |       |       |       |
| sacha     |         |       |       |       |
| snehan    |         |       |       |       |
| theophile |         |       |       |       |
| tony      |         |       |       |       |
| zidane    |         |       |       |       |