# Paradigmes fonctionnels

Ce module introduit la programmation fonctionnelle en C# à travers LINQ. L'idée centrale : écrire du code qui décrit *ce que l'on veut* plutôt que *comment le faire* étape par étape — une approche que C# rend accessible dès la première session.

**À l'issue de cette thématique, vous serez capable de :**
- distinguer code impératif et code déclaratif sur un exemple concret
- expliquer pourquoi la généricité (`List<T>`, classes génériques) est une première forme d'abstraction FP
- utiliser la syntaxe de requête LINQ pour filtrer et projeter une collection simple
- décrire ce qu'est `DataSeries<T>` et le rôle qu'elle joue dans le fil rouge

> **Note :** l'immutabilité est introduite pratiquement dès l'exercice 01 ; sa théorie de fond (pureté, transparence référentielle) est développée en [thématique 06](./06-purete-immutabilite.md).

## Mise en route

- Se dire bonjour / Faire connaissance (si nécessaire)
- Voir les [objectifs du module](https://www.modulbaukasten.ch/module/323/1/fr-FR?title=Programmer-de-mani%C3%A8re-fonctionnelle)
- Voir le [chemin](https://roadmap.sh/r/embed?id=66b88565b64402e0526d8ebc) vers ces objectifs
- Voir [les modalités d'évaluation](../evaluation/DEP.md)
- [Git workflow](../USEME.md)

### Récap OO (optionnel)
- [Consolidation OO](../exos/consolidation-OO/) — révision des concepts orientés objet (terminologie, Dojo Randori Parachutes) utile si les bases OO semblent fragiles avant d'aborder le fil rouge

## Théorie

- [Slides](../slides/01-paradigmes/)
- [Slides paradigmes (Kahoot)](https://create.kahoot.it/share/ict-323-paradigme/ed5b81f2-c5be-4aa4-9e50-acdbbe368c86)
- [Théorie](../supports/source/01-paradigmes.md)
- [Généricité — abstraire les types](../supports/source/01b-genericite.md)
- Découverte de la syntaxe requête LINQ au moyen de la [cheatsheet](../supports/source/references.md), entre les étapes 1 et 2 de l'exercice pratique
- [Kahoot 01 paradigmes](https://create.kahoot.it/share/01-paradigmes-de-programmation/0f35b3fc-ffdc-471b-a983-5e121ec76a9d)

## Activités

Répondre aux besoins du fil rouge en construisant et adaptant la bibliothèque `DataSeries<T>`.

### Fil rouge

Chaque thématique s'appuie sur **[le fil rouge](../exos/fil-rouge/esport/README.md)** — un scénario commun qui évolue et s'enrichit au fil des exercices.

- [01-equipe-genericite — L'équipe](../exos/fil-rouge/esport/01-equipe-genericite/README.md) — modéliser l'équipe avec des classes immuables et explorer des données de matchs en dur (Valorant, CS2, LoL) — en construisant la bibliothèque générique

### Exercices complémentaires

- [Place du marché](../exos/marché/) — remise en main du C# et des outils, introduction à LINQ dans un contexte concret

- NCDU — créer un programme similaire à [ncdu](https://dev.yorhel.nl/ncdu) affichant les statistiques d'utilisation d'un répertoire Windows :
  - **Version 1** — sans LINQ : boucles et conditions classiques
  - **Version 2** — avec LINQ : s'inspirer de la [doc Microsoft](https://learn.microsoft.com/en-us/dotnet/csharp/linq/how-to-query-files-and-directories) pour rendre le programme plus concis
