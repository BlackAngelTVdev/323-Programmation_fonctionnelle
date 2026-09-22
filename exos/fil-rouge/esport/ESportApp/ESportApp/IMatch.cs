namespace ESportApp
{
    // Les trois jeux ont des statistiques propres (agent, map, champion, CS...),
    // mais ils partagent un socle commun. Cette interface décrit ce socle :
    // elle permet d'écrire UNE seule fois les prédicats (--filter) et les
    // sélecteurs (--stat), au lieu d'un jeu de tables par jeu.
    //
    // Elle vit dans EsportApp, pas dans DataSeries : la bibliothèque doit rester
    // ignorante du domaine.
    public interface IMatch
    {
        string Player { get; }
        int Kills { get; }
        int Deaths { get; }
        int Assists { get; }
        bool Won { get; }
    }
}
