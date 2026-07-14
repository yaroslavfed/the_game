using System.Globalization;
using TheGame.Core.Storage;

namespace TheGame.Infrastructure.Storage;

internal static class LegacyTextParser
{
    public static int ParseInt(string value, string field, string path)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        throw Invalid(field, value, path);
    }

    public static double ParseDouble(string value, string field, string path)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        throw Invalid(field, value, path);
    }

    public static void RequireLineCount(string[] lines, int expected, string path)
    {
        if (lines.Length < expected)
        {
            throw new DataFormatException(
                $"Legacy file '{path}' contains {lines.Length} lines; at least {expected} are required.");
        }
    }

    private static DataFormatException Invalid(string field, string value, string path) =>
        new($"Legacy field '{field}' in '{path}' has invalid value '{value}'.");
}
