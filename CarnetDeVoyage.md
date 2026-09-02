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
- On est d'accord sur les [modalités d'évaluation](evaluation/DEP.md) du module

- On a vérifié nos paquetage de départ, en [révisant les concepts OO](exos/consolidation-OO/).
  - La terminologie : c'était une bonne chose qu'on le fasse, plusieurs termes n'étaient pas vraiment maîtrisés
  - Un Dojo pratique : on n'a pas été au bout du programme, mais plein de questions importantes ont été remontées et discutées. On part avec une bonne base.

- On s'est attaqué à une thématiques: les [paradigmes de programmation](https://github.com/XCarrel/323-Programmation_fonctionnelle/blob/main/supports/source/01-paradigmes.md)

- On a revu la [manière de suivre un cours](USEME.md) avec Github et chacun mis en place son fork de [mon repo](https://github.com/XCarrel/323-Programmation_fonctionnelle), dans lequel il a créé son espace personnel. J'ai les références de tous les forks. @Albert, @Snehan : je ne vois pas votre espace personnel dans le repo

- On a étudié une deuxième thématique: [généricité](https://etml-inf.github.io/323-Programmation_fonctionnelle/supports/source/01b-genericite.html) (en matière de programmation). C'est complexe, la pente d'apprentissage est raide.

- On a commencé la mise en oeuvre la généricité avec [l'exercice 01](https://etml-inf.github.io/323-Programmation_fonctionnelle/exos/fil-rouge/esport/01-equipe-genericite/) du fil rouge. On a vu comment
  - Structurer une application console avec deux projet (un programme et une librairie)
  - Déclarer notre première classe générique

On n'est pas arrivé au bout de l'exercice, on reprendra ça la semaine prochaine

<hr>

## Semaine 36 (31 août)
### Lundi

La mission du jour était: finaliser l'analyse fonctionnelle et la planification initiale. Idéalement, il aurait dû être possible de faire la livraison de vendredi dès aujourd'hui en fin de matinée.

J'ai passé vers à peu près tout le monde. Le constat est que la rédaction de User Stories n'est pas encore maîtrisée, personne n'ayant pu me montrer une user story bien formulée du premier coup.  
Je tiens à ce que le codage ne commence qu'à partir du moment où au moins une US est bien formulée. Je préfère avoir un petit nombre de US de bonne qualité que beaucoup de US de mauvaise qualité. Conséquence:

> Pour la livraison du 4 septembre, je ne damande pas une analyse fonctionnelle complète. Je n'attends que une ou deux US totalisant au minimum 5 bons tests d'acceptance 

N'ayant reçu aucune livraison pour l'instant (mardi), j'en déduis que tout le monde a encore du travail à fournir pour arriver à ce stade.

### Mercredi (plan)

On commencera avec le checkpoint #1.

On va faire un Sync Fork, parce que j'ai mis à jour des supports et le carnet de voyage.  

On va voir ensemble la solution du début de l'étape 1 de l'application ESportApp, dans laquelle on sépare clairement les aspects métiers et les "logistique":
- Le projet `ESportApp` contient des classes propres au domaine (métier): `CS2Match`, `LolMatch`, `ValorantMatch`)
- Le projet `Dataseries` contient les moyens de gérer des séries de `<un_certain_type>`

On va commencer par voir qu'avec C#, on peut aussi **stocker une fonction dans une variable** .

Et si on peut mettre une fonction dans une variable, cela veut dire qu'on peut également **donner une fonction comme argument à une autre fonction**. Du coup, cette dernière est ce qu'on appelle une [fonction d'ordre supérieur](./supports/source/02a-fonctions-sup.md)

On se prendra une pause à ce moment

On va mettre en pratique avec l'étape 1 de l'[exercice 2](./exos/fil-rouge/esport/02-recrues-generation/README.md) du fil rouge, dans laquelle on importe les données à partir de fichiers CSV en s'appuyant sur des fonctions spécialisées pour parser les données.

Une fois que notre application sera capable de lire les fichiers, nous allons commencer à jouer avec nos statistiques grâce aux premières méthodes LinQ de la [cheatsheet](./supports/linq-cheatsheet.pdf): combien de victoires ? A quand remonte la dernière défaite de Léa ? Quelles sont les stats du troisième match de Dylan ? ...

S'il nous reste du temps et de l'énergie, nous allons ajouter un traitement des arguments à notre application: choisir entre des données hardcodées ou en provenance de fichiers.

<hr>

## Semaine 37 (7 septembre)
### Lundi

Objectif: pouvoir commencer la réalisation

Prérequis:
- Avoir au moins une US valide dans laquelle la séquence de démarrage de l'application est décrite
- Avoir des données à disposition (5 séries cohérentes de 500 valeurs chacune)