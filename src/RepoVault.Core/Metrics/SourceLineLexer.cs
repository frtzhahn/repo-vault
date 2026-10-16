namespace RepoVault.Core.Metrics;

public sealed record FileMetricsResult(
    string Language, int SourceLines, int CommentLines, int BlankLines, bool IsBinary);

public static class SourceLineLexer
{
    private sealed record LexerConfig(string[] LineMarkers, (string Start, string End)[] Blocks);

    // shared comment grammars, html/markdown/plain text intentionally have none
    private static readonly LexerConfig CStyle = new(["//"], [("/*", "*/")]);
    private static readonly LexerConfig Python = new(["#"], [("\"\"\"", "\"\"\""), ("'''", "'''")]);
    private static readonly LexerConfig Sql = new(["--"], [("/*", "*/")]);
    private static readonly LexerConfig None = new([], []);

    private static readonly Dictionary<string, (string Language, LexerConfig Config)> ByExtension =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".cs"] = ("C#", CStyle),
            [".java"] = ("Java", CStyle),
            [".c"] = ("C/C++", CStyle),
            [".cpp"] = ("C/C++", CStyle),
            [".cc"] = ("C/C++", CStyle),
            [".h"] = ("C/C++", CStyle),
            [".hpp"] = ("C/C++", CStyle),
            [".py"] = ("Python", Python),
            [".js"] = ("JavaScript/TypeScript", CStyle),
            [".jsx"] = ("JavaScript/TypeScript", CStyle),
            [".ts"] = ("JavaScript/TypeScript", CStyle),
            [".tsx"] = ("JavaScript/TypeScript", CStyle),
            [".mjs"] = ("JavaScript/TypeScript", CStyle),
            [".sql"] = ("SQL", Sql),
            [".html"] = ("HTML", None),
            [".htm"] = ("HTML", None),
            [".css"] = ("CSS", CStyle),
            [".scss"] = ("CSS", CStyle),
            [".md"] = ("Markdown", None),
            [".txt"] = ("Text", None),
        };

    public static FileMetricsResult ClassifyFile(string filename, byte[] content)
    {
        // binary sniff on the head of the file, same window git uses
        int scan = Math.Min(content.Length, 8000);
        for (int i = 0; i < scan; i++)
        {
            if (content[i] == 0)
            {
                return new FileMetricsResult("Binary", 0, 0, 0, true);
            }
        }

        int start = 0;
        if (content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF)
        {
            start = 3;
        }

        string text = System.Text.Encoding.UTF8.GetString(content, start, content.Length - start);
        string ext = Path.GetExtension(filename);

        string language;
        LexerConfig config;
        if (ByExtension.TryGetValue(ext, out var known))
        {
            language = known.Language;
            config = known.Config;
        }
        else if (ext.Length <= 1)
        {
            language = "Text";
            config = None;
        }
        else
        {
            language = ext.Substring(1).ToUpperInvariant();
            config = None;
        }

        int source = 0;
        int comments = 0;
        int blank = 0;
        bool inBlock = false;
        string openEnd = string.Empty;

        foreach (string raw in text.Split('\n'))
        {
            string line = raw.TrimEnd('\r').Trim();
            if (line.Length == 0)
            {
                blank++;
                continue;
            }

            bool codeSeen = false;
            int i = 0;

            while (i < line.Length)
            {
                if (inBlock)
                {
                    int end = line.IndexOf(openEnd, i, StringComparison.Ordinal);
                    if (end < 0)
                    {
                        break;
                    }

                    i = end + openEnd.Length;
                    inBlock = false;
                    continue;
                }

                int best = -1;
                bool isLineMarker = false;
                string endMarker = string.Empty;
                int markerLen = 0;

                foreach (string marker in config.LineMarkers)
                {
                    int at = line.IndexOf(marker, i, StringComparison.Ordinal);
                    if (at >= 0 && (best < 0 || at < best))
                    {
                        best = at;
                        isLineMarker = true;
                        markerLen = marker.Length;
                    }
                }

                foreach (var (blockStart, blockEnd) in config.Blocks)
                {
                    int at = line.IndexOf(blockStart, i, StringComparison.Ordinal);
                    if (at >= 0 && (best < 0 || at < best))
                    {
                        best = at;
                        isLineMarker = false;
                        endMarker = blockEnd;
                        markerLen = blockStart.Length;
                    }
                }

                if (best < 0)
                {
                    if (line.Substring(i).Trim().Length > 0)
                    {
                        codeSeen = true;
                    }

                    break;
                }

                if (line.Substring(i, best - i).Trim().Length > 0)
                {
                    codeSeen = true;
                }

                if (isLineMarker)
                {
                    break;
                }

                int close = line.IndexOf(endMarker, best + markerLen, StringComparison.Ordinal);
                if (close < 0)
                {
                    inBlock = true;
                    openEnd = endMarker;
                    break;
                }

                i = close + endMarker.Length;
            }

            // code wins ties, a line with both counts as source
            if (codeSeen)
            {
                source++;
            }
            else
            {
                comments++;
            }
        }

        return new FileMetricsResult(language, source, comments, blank, false);
    }
}
