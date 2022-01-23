namespace Jera.Frontend
{
    using System.Collections.Generic;

    public class Lexer
    {
        private static readonly Dictionary<string, TokenType> Keywords = new ()
        {
            { "print", TokenType.PRINT },
            { "if", TokenType.IF },
            { "int", TokenType.INT },
        };

        private readonly Diagnostics diagnostics;
        private readonly string source;

        private char curChar;

        private int pos;
        private int line;
        private int col;

        public Lexer(string source)
        {
            this.diagnostics = new ();
            this.source = source;
            this.pos = -1;
            this.line = 1;
            this.col = 0;

            this.Advance();
        }

        public Result<List<Token>, Diagnostics> Tokenize()
        {
            List<Token> tokens = new ();

            var isFinishedLexing = false;
            while (!isFinishedLexing)
            {
                switch (this.GetToken())
                {
                    case Option<Token>.Nil:
                        continue;

                    case Option<Token>.Some token when token.Data.Kind != TokenType.EOF:
                        tokens.Add(token.Data);
                        continue;

                    case Option<Token>.Some token when token.Data.Kind == TokenType.EOF:
                        tokens.Add(token.Data);
                        isFinishedLexing = true;
                        continue;
                }
            }

            if (!this.diagnostics.Ok)
            {
                return Result<List<Token>, Diagnostics>.Err(this.diagnostics);
            }

            return Result<List<Token>, Diagnostics>.Ok(tokens);
        }

        private Option<Token> GetToken()
        {
            this.SkipWhitespace();
            var token = (this.curChar, this.Peek()) switch
            {
                ('+', _) => this.CreateToken(TokenType.PLUS, '+'),
                ('-', _) => this.CreateToken(TokenType.DASH, '-'),
                ('*', _) => this.CreateToken(TokenType.ASTERISK, '*'),
                ('/', _) => this.CreateToken(TokenType.SLASH, '/'),
                ('=', _) => this.CreateToken(TokenType.EQ, '='),
                (':', _) => this.CreateToken(TokenType.COLON, ':'),
                (';', _) => this.CreateToken(TokenType.SEMICOLON, ';'),

                ('\0', _) => this.CreateToken(TokenType.EOF, '\0'),
                _ => null
            };

            if (token != null)
            {
                this.Advance(token.Value.Length);
                return Option<Token>.Of(token);
            }

            if (char.IsDigit(this.curChar))
            {
                return this.LexNumber();
            }

            if (char.IsLetter(this.curChar) || this.curChar == '_')
            {
                return this.LexIdentifier();
            }

            this.Error($"Unexpected character '{this.curChar}' (U+{((int)this.curChar):X4})");
            this.Advance();
            return new Option<Token>.Nil();
        }

        private Option<Token> LexNumber()
        {
            var startPos = this.pos;
            var startLine = this.line;
            var startCol = this.col;

            while (char.IsDigit(this.curChar))
            {
                this.Advance();
            }

            if (this.curChar == '.')
            {
                this.Advance();
                if (!char.IsDigit(this.curChar))
                {
                    this.Error($"Unexpected character '{this.curChar}' (U+{((int)this.curChar):X4}) in number");
                    return new Option<Token>.Nil();
                }
                else
                {
                    while (char.IsDigit(this.curChar))
                    {
                        this.Advance();
                    }
                }
            }

            var numberText = this.source[startPos..this.pos];
            return Option<Token>.Of(this.CreateToken(TokenType.NUMBER, numberText, startPos, startLine, startCol));
        }

        private Option<Token> LexIdentifier()
        {
            var startPos = this.pos;
            var startLine = this.line;
            var startCol = this.col;

            while (char.IsLetterOrDigit(this.curChar) || this.curChar == '_')
            {
                this.Advance();
            }

            var ident = this.source[startPos..this.pos];

            if (Lexer.Keywords.TryGetValue(ident, out TokenType kind))
            {
                return Option<Token>.Of(this.CreateToken(kind, ident, startPos, startLine, startCol));
            }

            return Option<Token>.Of(this.CreateToken(TokenType.IDENT, ident, startPos, startLine, startCol));
        }

        private char Peek()
        {
            if (this.pos + 1 >= this.source.Length)
            {
                return '\0';
            }

            return this.source[this.pos + 1];
        }

        private void Advance(int length = 1)
        {
            this.pos += length;
            this.col += length;

            if (this.pos >= this.source.Length)
            {
                this.curChar = '\0';
                return;
            }

            this.curChar = this.source[this.pos];
        }

        private void AdvanceNewline(int newlineLength)
        {
            this.Advance(newlineLength);
            this.line += 1;
            this.col = 1;
        }

        private bool IsNewline(out int length)
        {
            var newlineLength = (this.curChar, this.Peek()) switch
            {
                ('\r', '\n') => 2,
                ('\n', _) => 1,
                _ => 0
            };

            length = newlineLength;
            return newlineLength > 0;
        }

        private void SkipSingleLineComment()
        {
            int newlineLength;
            while (!this.IsNewline(out newlineLength))
            {
                this.Advance();
            }

            this.AdvanceNewline(newlineLength);
        }

        private void SkipMultiLineComment()
        {
            while (!(this.curChar == '*' && this.Peek() == '/'))
            {
                if (this.IsNewline(out int newlineLength))
                {
                    this.AdvanceNewline(newlineLength);
                    continue;
                }

                this.Advance();
            }

            this.Advance(2);
        }

        private void SkipWhitespace()
        {
            for (int whitespaceCount = 1; whitespaceCount > 0;)
            {
                whitespaceCount = 0;
                while (this.IsNewline(out int newlineLength))
                {
                    this.AdvanceNewline(newlineLength);
                    whitespaceCount += 1;
                }

                while (this.curChar == ' ' || this.curChar == '\t')
                {
                    this.Advance();
                    whitespaceCount += 1;
                }

                while (this.curChar == '/' && this.Peek() == '/')
                {
                    this.SkipSingleLineComment();
                    whitespaceCount += 1;
                }

                while (this.curChar == '/' && this.Peek() == '*')
                {
                    this.SkipMultiLineComment();
                    whitespaceCount += 1;
                }
            }
        }

        private Token CreateToken(TokenType kind, char value, int index = -1, int line = 0, int col = 0)
            => this.CreateToken(kind, value.ToString(), index, line, col);

        private Token CreateToken(TokenType kind, string value, int index = -1, int line = 0, int col = 0)
        {
            index = index == -1 ? this.pos : index;
            line = line == 0 ? this.line : line;
            col = col == 0 ? this.col : col;
            return new (kind, value, index, line, col);
        }

        private void Error(string message, int index = 0, int line = 0, int col = 0)
        {
            index = index == 0 ? this.pos : index;
            line = line == 0 ? this.line : line;
            col = col == 0 ? this.col : col;
            this.diagnostics.Error(message, index, line, col);
        }
    }
}
