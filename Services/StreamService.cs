using System;
using System.Linq;
using System.Threading.Tasks;
using Models;
using streamingservice.Helper;

namespace streamingservice.Services
{
    public class StreamService
    {
        AppDbContext appDbContext;
        public StreamService (AppDbContext _appDbContext)
        {
            appDbContext = _appDbContext;
        }

        public async Task<string> CreateRoom(string meetingName , MeetingServicesModel servicesModel , int OwnerId)
        {
            try
            {
                Meeting meetingDuplicate = null;
                string meetingId = "";

                do
                {
                    meetingId = RandomPassword.GenerateGUID(true , true , true);
                    meetingDuplicate = appDbContext.Meetings.Where(x => x.MeetingId == meetingId).FirstOrDefault();

                }while(meetingDuplicate != null);
                
                BBBApi bbbApi = new BBBApi(appDbContext , servicesModel.Service_URL , servicesModel.Service_Key);

                MeetingsResponse response = await bbbApi.CreateRoom(meetingName , meetingId , "" , 0);

                if(response != null)
                {
                    string mkdir = ShellRunner.Execute("mkdir " + meetingId);

                    Meeting meeting = new Meeting();
                    meeting.MeetingName = meetingName;
                    meeting.OwnerId = OwnerId;
                    meeting.ServiceId = servicesModel.Id;
                    meeting.MeetingId = meetingId;
                    
                    await appDbContext.Meetings.AddAsync(meeting);
                    await appDbContext.SaveChangesAsync();

                    return meetingId;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
        }
    }
}