using System;
using System.IO;
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
                
                //BBBApi bbbApi = new BBBApi(appDbContext , servicesModel.Service_URL , servicesModel.Service_Key);

                //MeetingsResponse response = await bbbApi.CreateRoom(meetingName , meetingId , "" , 0);

                string mkdir = ShellRunner.Execute("mkdir Rooms/" + meetingId);
                string cp = ShellRunner.Execute("cp docker-compose.yml Rooms/" + meetingId);

                string dockerCompose = File.ReadAllText("./Rooms/" + meetingId + "/docker-compose.yml");

                dockerCompose = dockerCompose.Replace("BBBURL" , servicesModel.Service_URL);
                dockerCompose = dockerCompose.Replace("BBBSECRET" , servicesModel.Service_Key);
                dockerCompose = dockerCompose.Replace("BBBMEETINGID" , meetingId);

                File.WriteAllText("./Rooms/" + meetingId + "/docker-compose.yml" , dockerCompose);

                string cd = ShellRunner.Execute("cd Rooms/" + meetingId);
                string dockerUp = ShellRunner.Execute("docker-compose up -d");

                Meeting meeting = new Meeting();
                meeting.StartTime = MyDateTime.Now();
                meeting.MeetingName = meetingName;
                meeting.OwnerId = OwnerId;
                meeting.ServiceId = servicesModel.Id;
                meeting.MeetingId = meetingId;
                meeting.Finished = false;
                
                await appDbContext.Meetings.AddAsync(meeting);
                await appDbContext.SaveChangesAsync();

                return meetingId;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
                throw;
            }
        }
    
        public async Task<bool> EndRoom(string Roomid)
        {
            try
            {
                Meeting meeting = appDbContext.Meetings.Where(x => x.MeetingId == Roomid).FirstOrDefault();
                meeting.Finished = true;
                meeting.EndTime = MyDateTime.Now();

                appDbContext.Meetings.Update(meeting);
                await appDbContext.SaveChangesAsync();

                string cd = ShellRunner.Execute("cd Rooms/" + meeting.MeetingId);
                string dockerDown = ShellRunner.Execute("docker-compose down");
                Directory.Delete("./Rooms/" + meeting.MeetingId , true);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
                throw;
            }
        }
    
    }
}
