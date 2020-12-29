using System.Collections.Generic;
using Newtonsoft.Json;

public class Attendees
{
    [JsonConverter(typeof(SingleValueArrayConverter<AttendeeInfo>))]
    public List<AttendeeInfo> attendee { get; set; }
}
public class MeetingInfo {

    public string meetingName { get; set; }
    public string meetingID { get; set; }
    public string createTime { get; set; }
    public bool running { get; set; }
    public string startTime { get; set; }
    public int moderatorCount { get; set; }
    public int participantCount { get; set; }
    public Attendees attendees { get; set; }

    
}