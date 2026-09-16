namespace DataLib
{
    // Contrat commun à tous les matchs, quel que soit le jeu. C'est lui qui rend
    // possible un pipeline générique (CLI) : le code peut filtrer sur le joueur
    // ou sur le résultat sans connaître le type concret du match.
    public interface IMatch : ITimestamped
    {
        string Player { get; }
        bool Won { get; }
    }
}
