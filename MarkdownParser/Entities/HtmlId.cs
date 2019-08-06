using System;

namespace MarkdownParser.Entities
{
    /// <summary>
    /// For HTML statements with Ids
    /// </summary>
    public class HtmlId
    {
        public HtmlId(string id, string statement = null)
        {
            Id = id;
            Statement = statement;
        }

        /// <summary>
        /// HTML statement that contained this ID?
        /// </summary>
        public string Statement { get; set; }

        /// <summary>
        /// Statement Id
        /// </summary>
        public string Id { get; set; }

        public override string ToString()
        {
            var output = $"{Id}'";

            return output;
        }
    }
}