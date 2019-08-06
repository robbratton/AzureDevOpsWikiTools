using System;
using System.Collections.Generic;
using Upmc.DevTools.VsTs.Entities;

namespace MarkdownParser.Entities
{
    /// <summary>
    /// Metadata about a Markdown document.
    /// </summary>
    public class MarkdownMetadata
    {
        /// <summary>
        /// Full Id Content of the Markdown Document
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Collection of Headings
        /// </summary>
        public List<Heading> Headings { get; } = new List<Heading>();

        /// <summary>
        /// Collection of HTML entities with IDs
        /// </summary>
        public List<HtmlId> HtmlIds { get; } = new List<HtmlId>();

        /// <summary>
        /// Collection of Links
        /// </summary>
        public List<Link> Links { get; } = new List<Link>();

        /// <summary>
        /// Source Information for this File
        /// </summary>
        public SourceInformation SourceInformation { get; set; }

        public override string ToString()
        {
            var output = string.Join(
                ", ",
                $"SourceInfo: {SourceInformation}",
                $"Link Count: {Links.Count}",
                $"HeadingCount: {Headings.Count}",
                $"Content Length: {Content.Length}");

            return output;
        }
    }
}