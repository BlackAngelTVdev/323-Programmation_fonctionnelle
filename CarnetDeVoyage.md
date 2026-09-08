# Carnet de voyage

Du 24 août au 30 octobre 2026, la classe CMID3b et moi avons parcouru un chemin d'apprentissage de la programmation fonctionnelle.
Ce document relate les péripéties de ce voyage.

<hr>

## Semaine 35 (24 août)

### Lundi

- On a découvert le thème du projet Plot Those Lines, chacun a choisi un domaine
- Tout le monde a référencé son repo dans MarketPlace. Certains doivent encore ajouter un Readme.
- On a passé en revue le [Project Handbook](https://github.com/XCarrel/Project-Handbook/tree/T1_2026-2027_P_FUN_P_OO)

### Mardi

- J'ai publié et annoncé la version 1.3 de gistodoc, pour importer les issues Github dans un document Word

### Mercredi

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

## Semaine 36 (31 août)

### Lundi

La mission du jour était: finaliser l'analyse fonctionnelle et la planification initiale. Idéalement, il aurait dû être possible de faire la livraison de vendredi dès aujourd'hui en fin de matinée.

J'ai passé vers à peu près tout le monde. Le constat est que la rédaction de User Stories n'est pas encore maîtrisée, personne n'ayant pu me montrer une user story bien formulée du premier coup.Je tiens à ce que le codage ne commence qu'à partir du moment où au moins une US est bien formulée. Je préfère avoir un petit nombre de US de bonne qualité que beaucoup de US de mauvaise qualité. Conséquence:

> Pour la livraison du 4 septembre, je ne damande pas une analyse fonctionnelle complète. Je n'attends que une ou deux US totalisant au minimum 5 bons tests d'acceptance

N'ayant reçu aucune livraison pour l'instant (mardi), j'en déduis que tout le monde a encore du travail à fournir pour arriver à ce stade.

### Mercredi

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

## Semaine 37 (7 septembre)

### Lundi

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

### Mercredi

On va faire le checkpoint #2 pour voir ce que vous avez retenu au sujet des fonctions d'ordre supérieur et des première méthodes d'extension LinQ.  
Si vous avez terminé avant la fin du temps à disposition, utilisez ce temps pour compléter et/ou committer votre JdT. 

On a grimpé une pente bien raide la semaine passée. Cette semaine, on va pouvoir récupérer un peu sur un chemin bien plus plat.
Mais il va quand même falloir avancer, avec un objectif général de la semaine: savoir **transformer** les objets d'une liste.

Mais d'abord, synchronisation ...

J'observe des usages du repo du cours très variables: de "aucune activité" pour certains à "plusieurs coups d'avance" pour d'autres. J'ai de la peine à savoir où vous en êtes. Du coup, je vous demande à chacun de créer un fichier `PointDeSituation.md` dans votre dossier perso et de le commit/push.

Quand c'est fait, vous avancez votre ESportApp jusqu'à qu'elle soit capable de lire les fichiers CSV. Pendant ce temps, je consulte vos retours.  
Si vous avez atteint ce point, attendez les autres en vous attaquant à l'un des [autres exercices](./exos/README.md).

On se synchronise en revoyant ensemble une solution qui importe nos stats à partir de fichiers CSV.

On discute des méthodes de génération (`Range`) et de transformation (`Select`) sur la base la cheatsheet.

- Vous utilisez `Range` et `Random` (bien connu) pour réaliser l'[étape 2](https://github.com/XCarrel/323-Programmation_fonctionnelle/tree/main/exos/fil-rouge/esport/02-recrues-generation).

- Vous utilisez `Select` pour refaire des lignes de CSV à partir de chaque objet et enregistrer les données générées dans vos fichiers CSV ([étape 3](https://github.com/XCarrel/323-Programmation_fonctionnelle/tree/main/exos/fil-rouge/esport/02-recrues-generation)).

- Exercez les fonctions de tri avec l'[exercice 3](./exos/fil-rouge/esport/03-tri-filter/README.md)

N'oubliez pas de semer des petits cailloux blancs le long de votre chemin ...

<hr>

## Semaine 38 (14 septembre)

### Lundi

Il faut qu'on discute, parce que je fais un constat qui me dérange et que je ne trouve pas les bonnes raisons qui pourraient l'expliquer.  
Peut-être que quelque chose m'échappe.

On travaille 3 heures ensemble le lundi matin, sur un projet dans lequel vous devez tenir un journal de travail et appliquer de bonnes pratiques en matière de contrôle de version.

Je m'attends donc à ce que dans chacun de vos repos je trouve un dernier commit fait aux alentours de 11h25, qui serait nommé "chore(jdt): Mettre à jour avec les activités de ce matin".

Or voici ce que je vois:
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
