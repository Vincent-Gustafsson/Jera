namespace Jera
{
    using System;
    using System.Collections.Generic;

    using Jera.Frontend;

    public class Program
    {
        public static void Main(string[] args)
        {
            string source = System.IO.File.ReadAllText("test.jera");
            Lexer lexer = new (source);

            switch (lexer.Tokenize())
            {
                case Result<List<Token>, Diagnostics>.Okay tokenResult:
                    Program.DumpTokens(tokenResult.Data);
                    break;

                case Result<List<Token>, Diagnostics>.Error diagnosticResult:
                    Program.DumpDiagnostics(diagnosticResult.Data);
                    break;
            }
        }

        public static void DumpTokens(List<Token> tokens)
        {
            foreach (var token in tokens)
            {
                Console.WriteLine($"{token.Kind}: '{token.Value}' ({token.Position.Index}) ({token.Position.Line}.{token.Position.Col})");
            }
        }

        public static void DumpDiagnostics(Diagnostics diagnostics)
        {
            foreach (var report in diagnostics.Reports)
            {
                Console.Write($"[Lexer] ({report.Position.Index}) {report.Position.Line}:{report.Position.Col} ");

                switch (report.Level)
                {
                    case Diagnostics.Report.ReportLevel.Info:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("Info");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case Diagnostics.Report.ReportLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Warning");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case Diagnostics.Report.ReportLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }

                Console.WriteLine($": {report.Message}");
            }

            Environment.Exit(1);
        }
    }
}
