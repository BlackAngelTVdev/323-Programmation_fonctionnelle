# Pureté et immutabilité

Depuis l'exercice 01, la bibliothèque `DataSeries<T>` utilise des propriétés get-only. Cette thématique formalise le *pourquoi* : une fonction pure ne produit pas d'effets de bord et renvoie toujours le même résultat pour les mêmes arguments — propriété qui simplifie les tests, le débogage et la parallélisation. Les records C# matérialisent cette idée directement dans la syntaxe du langage.

**À l'issue de cette thématique, vous serez capable de :**
- définir la pureté d'une fonction et identifier les effets de bord dans du code existant
- expliquer la transparence référentielle et son impact sur la testabilité
- convertir une classe mutable en record C# immuable avec l'expression `with`
- auditer un pipeline de données pour y corriger les violations de pureté

## Théorie

- [Slides](../slides/06-purete/)
- [Pureté](../supports/source/06-PureteImmutabilite.md#purete)
- [Immutabilité](../supports/source/06-PureteImmutabilite.md#immutabilite)
- [06-PureteImmutabilite PPT](../supports/source/06-PureteImmutabilite.pptx)
- [Parallélisation (cas pratique du PPT)](../supports/Program-PI.cs)

**Particularités utiles pour les activités**

- [Transparence référentielle et pureté](../supports/source/06-PureteImmutabilite.md#transparence-referentielle)
- [Records C# — l'immutabilité par défaut](../supports/source/06-PureteImmutabilite.md#records-c-—-l-immutabilite-par-defaut)

## Activités

Répondre aux besoins du fil rouge en auditant la bibliothèque `DataSeries<T>`.

### Fil rouge

- [07-audit-purete — Audit sécurité](../exos/fil-rouge/esport/07-audit-purete/) — convertir les classes en records, auditer et sécuriser le pipeline de données — en enrichissant la bibliothèque

### Exercices complémentaires

#### Activités prioritaires

- [Diffit](../exos/diffit/README.md) — entraînement au test
