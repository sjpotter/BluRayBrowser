using System.Runtime.Serialization;

namespace BluRayStreaming
{
    [DataContract]
    public class BluRayPlaylistInfos
    {
        [DataMember(Name = "results")]
        public BluRayPlaylistInfo[] Results { get; set; }
    }

    public class BluRayPlaylistInfo
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }
        
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; }
        
        [DataMember(Name = "isFolder")]
        public bool IsFolder { get; set; }
        
        [DataMember(Name = "runtime", IsRequired = true)]
        public long Runtime { get; set; }
        
        [DataMember(Name = "type", IsRequired = true)]
        public string Type { get; set; }
        
        [DataMember(Name = "seriesName")]
        public string SeriesName { get; set; }
    }
}