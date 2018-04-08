using System.Collections.Generic;
using System.Linq;

namespace DotNet.CoreFx.Issue17905.ReportGenerator
{
    public static class Filter
    {
        public static IEnumerable<TokenInfo> RemoveRecordsThatAreNotDeadAcrossAllAssemblies(this IEnumerable<TokenInfo> records)
        {
            // If types are used across assemblies, filter out members that are not used in each of those assemblies
            var nonDeadRecords = records
                .Where(t => t.SubKind != "Struct" && t.SubKind != "Class")  // Ignore since sometimes just a file link can be removed
                .GroupBy(a => new { a.Tokens, a.Type, a.Namespace })
                .Where(a => a.Any(t => t.Difference != "Removed"))
                .SelectMany(a => a);

            return records.Except(nonDeadRecords);
        }
    }
}
