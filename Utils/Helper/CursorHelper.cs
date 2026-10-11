using System.Globalization;
using System.Text;

namespace BackEnd.Utils.Helper;

public static class CursorHelper
{
    public static string Encode(DateTimeOffset timestamp, Guid id)
    {
        var value = $"{timestamp:O}|{id:D}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static (DateTimeOffset Timestamp, Guid Id)? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var value = Encoding.UTF8.GetString(
                Convert.FromBase64String(cursor));

            var parts = value.Split('|');

            if (parts.Length != 2 ||
                !DateTimeOffset.TryParse(
                    parts[0],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var timestamp) ||
                !Guid.TryParse(parts[1], out var id))
            {
                throw new ArgumentException("Invalid pagination cursor.");
            }

            return (timestamp, id);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid pagination cursor.");
        }
    }
}
