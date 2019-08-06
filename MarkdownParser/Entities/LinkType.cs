using System;

namespace MarkdownParser.Entities
{
    public enum LinkType
    {
        Empty = 0,

        /// <summary>
        /// Normal HTTP or HTTPS Url
        /// </summary>
        Url,

        /// <summary>
        /// Absolute path to a page in the Wiki
        /// </summary>
        AbsolutePath,

        /// <summary>
        /// Relative path to a page in the Wiki
        /// </summary>
        RelativePath,

        /// <summary>
        /// Link to a heading or HTML Id on this page.
        /// </summary>
        NamedLink, 
    }
}