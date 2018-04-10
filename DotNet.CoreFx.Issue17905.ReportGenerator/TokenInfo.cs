using System;

namespace DotNet.CoreFx.Issue17905.ReportGenerator
{
    public class TokenInfo : IEquatable<TokenInfo>
    {
        //public string Id { get; set; }
        //public string Kind { get; set; }
        public string SubKind { get; set; }
        //public string Static { get; set; }
        //public string Virtuality { get; set; }
        //public string Override { get; set; }
        //public string Unsafe { get; set; }
        //public string Obsoleted { get; set; }
        public string Difference { get; set; }
        public string Assembly { get; set; }
        public string Namespace { get; set; }
        public string Type { get; set; }
        public string Member { get; set; }
        public string Visibility { get; set; }
        //public string TypeId { get; set; }
        //public string TypeIsExposed { get; set; }
        //public string ReturnType { get; set; }
        public string Tokens { get; set; }
        public string HtmlDiff { get; set; }

        public bool Equals(TokenInfo other)
        {
            return Namespace == other.Namespace
                && Type == other.Type
                && Member == other.Member;
        }
    }
}
