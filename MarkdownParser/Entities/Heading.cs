using System;

namespace MarkdownParser.Entities
{
    public class Heading
    {
        public Heading(int level, string text)
        {
            if (level < 0)
            {
                throw new ArgumentException("Value must be 1 or greater.", nameof(level));
            }

            Level = level;
            Text = text;
        }

        /// <summary>
        /// Header level 1+
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Header Id
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Generated Heading Link Id for this Instance's Id
        /// </summary>
        public string HeadingLinkText
        {
            get
            {
                var output = Text.Replace(" ", "-").ToLower();
                // todo Handle quoting special characters.

                return output;
            }
        }

        public override string ToString()
        {
            var output = $"H{Level}, '{Text}'";

            return output;
        }
    }
}