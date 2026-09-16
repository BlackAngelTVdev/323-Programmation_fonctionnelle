namespace DataLib
{
    // Un élément horodaté : la date est portée par la valeur elle-même,
    // ce qui permet à DataSeries<T> de rester une simple séquence de T.
    public interface ITimestamped
    {
        DateTime Timestamp { get; }
    }
}
