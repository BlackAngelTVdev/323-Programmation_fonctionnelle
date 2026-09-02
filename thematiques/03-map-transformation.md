# Map et transformation

Après avoir filtré des collections, l'étape naturelle est de les transformer. Map (alias `Select` en LINQ) applique une fonction à chaque élément et produit une nouvelle collection — sans toucher à l'originale. C'est le fondement de la transformation déclarative et de la composition de pipelines.

**À l'issue de cette thématique, vous serez capable de :**
- transformer chaque élément d'une collection avec `Select` sans muter la source
- enchaîner plusieurs opérations LINQ en pipeline lisible
- expliquer la différence entre `Select` et `SelectMany` (flatMap)
- décrire pourquoi l'absence de mutation simplifie le raisonnement sur le code

## Théorie

- [Slides](../slides/03-map/)
- [Exemple introductif](../supports/source/03-MapIntro.md)
- [Condensé PPT](../supports/source/03-Map.pptx)
- [Théorie complète](../supports/source/03-Map.md)
- [Kahoot](https:/create.kahoot.it/share/03-map/1c5c732d-0506-412e-a923-fa261effdfcc)

**Particularités utiles pour les activités**

- [Composition de pipelines](../supports/source/03-Map.md#composition-de-pipelines)
- [SelectMany — le flatMap](../supports/source/03-Map.md#selectmany-—-le-flatmap) (pour l'étape bonus)

## Activités

Répondre aux besoins du fil rouge en enrichissant la bibliothèque `DataSeries<T>`.

### Fil rouge

- [04-performance-map — Calculer le KDA par joueur](../exos/fil-rouge/esport/04-performance-map/) — calculer et comparer les performances des joueurs — en enrichissant la bibliothèque (étape bonus : `SelectMany`)

### Exercices complémentaires

#### Activités prioritaires

- [Mib-map](../exos/mib-map/) — échauffement Map
- [Rando](../exos/rando/)

#### Activités libres

- Consolidation de Filter : [Words](../exos/words/), [Cinéma](../exos/cinema/), [Hardware](../exos/hardware/)
