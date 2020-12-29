using System.Collections.Generic;
using Newtonsoft.Json;

public class Meetings{
    [JsonConverter(typeof(SingleValueArrayConverter<MeetingInfo>))]
    public List<MeetingInfo> meeting { get; set; }
}
public class MeetingsResponse {
    public string returncode { get; set; }
    public string url { get; set; }
    public Meetings meetings { get; set; }

    
}