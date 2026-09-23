# Exercice 06 — Dashboard ESL

> Partie 5 — Extensions fluentes + `ToCsv()` + `WithFallback()` + `PairWith()` + DSL

## Concepts théoriques

- [Thématique 05 — Extensions et DSL fluent](../../../../thematiques/05-extensions-dsl.md)
- [Méthodes d'extension et chaînage](../../../../supports/source/05-Extension.md)
- [Composition de fonctions f ∘ g](../../../../supports/source/05-Extension.md#composition-de-fonctions-f-∘-g)
- [DSL — Domain Specific Language](../../../../supports/source/05-Extension.md#dsl-domain-specific-language)
- [Zip — combiner deux séquences en parallèle](../../../../supports/source/05-Extension.md#zip-—-combiner-deux-sequences-en-parallele)

## Contexte

L'équipe présente ses stats à un tournoi ESL. Le data analyst prépare un rapport
hebdomadaire : export CSV pour l'organisateur, gestion des données manquantes,
comparaison kills vs assists côte à côte.

Un pipeline fluent lisible comme du langage naturel est l'objectif :

```csharp
kdaLea
    .Smooth(windowSize: 3)
    .WithFallback(0.0, v => double.IsNaN(v))
    .ToCsv("report.csv");
```

**Avant de coder :**

- Pourquoi des méthodes d'*extension* plutôt que des méthodes dans la classe `StatSeries` ?
- Quel contrat chaque méthode doit-elle respecter pour que le chaînage reste possible ?

<details>
<summary>Indice</summary>

Le contrat de la composition : chaque méthode retourne le même type qu'elle reçoit
(ou un type compatible). C'est ce qui permet `x.g().f()` — l'équivalent C# de `(f ∘ g)(x)`.
→ [Le contrat de la composition](../../../../supports/source/05-Extension.md#le-contrat-de-la-composition)

</details>

---

## Étape 1 — `ToCsv()` dans `StatSeriesExtensions.cs`

Créer `DataSeries/StatSeriesExtensions.cs` :

```csharp
public static class StatSeriesExtensions
{
    public static void ToCsv(this StatSeries series, string path)
    {
        // générer les lignes "date,valeur" depuis DataPoints et écrire dans le fichier
        // ...
    }
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public static void ToCsv(this StatSeries series, string path)
{
    var lines = series.DataPoints.Select(d => $"{d.Date:yyyy-MM-dd},{d.Value:F4}");
    File.WriteAllLines(path, lines.Prepend("date,value"));
}
```

Chaque ligne porte la vraie date du match — le fichier est importable et triable dans Excel.
Les dates viennent directement des tuples `(DateTime Date, double Value)` stockés dans `StatSeries`.

</details>

---

## Étape 2 — `WithFallback(fallback, isMissing)`

`WithFallback` n'est qu'un filtre conditionnel : remplacer les valeurs manquantes (NaN, 0, etc.)
par une valeur de remplacement.

```csharp
public static StatSeries WithFallback(
    this StatSeries series, double fallback, Func<double, bool> isMissing)
{
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public static StatSeries WithFallback(
    this StatSeries series, double fallback, Func<double, bool> isMissing)
    => StatSeries.From(
        series.DataPoints.Select(d => (d.Date, isMissing(d.Value) ? fallback : d.Value))
    );
```

</details>

---

## Étape 3 — `PairWith()` — kills vs assists côte à côte

`Zip` (ici `PairWith`) combine deux listes élément par élément — pattern fondamental pour
traiter des séries corrélées (kills + assists, température + humidité...).
→ [Zip — combiner deux séquences en parallèle](../../../../supports/source/05-Extension.md#zip-—-combiner-deux-sequences-en-parallele)

```csharp
public static IEnumerable<(double Left, double Right)> PairWith(
    this StatSeries left, StatSeries right)
{
    // Hint : Zip combine deux séquences élément par élément
    // ...
}
```

<details>
<summary>Voir la solution</summary>

```csharp
public static IEnumerable<(double Left, double Right)> PairWith(
    this StatSeries left, StatSeries right)
    => left.Values.Zip(right.Values, (l, r) => (l, r));
```

`Zip` s'arrête à la séquence la plus courte — les séries de longueurs différentes sont acceptées.

> **Différence avec esport :** `PairWith` dans le fil rouge esport retourne un
> `DataSeries<(double Left, double Right)>` et préserve les timestamps.
> Ici, retourner `IEnumerable<(double, double)>` suffit pour la comparaison côte à côte.
> Le compromis : on perd les dates dans le résultat combiné.

</details>

Comparer kills normalisés et assists normalisés de Léa :

```csharp
var kills   = valorant.Filter(m => m.Player == "Léa").Extract(m => (double)m.Kills);
var assists = valorant.Filter(m => m.Player == "Léa").Extract(m => (double)m.Assists);

var report = kills.Normalize().PairWith(assists.Normalize());
foreach (var (k, a) in report)
    Console.WriteLine($"kills={k:F2}  assists={a:F2}");
```

---

## DSL final — rapport hebdomadaire en une expression

Les extensions définissent le vocabulaire du domaine. Un bon DSL se lit sans avoir besoin
de connaître l'implémentation :
→ [DSL](../../../../supports/source/05-Extension.md#dsl-domain-specific-language)

```csharp
valorant
    .Filter(m => m.Player == "Léa")
    .Extract(m => (double)m.Kills)
    .Normalize()
    .Smooth(3)
    .WithFallback(0.0, v => double.IsNaN(v))
    .ToCsv("lea_kills_smoothed.csv");
```

**Un pipeline est aussi une valeur.** La préparation commune (lisser puis combler les trous)
se répète pour chaque série du rapport — la stocker dans une variable :

```csharp
// Préparation commune : lisser puis combler les trous
Func<StatSeries, StatSeries> prepare =
    s => s.Smooth(3).WithFallback(0.0, v => double.IsNaN(v));

// Réutilisé sur chaque série du rapport hebdomadaire
prepare(killsLea).ToCsv("lea_kills.csv");
prepare(assistsLea).ToCsv("lea_assists.csv");
prepare(kdaRaphael).ToCsv("raphael_kda.csv");
```

Composer deux pipelines stockés donne un nouveau pipeline — c'est exactement `f ∘ g` :
→ [Composition explicite avec Func](../../../../supports/source/05-Extension.md#composition-explicite-avec-func) ·
[Fonctions comme valeurs](../../../../supports/source/02a-fonctions-sup.md)

---

## Étape 4 — Interface CLI

Ajouter `--export <fichier>` pour déclencher l'export CSV depuis la ligne de commande.

**Avant de coder :** À quel endroit du pipeline appeler `ToCsv` — avant ou après `Smooth` ?
Comment récupérer le nom du fichier cible depuis `args` ?

```
dotnet run -- --game valorant --player Léa --stat kda --export lea_kda.csv
```

<details>
<summary>Voir la solution</summary>

```csharp
if (args.Contains("--export"))
{
    var file = args[Array.IndexOf(args, "--export") + 1];
    values.ToCsv(file);
    Console.WriteLine($"Exporté : {file}");
}
```

</details>

---

## Vérification

- `ToCsv` produit un fichier importable dans Excel
- `WithFallback` sur des données propres ne modifie aucun élément
- `PairWith` sur deux séries de longueur différente → Zip s'arrête à la plus courte
- Le pipeline DSL compile et produit un résultat identique aux étapes intermédiaires
