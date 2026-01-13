// <copyright file="QuotaErrors.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Domain.Errors;

/// <summary>
/// Contains error definitions related to usage quotas.
/// </summary>
public static class QuotaErrors
{
    /// <summary>
    /// Gets an error indicating the conversions quota has been exceeded.
    /// </summary>
    /// <param name="used">The number of conversions used.</param>
    /// <param name="limit">The conversions limit.</param>
    /// <returns>An error indicating quota exceeded.</returns>
    public static Error ConversionsQuotaExceeded(int used, int limit) =>
        Error.Forbidden(
            "Quota.ConversionsExceeded",
            $"Monthly conversions quota exceeded. Used: {used}, Limit: {limit}. Quota resets at the beginning of next month.");

    /// <summary>
    /// Gets an error indicating the bytes quota has been exceeded.
    /// </summary>
    /// <param name="used">The bytes used.</param>
    /// <param name="limit">The bytes limit.</param>
    /// <returns>An error indicating quota exceeded.</returns>
    public static Error BytesQuotaExceeded(long used, long limit) =>
        Error.Forbidden(
            "Quota.BytesExceeded",
            $"Monthly data processing quota exceeded. Used: {FormatBytes(used)}, Limit: {FormatBytes(limit)}. Quota resets at the beginning of next month.");

    /// <summary>
    /// Gets an error indicating the quota was not found.
    /// </summary>
    /// <returns>An error indicating quota not found.</returns>
    public static Error NotFound() =>
        Error.NotFound("Quota.NotFound", "Usage quota not found.");

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var counter = 0;
        var number = (decimal)bytes;

        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1} {suffixes[counter]}";
    }
}
