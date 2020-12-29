using System.Collections.Generic;
using Newtonsoft.Json;

public class Recordings{
    [JsonConverter(typeof(SingleValueArrayConverter<RecordInfo>))]
    public List<RecordInfo> recording { get; set; }
}
public class RecordsResponse {
    public string returncode { get; set; }
    public string url { get; set; }
    public Recordings recordings { get; set; }

    
}