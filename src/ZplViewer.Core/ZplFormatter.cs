using System.Text;

namespace ZplViewer.Core;

/// <summary>Lossless command slices for presentation only. Never use the display text as printer input.</summary>
public static class ZplFormatter
{
    public static IReadOnlyList<string> GetSegments(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        // Binary graphics/downloads can contain command-looking bytes. Keep these streams opaque.
        if (source.Contains("^GFB", StringComparison.Ordinal) || source.Contains("~DB", StringComparison.Ordinal)
            || source.Contains("~DT", StringComparison.Ordinal) || source.Contains("~DU", StringComparison.Ordinal))
            return [source];

        var segments = new List<string>();
        var start = 0;
        var index = 0;
        var formatPrefix = '^';
        var controlPrefix = '~';
        while (index + 2 < source.Length)
        {
            if ((source[index] != formatPrefix && source[index] != controlPrefix)
                || !IsCommandCharacter(source[index + 1])
                || !(IsCommandCharacter(source[index + 2])
                    || (source[index] == formatPrefix && source[index + 1] == 'A' && char.IsAsciiLetterLower(source[index + 2]))))
            {
                index++;
                continue;
            }

            if (index > start)
                segments.Add(source[start..index]);
            start = index;
            var command = source.Substring(index + 1, 2);
            index += 3;
            if (command is "FD" or "FV" or "FX")
            {
                var end = source.IndexOf($"{formatPrefix}FS", index, StringComparison.Ordinal);
                index = end < 0 ? source.Length : end;
            }
            else if (command is "CC" or "CT")
            {
                // Consume the parameter before searching for the next command.
                if (index < source.Length && source[index] is not '\r' and not '\n')
                {
                    if (command == "CC") formatPrefix = source[index];
                    else controlPrefix = source[index];
                    index++;
                }
            }
            // Custom prefixes may introduce binary data we cannot safely tokenize.
            if (command is "GF" && index < source.Length && source[index] == 'B')
                return [source];
        }
        if (start < source.Length)
            segments.Add(source[start..]);
        return segments;
    }

    public static string Format(string source) => Format(GetSegments(source));

    public static string Format(IReadOnlyList<string> segments)
    {
        var output = new StringBuilder();
        foreach (var segment in segments)
        {
            if (output.Length > 0 && output[^1] is not '\r' and not '\n')
                output.AppendLine();
            output.Append(segment);
        }
        return output.ToString();
    }

    private static bool IsCommandCharacter(char value) => value is >= 'A' and <= 'Z' or >= '0' and <= '9' or '@';
}
