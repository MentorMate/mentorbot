using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CoreHelpers.WindowsAzure.Storage.Table.Models;

namespace MentorBot.Functions.Services.AzureStorage
{
    /// <summary>A helper to convert azure query filters.</summary>
    public static class AzureQuery
    {
        private static readonly Regex ParseExp = new Regex("((AND|OR)?\\s*([A-Za-z0-9_\\-]+) (eq|<|>|>=|<=|ne|noeq) '?([A-Za-z0-9_\\-@\\.:]+)'?)+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>Create a query filter out of a query text.</summary>
        public static IEnumerable<QueryFilter> CreateQueryFilters(string query)
        {
            var matches = ParseExp.Matches(query);
            foreach (Match match in matches)
            {
                var filter = new QueryFilter
                {
                    FilterType = ParseType(match.Groups[2].Value.ToUpperInvariant()),
                    Property = match.Groups[3].Value,
                    Operator = ParseOperator(match.Groups[4].Value.ToUpperInvariant()),
                    Value = match.Groups[5].Value,
                };

                yield return filter;
            }
        }

        private static QueryFilterType ParseType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return QueryFilterType.Where;
            }

            return value == "AND" ? QueryFilterType.And : QueryFilterType.Or;
        }

        private static QueryFilterOperator ParseOperator(string value)
        {
            switch (value)
            {
                case "EQ":
                case "==":
                    return QueryFilterOperator.Equal;
                case "<": return QueryFilterOperator.Lower;
                case ">": return QueryFilterOperator.Greater;
                case "<=": return QueryFilterOperator.LowerEqual;
                case ">=": return QueryFilterOperator.GreaterEqual;
                case "!=":
                case "NOEQ":
                case "NE":
                    return QueryFilterOperator.NotEqual;
                default: throw new InvalidOperationException("The expression operator is unknown. " + value);
            }
        }
    }
}
