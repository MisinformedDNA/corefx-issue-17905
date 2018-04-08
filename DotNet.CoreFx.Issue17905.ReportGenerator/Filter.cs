using MoreLinq;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.CoreFx.Issue17905.ReportGenerator
{
    public static class Filter
    {
        public static List<TokenInfo> RemoveRecordsThatAreNotDeadAcrossAllAssemblies(IEnumerable<TokenInfo> records)
        {
            // If types are used across assemblies, filter out members that are not used in each of those assemblies
            var sharedTypes = records
                .Where(t => t.SubKind != "Struct" && t.SubKind != "Class")
                .GroupBy(a => new { a.Type, a.Namespace });
            foreach (var sharedType in sharedTypes)
            {
                int sharedTypeAssemblyCount = sharedType.DistinctBy(t => t.Assembly).Count();
                var nonDeadRecords = sharedType
                    .GroupBy(t => t.Member)
                    .Where(t => t.Count() < sharedTypeAssemblyCount)
                    .SelectMany(t => t)
                    .ToList()
                    ;

                records = records.Except(nonDeadRecords);
            }

            return records.ToList();
        }


    }
}
