using System;
using System.ComponentModel.DataAnnotations.Schema;

public class Meeting {
    public int Id {get; set;}
    public string MeetingName {get; set;} 
    public string MeetingId {get; set;} 
    public bool Finished {get; set;}
    public DateTime StartTime {get; set;}
    public DateTime EndTime {get; set;}
    public int OwnerId {get; set;}

    public int ServiceId {get; set;}



}