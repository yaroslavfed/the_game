using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TheGame.Infrastructure.Persistence;

internal static class SqliteRetryPolicy
{
    private const int MaxAttempts = 4;

    public static async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception exception) when (attempt < MaxAttempts && IsTransient(exception))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20 * attempt), cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception exception)
    {
        if (exception is DbUpdateConcurrencyException) return true;
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is SqliteException { SqliteErrorCode: 5 or 6 }) return true;
        return false;
    }
}
