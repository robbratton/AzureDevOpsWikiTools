using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarkdownParser;
using MarkdownParser.Entities;
using Upmc.DevTools.VsTs;
using Upmc.DevTools.VsTs.Entities;
using Upmc.DevTools.VsTs.SourceTools;

namespace WikiLinkChecker
{
    /// <summary>
    /// Command Line Class
    /// </summary>
    internal static class Program
    {
        private static readonly AppSettings AppSettings = new AppSettings();

        /// <summary>
        /// Command Line Entry Point
        /// </summary>
        private static void Main()
        {
            var client = SetUpClient();

            GetRepositoryInformation(client, out var items, out var files);

            var metadataItems = ParseInformation(files, client);

            Counts counts = new Counts();
            ConcurrentBag<Result> results = new ConcurrentBag<Result>();

            try
            {
                ProcessMetadata(metadataItems, items, counts, results);

                WriteResults(results);
            }
            catch (Exception exception)
            {
                if (exception is AggregateException ae)
                {
                    foreach (var ex in ae.InnerExceptions)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.Error.WriteLine(exception);
                }
            }

            if (counts != null)
            {
                Console.WriteLine("");
                Console.WriteLine(counts.ToString());
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("");
                //Console.WriteLine("Exit code: " + exitCode + " (" + (int)exitCode + ")");
                Console.WriteLine("DEBUG: Press x to exit.");

                while (Console.ReadKey(true).KeyChar != 'x')
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void WriteResults(IEnumerable<Result> results)
        {
            foreach (var pageGroup in results.GroupBy(x => x.Page.SourcePath))
            {
                Console.WriteLine($"Page {pageGroup.Key}");
                foreach (var item in pageGroup.OrderBy(x => x.Link.LineNumber))
                {
                    if (AppSettings.ShowSuccesses || !item.Found)
                    {
                        Console.WriteLine($"  {item.ToString(false)}");
                    }
                }

                if (!AppSettings.ShowSuccesses && pageGroup.All(x => x.Found))
                {
                    Console.WriteLine("  All links were correct.");
                }
                Console.WriteLine("");
            }
        }

        private static void ProcessMetadata(
            IEnumerable<MarkdownMetadata> metadataItems,
            SourceInformation[] items,
            Counts counts,
            ConcurrentBag<Result> results
        )
        {
            var exceptions = new ConcurrentQueue<Exception>();

            try
            {
                Parallel.ForEach(
                    metadataItems,
                    metadata =>
                    {
                        WriteOutput($"Checking {metadata.SourceInformation.SourcePath}");

                        foreach (var link in metadata.Links)
                        {
                            ProcessLink(link, items, metadata, counts, results);
                        }
                    }
                );
            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private static IEnumerable<MarkdownMetadata> ParseInformation(
            IEnumerable<SourceInformation> files,
            GitSourceTool client)
        {
            var metadataItems = new ConcurrentBag<MarkdownMetadata>();

            var exceptions = new ConcurrentQueue<Exception>();

            Parallel.ForEach(
                files,
                file =>
                {
                    try
                    {
                        WriteOutput($"Parsing {file.SourcePath}");

                        var content = client.GetItemContent(file);

                        var metadata = Parser.Parse(content, file);

                        metadataItems.Add(metadata);
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e);
                    }
                }
            );

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            return metadataItems;
        }

        private static void GetRepositoryInformation(
            GitSourceTool client,
            out SourceInformation[] items,
            out SourceInformation[] files)
        {
            var searchInformation = new SourceInformation(
                SourceType.TfsGit,
                "/",
                true,
                AppSettings.GitRepository,
                AppSettings.GitBranch);

            WriteOutput("Getting repository content...");

            items = client.GetItems(searchInformation)
                .Select(x => client.Map(x, AppSettings.GitRepository, AppSettings.GitBranch))
                .ToArray();

            WriteOutput("Selecting Markdown files...");
            
            files = items.Where(
                    x => !x.IsDirectory && x.SourcePath.EndsWith(".md", StringComparison.CurrentCultureIgnoreCase)) //.Take(50)
                .ToArray();
        }

        private static GitSourceTool SetUpClient()
        {
            var personalAccessToken = AuthenticationHelper.GetPersonalAccessToken();

            var tool = new VsTsTool(AppSettings.ServerUri, personalAccessToken, AppSettings.Project);

            var client = new GitSourceTool(tool);

            return client;
        }

        private static void ProcessLink(
            Link link,
            IEnumerable<SourceInformation> items,
            MarkdownMetadata metadata,
            Counts counts,
            ConcurrentBag<Result> results)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (link.LinkType)
            {
                case LinkType.AbsolutePath:
                    counts.CountAbsolutePaths++;

                    // Verify all absolute wiki links point to existing pages.
                    if (items.All(i => i.SourcePath != link.Ref))
                    {
                        counts.CountAbsolutePathsNotFound++;
                        results.Add(new Result(metadata.SourceInformation, link, false));
                    }
                    else
                    {
                        counts.CountAbsolutePathsFound++;
                        results.Add(new Result(metadata.SourceInformation, link, true));
                    }

                    break;
                case LinkType.RelativePath:
                    counts.CountRelativePaths++;

                    // Verify all relative wiki links point to existing pages.
                    if (items.All(i => !i.SourcePath.EndsWith(link.Ref)))
                    {
                        counts.CountRelativePathsNotFound++;
                        results.Add(new Result(metadata.SourceInformation, link, false));
                    }
                    else
                    {
                        counts.CountRelativePathsFound++;
                        results.Add(new Result(metadata.SourceInformation, link, true));
                    }

                    break;
                case LinkType.NamedLink:
                    counts.CountHeadingLinks++;
                    var refToFind = link.Ref.Substring(1); // Removes the leading #.

                    // Verify all heading links point to existing headers or HtmlIds
                    var foundInHeadings = metadata.Headings.Any(h => h.HeadingLinkText == refToFind);
                    var foundInHtml = metadata.HtmlIds.Any(h => h.Id == refToFind);

                    if (!foundInHeadings && !foundInHtml)
                    {
                        counts.CountHeadingLinksNotFound++;
                        results.Add(new Result(metadata.SourceInformation, link, false));
                    }
                    else
                    {
                        counts.CountHeadingLinksFound++;
                        results.Add(new Result(metadata.SourceInformation, link, true));
                    }

                    break;
                case LinkType.Url:

                    // todo Check Urls?
                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteOutput(string message, bool? show = null)
        {
            if (!show.HasValue)
            {
                show = AppSettings.Verbose;
            }

            if (show.Value)
            {
                Console.WriteLine(message);
            }
            else
            {
                Debug.WriteLine(message);
            }
        }

        private class Counts
        {
            public Counts()
            {
                CountHeadingLinksNotFound = 0;
                CountHeadingLinksFound = 0;
                CountHeadingLinks = 0;
                CountRelativePathsNotFound = 0;
                CountRelativePathsFound = 0;
                CountRelativePaths = 0;
                CountAbsolutePathsNotFound = 0;
                CountAbsolutePathsFound = 0;
                CountAbsolutePaths = 0;
            }

            public int CountAbsolutePaths { get; set; }
            public int CountAbsolutePathsFound { get; set; }
            public int CountAbsolutePathsNotFound { get; set; }
            public int CountRelativePaths { get; set; }
            public int CountRelativePathsFound { get; set; }
            public int CountRelativePathsNotFound { get; set; }
            public int CountHeadingLinks { get; set; }
            public int CountHeadingLinksFound { get; set; }
            public int CountHeadingLinksNotFound { get; set; }

            public override string ToString()
            {
                var output = new StringBuilder();

                output.AppendLine($"Absolute Paths:           {CountAbsolutePaths}");
                output.AppendLine($"Absolute Paths Found:     {CountAbsolutePathsFound}");
                output.AppendLine($"Absolute Paths Not Found: {CountAbsolutePathsNotFound}");
                output.AppendLine("");
                output.AppendLine($"Relative Paths:           {CountRelativePaths}");
                output.AppendLine($"Relative Paths Found:     {CountRelativePathsFound}");
                output.AppendLine($"Relative Paths Not Found: {CountRelativePathsNotFound}");
                output.AppendLine("");
                output.AppendLine($"Heading Links:            {CountHeadingLinks}");
                output.AppendLine($"Heading Links Found:      {CountHeadingLinksFound}");
                output.AppendLine($"Heading Links Not Found:  {CountHeadingLinksNotFound}");

                return output.ToString();
            }
        }
    }
}