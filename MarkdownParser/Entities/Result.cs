using System.Text;
using Upmc.DevTools.VsTs.Entities;

namespace MarkdownParser.Entities
{
    public class Result
    {
        public Result(SourceInformation page, Link link, bool found)
        {
            Page = page;
            Link = link;
            Found = found;
        }

        public SourceInformation Page { get; set; }

        public Link Link { get; set; }

        public bool Found { get; set; }

        public string ToString(bool includePage = true)
        {
            var output = new StringBuilder();

            if (includePage)
            {
                output.Append($"{Page} ");
            }

            output.Append($"{Link} was {(Found ? "" : "not")} found");

            return output.ToString();
        }
    }
}
