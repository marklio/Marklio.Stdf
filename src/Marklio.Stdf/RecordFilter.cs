using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for filtering STDF record streams by test head and site number.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class RecordFilter
{
    /// <summary>
    /// Filters the record stream to only include records matching the specified head and/or site numbers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Records that do not carry head/site information (FAR, MIR, MRR, ATR, DTR, GDR,
    /// ErrorRecord, UnknownRecord, etc.) always pass through the filter.
    /// </para>
    /// <para>
    /// Records implementing <see cref="IHeadSiteRecord"/> are filtered on both head and site.
    /// Records implementing only <see cref="IHeadRecord"/> are filtered on head alone.
    /// </para>
    /// <para>
    /// Summary records with <c>HeadNumber == 255</c> (meaning "all heads") always pass through
    /// when filtering by a specific head. Similarly, <c>SiteNumber == 255</c> always passes
    /// through when filtering by a specific site. These are aggregate records and are always relevant.
    /// </para>
    /// </remarks>
    /// <param name="source">The record stream to filter.</param>
    /// <param name="head">The head number to match, or <c>null</c> to accept any head.</param>
    /// <param name="site">The site number to match, or <c>null</c> to accept any site.</param>
    /// <returns>A filtered record stream.</returns>
    public static async IAsyncEnumerable<StdfRecord> FilterByHeadSite(
        this IAsyncEnumerable<StdfRecord> source,
        byte? head,
        byte? site,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (ShouldInclude(rec, head, site))
                yield return rec;
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="FilterByHeadSite(IAsyncEnumerable{StdfRecord}, byte?, byte?, CancellationToken)"/>.
    /// </summary>
    /// <param name="source">The record stream to filter.</param>
    /// <param name="head">The head number to match, or <c>null</c> to accept any head.</param>
    /// <param name="site">The site number to match, or <c>null</c> to accept any site.</param>
    /// <returns>A filtered record stream.</returns>
    public static IEnumerable<StdfRecord> FilterByHeadSite(
        this IEnumerable<StdfRecord> source,
        byte? head,
        byte? site)
    {
        foreach (var rec in source)
        {
            if (ShouldInclude(rec, head, site))
                yield return rec;
        }
    }

    private static bool ShouldInclude(StdfRecord rec, byte? head, byte? site)
    {
        if (rec is IHeadSiteRecord headSite)
        {
            if (head.HasValue && headSite.HeadNumber != head.Value && headSite.HeadNumber != 255)
                return false;
            if (site.HasValue && headSite.SiteNumber != site.Value && headSite.SiteNumber != 255)
                return false;
            return true;
        }

        if (rec is IHeadRecord headOnly)
        {
            if (head.HasValue && headOnly.HeadNumber != head.Value && headOnly.HeadNumber != 255)
                return false;
            return true;
        }

        // Non-head/site records always pass through
        return true;
    }
}
