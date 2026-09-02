# Récursivité

Certains problèmes — arbres, fractales, brackets de tournoi — ne se résolvent pas proprement avec une boucle. La récursion décompose le problème en un cas de base et un appel réduit sur lui-même. En FP, cette décomposition remplace les variables muables d'une boucle par un accumulateur passé en paramètre, ce qui maintient la pureté des fonctions.

**À l'issue de cette thématique, vous serez capable de :**
- identifier un problème récursif et définir son cas de base
- écrire une fonction récursive avec accumulateur (sans variable muable)
- expliquer le lien entre récursion terminale et Fold
- parcourir une structure arborescente par récursion

## Théorie

- [Slides](../slides/08-recursion/)
- [Recursivite](../supports/source/08-Recursivite.md)
- [Kahoot récap sur tout le module](https://create.kahoot.it/share/c-fonctionnel-linq-extension-recap/24dae2fb-b7e1-4fcd-9cec-818ebae195ae)
- [Vidéo sur le flocon de Koch](https://eduvaud-my.sharepoint.com/:v:/g/personal/jonathan_melly_eduvaud_ch/EV2ZwK0TqlVFhB45A29vWAEBGe_lqvxtq1_S5IA6MsX75g?e=aDZRSO)

**Particularités utiles pour les activités**

- [Récursion et programmation fonctionnelle](../supports/source/08-Recursivite.md#recursion-et-programmation-fonctionnelle)

La récursion impérative (fibonacci en procédural) repose souvent sur des variables muables et un état partagé. La version FP remplace cet état par un **accumulateur en paramètre** : chaque appel reçoit une valeur intermédiaire et la passe à l'appel suivant, sans jamais écrire dans une variable externe. Ce pattern est équivalent à un `Aggregate` (Fold) déroulé à la main — ce qui explique pourquoi Fold peut exprimer n'importe quelle récursion terminale.

## Activités

Répondre aux besoins du fil rouge en enrichissant la bibliothèque `DataSeries<T>`.

### Fil rouge

- [08-tournoi-recursion — Bracket de tournoi](../exos/fil-rouge/esport/08-tournoi-recursion/) — analyser un bracket de tournoi par décomposition récursive — en enrichissant la bibliothèque

### Exercices complémentaires

#### Activités prioritaires

- [Puissances](../exos/puissance/README.md)
- [Fibonacci](../exos/fibo/README.md)
- [Listing de fichiers](../exos/files/README.md)
- [Fractales](../exos/fractale/README.md)

#### Activités libres

- [MyLib](../exos/mylib/README.md)
- [Linqorne](../exos/linqorne/README.md)
