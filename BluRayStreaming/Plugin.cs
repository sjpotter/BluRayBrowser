using System;
using System.Collections.Generic;
using System.IO;
using BluRayStreaming.Configurations;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace BluRayStreaming
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }
        
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "BluRay Browser",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
        
        private Guid _id = new Guid("03255608-82c5-4dae-a15f-6a4d5a0dd34d");
        
        public override Guid Id
        {
            get { return _id; }
        }
        
        public override string Name
        {
            get { return "BluRay Browser"; }
        }
        
        public override string Description
        {
            get
            {
                return "Stream your BluRays without remuxing";
            }
        }
        
        public Stream GetThumbImage()
        {
            throw new System.NotImplementedException();
        }

        public ImageFormat ThumbImageFormat {  
            get 
            {
                return ImageFormat.Png; 
            } 
        }
        
        public static Plugin Instance { get; private set; }
    }
}