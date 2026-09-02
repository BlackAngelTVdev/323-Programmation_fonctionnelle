# Fold et agrégation

Filter et Map traitent chaque élément indépendamment. Fold (alias `Aggregate` en LINQ) parcourt une séquence en accumulant un résultat — somme, minimum, chaîne construite, structure complexe. C'est l'opération la plus puissante des trois, et celle qui permet d'exprimer presque tout le reste.

**À l'issue de cette thématique, vous serez capable de :**
- utiliser `Aggregate` pour réduire une séquence à une valeur unique
- choisir entre `Sum`, `Min`, `Max`, `Count` et `Aggregate` selon le besoin
- regrouper des données par clé avec `GroupBy` et les agréger
- expliquer pourquoi Fold est considéré comme l'opération fondamentale de la FP

## Théorie

- [Slides](../slides/04-fold/)
- [04-Reduce](../supports/source/04-Reduce.md)
- [PPT Reduce](../supports/source/03-Reduce.pptx)
- [Références techniques LINQ](../supports/source/references.md) — cheatsheet et documentation officielle
- [Kahoot reduce](https://create.kahoot.it/details/ae81d53f-ebc0-40f7-afea-fdcc001e7ba8)

**Particularités utiles pour les activités**

- [Fold — l'Agrégation Universelle](../supports/source/04-Reduce.md#fold-—-l-agregation-universelle)
- [GroupBy — agréger par clé](../supports/source/04-Reduce.md#groupby) (pour l'étape bonus)

## Activités

Répondre aux besoins du fil rouge en enrichissant la bibliothèque `DataSeries<T>`.

### Fil rouge

- [05-classement-fold — Classement de saison](../exos/fil-rouge/esport/05-classement-fold/) — établir le classement des joueurs et analyser leur progression — en enrichissant la bibliothèque (étape bonus : `GroupBy`)

### Exercices complémentaires

#### Activités prioritaires

- [Mib-map → Livrable 2](../exos/mib-map/README.md#livrable-2)
- [Rando](../exos/rando/)
- [Icequeen](../exos/icequeen/README.md)

#### Activités libres

- [Mib-map : Mesures de performances](../exos/mib-map/README.md#mesures-de-performances)
- Intégrer Map au projet PTL
- [La revanche du marché (Reduce)](../exos/mib-reduce/README.md)
- [Le retour de Rando (Reduce)](../exos/randoReduce/README.md)
