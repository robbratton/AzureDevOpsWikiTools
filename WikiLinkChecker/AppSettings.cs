using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WikiLinkChecker
{
    public class AppSettings
    {
        public AppSettings()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings.json", true, true)
                ;

            var configuration = builder.Build();

            configuration.Bind(this);
        }

        public string GitBranch { get; set; }

        [JsonIgnore]
        public string GitRepository => $"{Project}.wiki";

        public string Project { get; set; }

        // Even though the JSON has this as a string, it will be converted to a Uri automatically.
        public Uri ServerUri { get; set; }

        public bool ShowSuccesses { get; set; }

        public bool Verbose { get; set; }
    }
}