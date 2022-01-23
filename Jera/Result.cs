namespace Jera
{
    public abstract record Result<TOkay, TError>
    {
        public sealed record Okay : Result<TOkay, TError>
        {
            public TOkay Data { get; init; }
        }

        public sealed record Error : Result<TOkay, TError>
        {
            public TError Data { get; init; }
        }

        public static Result<TOkay, TError> Ok(TOkay data) => new Okay() { Data = data };
        public static Result<TOkay, TError> Err(TError data) => new Error() { Data = data };
    }
}
