using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace BluRayStreaming
{
    public class BluRayBrowserChannel : IChannel, IRequiresMediaInfoCallback, IHasCacheKey
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IApplicationHost _appHost;
        
        
        public BluRayBrowserChannel(IHttpClient httpClient, ILogManager logManager, IApplicationHost appHost)
        {
            _httpClient = httpClient;
            _appHost = appHost;
            _logger = logManager.GetLogger(GetType().Name);
        }

        public string DataVersion
        {
            get
            {
                // Increment as needed to invalidate all caches
                return "1";
            }
        }

        public string Description
        {
            get { return "Stream your BluRays without having to remux them"; }
        }
        
        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.Debug("Category ID " + query.FolderId);

            var items = new List<ChannelItemInfo>();
            
            items = await GetMenu(query, cancellationToken).ConfigureAwait(false);
            
            return new ChannelItemResult()
            {
                Items = items
            };
        }

        private async Task<List<ChannelItemInfo>> GetMenu(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(BluRayPlaylistInfos));

            var items = new List<ChannelItemInfo>();
            
            var url = BluRayStreaming.Plugin.Instance.Configuration.BaseURL + "/list";
            if (query.FolderId != "")
            {
                url += "?dir=" + query.FolderId;
            }

            using (var response = await _httpClient.SendAsync(new HttpRequestOptions
            {
                Url = url,
                CancellationToken = cancellationToken

            }, "GET").ConfigureAwait(false))
            {
                using (var data = response.Content)
                {
                    using (var ms = new MemoryStream())
                    {
                        await data.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
                        
                        var result = (BluRayPlaylistInfos) deserializer.ReadObject(ms);

                        foreach (var item in result.Results)
                        {
                            items.Add(ResultToChannelItemInfo(item));
                        }
                    }
                }
            }

            return items;
        }

        private ChannelItemInfo ResultToChannelItemInfo(BluRayPlaylistInfo item)
        {
            ChannelItemInfo info;
            
            if (item.IsFolder)
            {
                info = new ChannelItemInfo
                {
                    Id = item.Id,
                    ImageUrl = "",
                    Name = item.Name,
                    Type = ChannelItemType.Folder
                };
            } 
            else
            {
                info = new ChannelItemInfo
                {
                    ContentType = ChannelMediaContentType.Movie,
                    Id = item.Id,
                    ImageUrl = "",
                    MediaType = ChannelMediaType.Video,
                    Name = item.Name,
                    RunTimeTicks = item.Runtime,
                    Type = ChannelItemType.Media, 
                };

                switch (item.Type.ToLower())
                {
                case "extra":
                    info.ContentType = ChannelMediaContentType.MovieExtra;
                    break;
                case "trailer":
                    info.ContentType = ChannelMediaContentType.Trailer;
                    break;
                case "episode":
                    info.ContentType = ChannelMediaContentType.Episode;
                    info.SeriesName = item.SeriesName;
                    break;
                case "tvextra":
                    info.ContentType = ChannelMediaContentType.TvExtra;
                    info.SeriesName = item.SeriesName;
                    break;
                }
            }

            return info;
        }

        public Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        
        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Movie,
                    ChannelMediaContentType.MovieExtra,
                    ChannelMediaContentType.Trailer,
                    ChannelMediaContentType.Episode,
                    ChannelMediaContentType.TvExtra,
                },
                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                }
            };
        }
        
        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Thumb:
                case ImageType.Backdrop:
                case ImageType.Primary:
                {
                    var path = GetType().Namespace + ".Images." + type.ToString().ToLower() + ".png";

                    return Task.FromResult(new DynamicImageResponse
                    {
                        Format = ImageFormat.Png,
                        HasImage = true,
                        Stream = GetType().Assembly.GetManifestResourceStream(path)
                    });
                }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);
            }
        }

        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Backdrop,
                ImageType.Primary
            };
        }
        
        public string Name
        {
            get { return "BluRay Browser"; }
        }

        public ChannelParentalRating ParentalRating
        {
            get { return ChannelParentalRating.GeneralAudience; }
        }
        
        public string GetCacheKey(string userId)
        {
            return userId;
        }
    }
}