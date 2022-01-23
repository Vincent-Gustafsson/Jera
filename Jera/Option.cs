namespace Jera
{
    public abstract record Option<T>
    {
        public sealed record Some : Option<T>
        {
            public T Data { get; init; }
        }

        public sealed record Nil : Option<T>
        {
        }

        // Convenience funtions
        public static Option<T> Of(T data) => new Some() { Data = data };
    }
}
