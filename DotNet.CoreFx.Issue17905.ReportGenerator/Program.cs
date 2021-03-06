﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DotNet.CoreFx.Issue17905.ReportGenerator
{
    class Program
    {
        private const string BaseUrl = "http://tempcoverage.blob.core.windows.net/report3/";
        private static List<TokenInfo> Records = new List<TokenInfo>();

        static void Main()
        {
            var files = Directory.EnumerateFiles(@"..\..\..\..\DotNet.CoreFx.Issue17905.DownloadDrop\files", "*.csv");
            foreach (var file in files)
            {
                ProcessFile(file);
            }

            Records = Records
                .RemoveRecordsThatAreNotDeadAcrossAllAssemblies()
                .Where(t => t.Difference == "Removed")
                .ToList();

            using (var stream = File.Create("issues.csv"))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(Records);
            }

            Console.WriteLine(Records.Count);

            var issueCounts = Records
                .GroupBy(r => r.Assembly)
                .ToDictionary(r => r.Key, r => r.Count());
            using (var stream = File.Create("issueCounts.csv"))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(issueCounts);
            }
        }

        private static void ProcessFile(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var htmlDiff = $"=HYPERLINK(\"{BaseUrl}{fileName}.html\")";
            var assembly = fileName.Substring(0, fileName.Length - 5);

            try
            {
                using (var stream = File.OpenRead(file))
                using (var reader = new StreamReader(stream))
                {
                    using (var csv = new CsvReader(reader))
                    {
                        csv.Configuration.RegisterClassMap<TokenInfoMap>();

                        // Remove records that are likely false positives (98+% of records)
                        IEnumerable<TokenInfo> records = csv.GetRecords<TokenInfo>()
                            .Where(t => !t.Tokens.Contains(" const "))
                            .Where(t => t.Visibility != "Private" || t.Member != ".ctor()")
                            .Where(t => t.Type != "SR")
                            .Where(t => t.Type != "__BlockReflectionAttribute")
                            .Where(t => t.Tokens != "internal static class HResults")
                            .Where(t => t.SubKind != "Class" || !t.Tokens.Contains("static class"))
                            .ToList()
                            ;

                        // Remove Enums and fields on the Enum
                        var enumTypes = records.Where(t => t.SubKind == "Enum");
                        foreach (var enumType in enumTypes)
                        {
                            records = records
                                .Where(t => t.SubKind != "Field" || t.Namespace != enumType.Namespace || t.Type != enumType.Type);
                        }
                        records = records
                            .Where(t => !t.SubKind.StartsWith("Enum"))
                            .ToList();

                        // Remove public parameterless constructor if its the only constructor
                        var publicConstructors = records.Where(t => t.Visibility == "Public" && t.SubKind == "Constructor").ToList();
                        var parameterlessConstructors = publicConstructors.Where(t => t.Member == ".ctor()");
                        foreach (var constructor in parameterlessConstructors)
                        {
                            bool IsRelatedConstructor(TokenInfo t) => t.Namespace == constructor.Namespace && t.Type == constructor.Type && t.Visibility == "Public" && t.SubKind == "Constructor";

                            int numConstructorsInType = records.Count(IsRelatedConstructor);
                            if (numConstructorsInType == 1)
                            {
                                (records as List<TokenInfo>).Remove(constructor);
                            }
                        }

                        // Remove dead equality checks when both `==` and `!=` are not included
                        var operators = records
                            .Where(r => r.Member.StartsWith("op_Equality") || r.Member.StartsWith("op_Inequality"))
                            .GroupBy(r => new { r.Namespace, r.Type, MemberTypes = GetTypes(r) })
                            .Where(r => r.Count(t => t.Difference == "Removed") != 2)
                            .SelectMany(r => r);
                        records = records.Except(operators).ToList();

                        // Finishing up
                        foreach (var record in records)
                        {
                            record.Assembly = assembly;
                            record.HtmlDiff = htmlDiff;
                        }

                        Records.AddRange(records);
                    }
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        private static string GetTypes(TokenInfo token)
        {
            var member = token.Member;
            var index = member.IndexOf('(');
            return member.Substring(index);
        }
    }
}
