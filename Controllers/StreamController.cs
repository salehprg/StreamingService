using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.User;
using Models.Users.Roles;
using streamingservice.Helper;
using streamingservice.Services;

namespace streamingservice.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StreamController : ControllerBase
    {
        private UserService userService;
        AppDbContext appDbContext;
        BBBApi bbbApi;
        StreamService StreamService;

        public StreamController(AppDbContext _appDbContext , UserManager<UserModel> _userManager)
        {
            appDbContext = _appDbContext;

            userService = new UserService(_userManager , _appDbContext);
            StreamService = new StreamService(appDbContext);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateStreamRoom([FromBody]CreateMeeting createRoom)
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);   
                MeetingServicesModel servicesModel = appDbContext.Services.Where(x => (x.Id == createRoom.ServiceId || x.Service_URL == createRoom.ServiceURL) && x.OwnerId == owner.Id).FirstOrDefault();

                if(servicesModel == null)
                    return BadRequest(new Response{
                        Description = "شما اجازه انتخاب این سرویس را ندارید",
                        Status = "Failed"
                    });

                string meetingId = await StreamService.CreateRoom(createRoom.Meetingname , servicesModel , owner.Id);

                bbbApi = new BBBApi(appDbContext , servicesModel.Service_URL , servicesModel.Service_Key);

                string moderatorURL = await bbbApi.JoinRoom(true , meetingId , owner.FirstName + " " + owner.LastName , owner.Id.ToString());
                string attendeeURL = await bbbApi.JoinRoom(false , meetingId ,"User" , "0");
                string rtmp = "rtmp://live.vir-gol.ir/stream/" + meetingId;
                string hls = "https://live.vir-gol.ir/hls/" + meetingId + ".m3u8" ;
                string dash = "https://live.vir-gol.ir/dash/" + meetingId + ".mpd" ;


                if(string.IsNullOrEmpty(meetingId))
                    return BadRequest(new Response{
                        Description = "ساخت اتاق مورد نظر با مشکل مواجه شد",
                        Status = "Failed"
                    });

                return Ok(new Response{
                        Status = Status.Success,
                        Data = new {
                            MeetingId = meetingId,
                            ModeratorLink = moderatorURL,
                            attendeeLink = attendeeURL,
                            rtmpLink = rtmp,
                            HLSLink = hls,
                            dashLink = dash

                        }
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return BadRequest(new Response{
                        Description = "Internal Error",
                        Status = "Failed"
                    });
                throw;
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> EndStream(string roomId)
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);   
                Meeting meeting = appDbContext.Meetings.Where(x => x.MeetingId == roomId && x.OwnerId == owner.Id).FirstOrDefault();

                if(meeting == null)
                    return BadRequest(new Response{
                        Description = "شما اجازه بستن این اتاق را ندارید",
                        Status = "Failed"
                    });

                bool result = await StreamService.EndRoom(roomId);

                return Ok(new Response{
                        Status = Status.Success,
                        Data = new {
                            EndResult = result
                        }
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return BadRequest(new Response{
                        Description = "Internal Error",
                        Status = "Failed"
                    });
                throw;
            }
        }


    }
}
