namespace DataLib
{
    // Un élément horodaté : la date est portée par la valeur elle-même.
    // DataSeries<T> ne s'appuie plus dessus (il stocke les dates à côté des valeurs),
    // mais l'interface reste le contrat commun du domaine : IMatch l'étend, donc
    // tout match du domaine sait donner sa date.
    public interface ITimestamped
    {
        DateTime Timestamp { get; }
    }
}
