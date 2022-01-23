namespace Jera
{
    using System.Collections.Generic;

    public class Diagnostics
    {
        public record Report
        {
            public enum ReportLevel
            {
                Info,
                Warning,
                Error,
            }

            public record ReportPosition
            {
                public ReportPosition(int index, int line, int col)
                {
                    this.Index = index;
                    this.Line = line;
                    this.Col = col;
                }

                public int Index { get; }
                public int Line { get; }
                public int Col { get; }
            }

            public Report(string message, ReportLevel level, int index, int line, int col)
            {
                this.Message = message;
                this.Level = level;
                this.Position = new (index, line, col);
            }

            public string Message { get; }
            public ReportLevel Level { get; }
            public ReportPosition Position { get; }
        }

        public Diagnostics()
        {
            this.Reports = new ();
        }

        public List<Report> Reports { get; }

        public bool Ok => this.Reports.Count == 0;

        public void Info(string message, int index, int line, int col)
            => this.Reports.Add(new (message, Report.ReportLevel.Info, index, line, col));

        public void Warning(string message, int index, int line, int col)
            => this.Reports.Add(new (message, Report.ReportLevel.Warning, index, line, col));

        public void Error(string message, int index, int line, int col)
            => this.Reports.Add(new (message, Report.ReportLevel.Error, index, line, col));
    }
}
