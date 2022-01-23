namespace Jera.Frontend
{
    public record Token
    {
        public record TokenPosition
        {
            public TokenPosition(int index, int line, int col)
            {
                this.Index = index;
                this.Line = line;
                this.Col = col;
            }

            public int Index { get; }
            public int Line { get; }
            public int Col { get; }
        }

        public Token(TokenType kind, string value, int index, int line, int col)
        {
            this.Kind = kind;
            this.Value = value;
            this.Position = new (index, line, col);
        }

        public TokenType Kind { get; }
        public string Value { get; }
        public TokenPosition Position { get; }
    }
}
