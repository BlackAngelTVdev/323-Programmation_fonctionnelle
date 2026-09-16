using DataLib;

namespace E_sport_traker
{
    // Contrat du DOMAINE (pas de DataLib) : un match qui compte des éliminations,
    // des assistances et des morts. Comme les trois jeux partagent ces colonnes,
    // une seule table de sélecteurs (kda / kills / assists) suffit pour tous.
    public interface ICombatMatch : IMatch
    {
        int Kills { get; }
        int Assists { get; }
        int Deaths { get; }
    }
}
