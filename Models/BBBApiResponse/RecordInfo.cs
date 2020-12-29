using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class PlayBack
{
    [JsonConverter(typeof(SingleValueArrayConverter<FormatInfo>))]
    public List<FormatInfo> format { get; set; }
}
public class RecordInfo {

    public string name { get; set; }
    public string recordID { get; set; }
    public PlayBack playback { get; set; }

    [JsonIgnore]
    public string url { get; set; }
    [JsonIgnore]
    public DateTime date { get; set; }


    
}