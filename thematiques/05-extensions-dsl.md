# Extensions et DSL fluent

Les méthodes d'extension permettent d'enrichir n'importe quel type sans en modifier le code source. Appliquées à `DataSeries<T>`, elles donnent naissance à un DSL fluent — une syntaxe expressive propre au domaine — et ouvrent la voie à la composition de fonctions (f ∘ g) et au traitement parallèle de séquences avec `Zip`.

**À l'issue de cette thématique, vous serez capable de :**
- écrire une méthode d'extension C# et l'enchaîner dans un pipeline fluent
- composer deux fonctions `Func<T, T>` en une seule transformation réutilisable
- utiliser `Zip` pour traiter deux séquences en parallèle élément par élément
- expliquer ce qu'est un DSL et pourquoi cette approche améliore la lisibilité

## Théorie

- [Slides](../slides/05-extensions/)
- [Théorie sur les extensions en C#](../supports/source/05-Extension.md)

**Particularités utiles pour les activités**

- [Composition de fonctions (f ∘ g)](../supports/source/05-Extension.md#composition-de-fonctions-f-∘-g)
- [DSL : Domain Specific Language](../supports/source/05-Extension.md#dsl-domain-specific-language)
- [Zip — combiner deux séquences en parallèle](../supports/source/05-Extension.md#zip-—-combiner-deux-sequences-en-parallele)

## Activités

Répondre aux besoins du fil rouge en enrichissant la bibliothèque `DataSeries<T>`.

### Fil rouge

- [06-rapport-dsl — Dashboard ESL](../exos/fil-rouge/esport/06-rapport-dsl/) — comparer kills et assists côte à côte dans un dashboard fluent — en enrichissant la bibliothèque

### Exercices complémentaires

#### Activités prioritaires

- [Terminer RandoReduce](../exos/randoReduce/README.md)
- [Gérer des événements avec Zip](../exos/events/README.md)

#### Activités libres

- SWAPI — exercice de consolidation Filter/Map/Reduce avec extensions. [La Cheatsheet](../supports/linq-cheatsheet.pdf) peut être utile. ![Yoda](yoda.png) [SWAPI](../exos/swapi/)
- Avancer sur le projet PTL
