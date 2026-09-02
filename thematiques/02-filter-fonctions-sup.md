# Filter et fonctions d'ordre supérieur

La programmation fonctionnelle repose sur un concept clé : les fonctions sont des valeurs comme les autres — on peut les passer en paramètre, les stocker dans une variable, les combiner. Cette thématique introduit `Func<T>`, les lambdas et le filtrage de collections : les briques sur lesquelles tout le reste s'appuie.

**À l'issue de cette thématique, vous serez capable de :**
- déclarer et utiliser un `Func<T, bool>` comme paramètre d'une méthode
- écrire une expression lambda et expliquer comment elle capture les variables de son contexte (closure)
- filtrer une collection avec `Where` de façon déclarative
- expliquer ce qu'est l'évaluation paresseuse et pourquoi elle est importante en LINQ

> **Ordre de lecture :** deux exercices sont associés à cette thématique (02 et 03). Lire `02a-fonctions-sup.md` avant l'exercice 02, puis `02b-filter.md` avant l'exercice 03.

## Théorie

- [Slides](../slides/02-filter-hof/)
- [Synthèse PPT](../supports/source/02-FilterLambdaFctSup.pptx)
- [Théorie complète Filter](../supports/source/02b-filter.md)
- [Théorie : fonctions d'ordre supérieur](../supports/source/02a-fonctions-sup.md)
- [Kahoot récapitulatif](https://create.kahoot.it/share/02-ordresupetfilter/10785ea9-cf34-4d6a-bd4e-37a30a256d52)

**Particularités utiles pour les activités**

- [Closures (captures de variables)](../supports/source/02a-fonctions-sup.md#closures-captures-de-variables)
- [Évaluation paresseuse (Deferred Execution)](../supports/source/02b-filter.md#evaluation-paresseuse-deferred-execution)

## Activités

### Fil rouge

Répondre aux besoins du fil rouge en enrichissant la bibliothèque `DataSeries<T>` :

- [02-recrues-generation — Charger les CSV (`FromCsv`, première HOF) et générer les données manquantes](../exos/fil-rouge/esport/02-recrues-generation/)
- [03-tri-filter — Valider les stats](../exos/fil-rouge/esport/03-tri-filter/)

### Exercices complémentaires

#### Activités prioritaires

- [Retour sur la place du marché](../exos/marché/) — points clés : import de données (CSV, copier-coller, librairie Excel), relation POO/FP, syntaxe LINQ. [Cheatsheet LINQ](../supports/linq-cheatsheet.pdf) disponible pour référence
- [Words](../exos/words/)

#### Activités libres

- [Cinéma](../exos/cinema/)
- [Hardware](../exos/hardware/)
