using System;
using MediaBrowser.Model.Plugins;

namespace BluRayStreaming.Configurations
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration() : base()
        {
            BaseURL = "http://localhost:8080";
        }
        
        public String BaseURL { get; set; }
    }
}