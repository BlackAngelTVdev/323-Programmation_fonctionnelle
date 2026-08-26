namespace DataLib
{
    public class DataPoint<T>
    {
        public DateTime Timestamp { get; }
        public T Value { get; }

        public DataPoint(DateTime timestamp, T value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
