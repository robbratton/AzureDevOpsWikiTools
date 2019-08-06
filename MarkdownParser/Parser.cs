using System;
using System.Text.RegularExpressions;
using MarkdownParser.Entities;
using Upmc.DevTools.VsTs.Entities;

namespace MarkdownParser
{
    /// <summary>
    /// Parses Markdown (*.md) files
    /// </summary>
    public static class Parser
    {
        // Greedy version
        //private static readonly Regex LinkRegex = new Regex(@"\[(?<text>.+)\]\((?<ref>.+)\)", RegexOptions.Compiled);

        // Non-Greedy version
        private static readonly Regex LinkRegex = new Regex(@"\[(?<text>.+?)\]\((?<ref>.+?)\)", RegexOptions.Compiled);

        private static readonly Regex HtmlRegex = new Regex(@" id=""(?<id>[^""]+)""", RegexOptions.Compiled);

        /// <summary>
        /// Parse the Content of a Markdown file.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sourceInformation"></param>
        /// <returns></returns>
        public static MarkdownMetadata Parse(string content, SourceInformation sourceInformation)
        {
            var output = new MarkdownMetadata
            {
                Content = content,
                SourceInformation = sourceInformation
            };

            var lines = content.Split(new[] {"\n"}, StringSplitOptions.None);

            var lineNumber = 0;
            foreach (var line in lines)
            {
                lineNumber++;

                var trimmedLine = line.Trim();

                // todo Find Links. There could be multiple on a single line. "[text](ref)"

                // Process header line.
                if (trimmedLine.StartsWith("#"))
                {
                    // Count number of #'s and store that in Level.
                    var level = 1;

                    while (trimmedLine.Substring(level, 1).Equals("#"))
                    {
                        level++;
                    }

                    var text = trimmedLine.Substring(level).Trim();

                    var item = new Heading(level, text);
                    output.Headings.Add(item);

                    // There can be nothing more to parse on this line.
                    continue;
                }

                // Process Html Id
                var htmlMatch = HtmlRegex.Match(trimmedLine);

                if (htmlMatch.Success && htmlMatch.Groups.Count == 2)
                {
                    var item = new HtmlId(htmlMatch.Groups["id"].Value, trimmedLine);
                    output.HtmlIds.Add(item);

                    // There can be nothing more to parse on this line.
                    continue;
                }

                // Process Links
                var match = LinkRegex.Match(trimmedLine);

                if (match.Success && match.Groups.Count == 3)
                {
                    var item = new Link(match.Groups["text"].Value, match.Groups["ref"].Value, lineNumber);
                    output.Links.Add(item);
                }
            }

            return output;
        }
    }
}