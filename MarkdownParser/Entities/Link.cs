using System;

namespace MarkdownParser.Entities
{
    /// <summary>
    /// Defines a Link inside a Markdown document.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ref"></param>
        /// <param name="lineNumber"></param>
        /// <param name="linkType"></param>
        public Link(string text, string @ref, int lineNumber, LinkType linkType)
        {
            switch (linkType)
            {
                case Entities.LinkType.Empty:

                    if (!string.IsNullOrEmpty(@ref))
                    {
                        throw new ArgumentException("Value should be null or empty.", nameof(@ref));
                    }

                    Ref = null;

                    break;
                case Entities.LinkType.Url:

                    if (!IsUrl(@ref))
                    {
                        throw new ArgumentException("Value is not a valid Url.", nameof(@ref));
                    }

                    Ref = @ref;

                    break;
                case Entities.LinkType.AbsolutePath:

                    if (!IsAbsolutePath(@ref))
                    {
                        throw new ArgumentException("Value is not a valid absolute path.", nameof(@ref));
                    }

                    Ref = @ref;

                    break;
                case Entities.LinkType.RelativePath:

                    if (!IsRelativePath(@ref))
                    {
                        throw new ArgumentException("Value is not a valid relative path.", nameof(@ref));
                    }

                    Ref = @ref;

                    break;
                case Entities.LinkType.NamedLink:

                    if (!IsHeadingLink(@ref))
                    {
                        throw new ArgumentException("Value is not a valid relative path.", nameof(@ref));
                    }

                    Ref = @ref;

                    break;

                default:

                    throw new ArgumentException("Value is not supported.", nameof(linkType));
            }

            Text = text;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ref"></param>
        /// <param name="lineNumber"></param>
        public Link(string text, string @ref, int lineNumber)
        {
            Text = text;
            LineNumber = lineNumber;
            Ref = @ref;
        }

        /// <summary>
        /// Id Shown
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Reference Content
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// Link Type
        /// </summary>
        public LinkType? LinkType
        {
            get
            {
                LinkType? output = null;

                if (IsUrl(Ref))
                {
                    output = Entities.LinkType.Url;
                }
                else if (IsAbsolutePath(Ref))
                {
                    output = Entities.LinkType.AbsolutePath;
                }
                else if (IsRelativePath(Ref))
                {
                    output = Entities.LinkType.RelativePath;
                }
                else if (IsHeadingLink(Ref))
                {
                    output = Entities.LinkType.NamedLink;
                }

                return output;
            }
        }

        public int LineNumber { get; set; }

        /// <inheritdoc />
        /// >
        public override string ToString()
        {
            var output = $"Id: '{Text}', Ref: '{Ref}' on line {LineNumber}";

            return output;
        }

        private static bool IsHeadingLink(string value)
        {
            return value.StartsWith("#");
        }

        private static bool IsRelativePath(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && !IsHeadingLink(value)
                   && !IsAbsolutePath(value)
                   && !IsUrl(value);
        }

        private static bool IsAbsolutePath(string value)
        {
            return value.StartsWith("/");
        }

        private static bool IsUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}