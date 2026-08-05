namespace BackEnd.Utils.Helper;

public static class QsDateFilter
{
    public static void NormalizeDateFilter<T>(HttpRequest request, T filter)
    {
        var type = typeof(T);

        var createdAtRange = type.GetProperty("CreatedAtRange");
        var updatedAtRange = type.GetProperty("UpdatedAtRange");
        var sortByCreatedAt = type.GetProperty("SortByCreatedAt");
        var sortByUpdatedAt = type.GetProperty("SortByUpdatedAt");

        if (!string.IsNullOrWhiteSpace(request.Query["createdAtRange"]))
        {
            var dates = request.Query["createdAtRange"].ToString().Split(',');

            if (dates.Length == 2 &&
                DateTime.TryParse(dates[0], out var start) &&
                DateTime.TryParse(dates[1], out var end))
            {
                createdAtRange?.SetValue(filter, new[] { start, end });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Query["updatedAtRange"]))
        {
            var dates = request.Query["updatedAtRange"].ToString().Split(',');

            if (dates.Length == 2 &&
                DateTime.TryParse(dates[0], out var start) &&
                DateTime.TryParse(dates[1], out var end))
            {
                updatedAtRange?.SetValue(filter, new[] { start, end });
            }
        }

        if (bool.TryParse(request.Query["sortByCreatedAt"], out var createdSort))
            sortByCreatedAt?.SetValue(filter, createdSort);

        if (bool.TryParse(request.Query["sortByUpdatedAt"], out var updatedSort))
            sortByUpdatedAt?.SetValue(filter, updatedSort);
    }
}