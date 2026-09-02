# Consolidation des concepts OO

## Terminologie

Donner la définition des termes suivants :

<details>
<summary>Interface</summary>

Un contrat définissant un ensemble de méthodes (signatures) qu'une classe doit implémenter, sans en fournir l'implémentation. Elle permet d'assurer qu'une classe respecte un comportement attendu.

```csharp
public interface IAnimal
{
    void Manger();
}

public class Chat : IAnimal
{
    public void Manger()
    {
        Console.WriteLine("Le chat mange des croquettes.");
    }
}
```

</details>

<details>
<summary>Signature (de méthode)</summary>

L'ensemble des éléments qui identifient une méthode de façon unique : son nom, le nombre et le type de ses paramètres (et parfois son type de retour selon le langage).

```csharp
// Signature : Additionner(int, int)
public int Additionner(int a, int b)
{
    return a + b;
}
```

</details>

<details>
<summary>Objet</summary>

Une entité concrète créée à partir d'une classe, possédant son propre état (valeurs des attributs) et pouvant exécuter les comportements (méthodes) définis par sa classe.  
Quand on dit qu'un objet est de type `T`, cela veut dire qu'il a été créé à partir de la classe `T`.

```csharp
Personne alice = new Personne();
alice.Nom = "Alice";
alice.Age = 30;
alice.SePresenter(); // Je m'appelle Alice.
```

</details>

<details>
<summary>Encapsulation</summary>

Le principe qui consiste à regrouper les attributs et les méthodes au sein d'une classe, et à restreindre l'accès direct aux détails internes (attributs) en le contrôlant via des méthodes publiques (souvent des accesseurs/mutateurs). Cela protège la cohérence de l'état de l'objet.

```csharp
public class CompteBancaire
{
    private decimal solde; // détail interne caché

    public decimal Solde => solde; // lecture contrôlée

    public void Deposer(decimal montant)
    {
        if (montant <= 0) throw new ArgumentException("Montant invalide.");
        solde += montant;
    }
}
```

</details>

<details>
<summary>Propriété</summary>

Un membre d'une classe qui expose un attribut de manière contrôlée, généralement via des accesseurs (getter/setter), permettant de valider ou de restreindre l'accès en lecture/écriture.

```csharp
public class Personne
{
    private int age;

    public int Age // propriété
    {
        get { return age; }
        set
        {
            if (value < 0) throw new ArgumentException("L'âge ne peut pas être négatif.");
            age = value;
        }
    }
}
```

</details>

<details>
<summary>Namespace</summary>

Un espace de noms qui permet de regrouper et d'organiser des classes (ou d'autres éléments) sous un nom commun, évitant ainsi les conflits de noms entre différentes parties d'un projet.

```csharp
namespace MonProjet.Modeles
{
    public class Personne
    {
        // ...
    }
}

// Utilisation depuis un autre fichier :
using MonProjet.Modeles;
```

</details>

<details>
<summary>Classe</summary>

Un plan (modèle) qui décrit la structure (attributs) et le comportement (méthodes) d'un ensemble d'objets. Elle sert de "moule" pour créer des objets.

```csharp
public class Personne
{
    public string Nom;
    public int Age;

    public void SePresenter()
    {
        Console.WriteLine($"Je m'appelle {Nom}.");
    }
}
```

</details>

<details>
<summary>Surcharge</summary>

Le fait de définir plusieurs méthodes portant le même nom dans une classe, mais avec des signatures différentes (nombre ou type de paramètres différents). Le compilateur choisit la bonne méthode à exécuter en fonction des arguments fournis lors de l'appel.

```csharp
public int Additionner(int a, int b)
{
    return a + b;
}

public double Additionner(double a, double b) // surcharge
{
    return a + b;
}

public int Additionner(int a, int b, int c) // surcharge
{
    return a + b + c;
}
```

</details>

<details>
<summary>Public</summary>

Modificateur d'accès qui rend un membre (attribut ou méthode) accessible depuis n'importe où, y compris depuis l'extérieur de la classe.

```csharp
public class Personne
{
    public string Nom; // accessible depuis n'importe où
}

Personne alice = new Personne();
alice.Nom = "Alice"; // accès depuis l'extérieur, autorisé
```

</details>

<details>
<summary>Méthode</summary>

Une fonction définie dans une classe qui décrit un comportement ou une action que les objets de cette classe peuvent réaliser.

```csharp
public class Personne
{
    public void SePresenter() // méthode
    {
        Console.WriteLine("Bonjour !");
    }
}
```

</details>

<details>
<summary>Attribut</summary>

Une variable définie dans une classe qui représente une caractéristique ou une donnée d'un objet (aussi appelé champ ou membre).

```csharp
public class Personne
{
    public string Nom; // attribut
    public int Age;    // attribut
}
```

</details>

<details>
<summary>`this`</summary>

Une référence utilisée à l'intérieur d'une méthode pour désigner l'objet depuis lequel la méthode est appelée. Elle permet notamment de distinguer un attribut d'un paramètre portant le même nom.

```csharp
public class Personne
{
    public string nom; // Ouch! convention de nommage !

    public Personne(string nom)
    {
        this.nom = nom; // this.nom = attribut, nom = paramètre
    }
}
```

</details>

<details>
<summary>Statique</summary>

Qualifie un membre (attribut ou méthode) qui appartient à la classe elle-même plutôt qu'à une instance particulière. Il est accessible sans devoir créer d'objet et est partagé par toutes les instances.

```csharp
public class CompteurPersonnes
{
    public static int NombreCrees = 0;

    public CompteurPersonnes()
    {
        NombreCrees++;
    }
}

// Utilisation, sans instancier :
Console.WriteLine(CompteurPersonnes.NombreCrees);
```

</details>

<details>
<summary>Privé</summary>

Modificateur d'accès qui restreint la visibilité d'un membre (attribut ou méthode) à la classe dans laquelle il est défini. Il n'est pas accessible depuis l'extérieur de la classe.

```csharp
public class Personne
{
    private string numeroSecuriteSociale; // accessible uniquement dans Personne
}
```

</details>

<details>
<summary>Etat</summary>

L'ensemble des valeurs des attributs d'un objet à un instant donné. L'état d'un objet peut évoluer au cours de son cycle de vie suite à l'exécution de ses méthodes.

```csharp
Personne alice = new Personne();
alice.Nom = "Alice";
alice.Age = 30; // état actuel : Nom = "Alice", Age = 30

alice.Age = 31; // l'état a changé suite à une action
```

</details>

<details>
<summary>Instance</summary>

Un exemplaire particulier d'une classe. Chaque objet créé à partir d'une classe est une instance de celle-ci.

```csharp
Personne alice = new Personne(); // alice est une instance de Personne
Personne bob = new Personne();   // bob est une autre instance de Personne
// alice et bob ont chacun leur propre état, même s'ils partagent la même classe
```

</details>

<details>
<summary>Constructeur</summary>

Une méthode spéciale d'une classe, portant le même nom qu'elle, qui est appelée automatiquement lors de l'instanciation d'un objet. Elle sert à initialiser l'état de l'objet (ses attributs).

```csharp
public class Personne
{
    public string Nom;
    public int Age;

    public Personne(string nom, int age) // constructeur
    {
        Nom = nom;
        Age = age;
    }
}

Personne alice = new Personne("Alice", 30); // appelle le constructeur
```

</details>

<details>
<summary>Instancier</summary>

L'action de créer un objet (une instance) à partir d'une classe, généralement à l'aide du mot-clé `new`.

```csharp
Personne alice = new Personne(); // "instancier" la classe Personne
```

</details>

## Dojo Randori

Reprenons l'exercice [Parachutes](<../Parachutes%20(OO)/>) que vous avez peut-être vu au début du module I320.

Chacun à votre tour, vous allez venir coder un petit bout de cet exercice qui nous fera revisiter la grande majorité de ces concepts OO, qui nous seront indispensables dans la suite de ce module.
