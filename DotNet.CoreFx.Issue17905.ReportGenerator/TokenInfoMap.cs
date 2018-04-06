using CsvHelper.Configuration;

namespace DotNet.CoreFx.Issue17905.ReportGenerator
{
    public sealed class TokenInfoMap : ClassMap<TokenInfo>
    {
        public TokenInfoMap()
        {
            AutoMap();
            Map(m => m.Id).Name("ID");
            Map(m => m.SubKind).Name("Sub Kind");
            Map(m => m.Assembly).Ignore();
            Map(m => m.HtmlDiff).Ignore();
        }
    }
}
