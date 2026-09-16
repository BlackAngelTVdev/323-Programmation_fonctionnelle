namespace E_sport_traker
{
    // Le domaine, pas la librairie : DataLib ne connaît ni Kills, ni Deaths, ni
    // même le mot "KDA". C'est ici qu'on définit les indicateurs du domaine, et
    // c'est le mapper qu'on passera à DataSeries.Transform.
    public static class Metrics
    {
        // KDA = (Kills + Assists) / Deaths — la métrique commune aux trois jeux,
        // donc celle qui permet de comparer les joueurs entre eux.
        // Division en double : sinon (10 + 5) / 3 vaudrait 5 et non 5.0.
        public static double Kda(int kills, int assists, int deaths)
            => deaths <= 0 ? kills + assists : (kills + assists) / (double)deaths;

        // Surcharges par jeu : elles permettent de passer Metrics.Kda en groupe de
        // méthodes à Transform, sans lambda (le compilateur choisit la surcharge
        // d'après le type de la série).
        public static double Kda(ValorantMatch m) => Kda(m.Kills, m.Assists, m.Deaths);
        public static double Kda(Cs2Match m) => Kda(m.Kills, m.Assists, m.Deaths);
        public static double Kda(LolMatch m) => Kda(m.Kills, m.Assists, m.Deaths);
    }
}
