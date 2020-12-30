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
using streamingservice.Models;
using streamingservice.Services;

namespace streamingservice.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StreamController : ControllerBase
    {
        private UserService userService;
        AppDbContext appDbContext;
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

                if(string.IsNullOrEmpty(meetingId))
                    return BadRequest(new Response{
                        Description = "اتاق مورد نظر وجود ندارد",
                        Status = "Failed"
                    });

                return Ok(new Response{
                        Status = "Success",
                        Data = new {
                            MeetingId = meetingId
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
                        Status = "Success",
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

        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.User + "," + Roles.Trial)]
        public async Task<IActionResult> AddServiceInfo([FromBody]MeetingServicesModel servicesModel)
        {
            try
            {
                if(string.IsNullOrEmpty(servicesModel.Service_URL) || string.IsNullOrEmpty(servicesModel.Service_Key))
                    return BadRequest(new Response{
                        Description = "اطلاعات به درستی وارد نشده است\n Service_URL Or Service_Key Empty",
                        Status = "Failed"
                    });

                UserModel owner = userService.GetUserModel(User);

                if(appDbContext.Services.Where(x => x.Service_URL == servicesModel.Service_URL).FirstOrDefault() != null)
                    return BadRequest(new Response{
                        Description = "Duplicate ServiceURL",
                        Status = "Failed"
                    });

                
                servicesModel.OwnerId = owner.Id;
                servicesModel.ServiceType = ServiceType.BBB;
                
                await appDbContext.Services.AddAsync(servicesModel);
                await appDbContext.SaveChangesAsync();
                
                List<int> ids = owner.GetServiceList();
                ids.Add(servicesModel.Id);

                owner.SetServiceList(ids);
                appDbContext.Users.Update(owner);
                
                await appDbContext.SaveChangesAsync();

                return Ok(new Response{
                        Status = "Success",
                        Data = new {
                            ServiceId = servicesModel.Id
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
            }
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AddNewUser([FromBody]UserModel userModel)
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);   

                if(owner.UserName != "admin")
                    return Unauthorized();

                if(string.IsNullOrEmpty(userModel.UserName) || string.IsNullOrEmpty(userModel.MelliCode))
                    return BadRequest(new Response{
                        Description = "اطلاعات به درستی وارد نشده است\n UserName Or MelliCode Empty",
                        Status = "Failed"
                    });

                string token = TokenCreator.CreateToken(userModel.UserName , new List<string>{Roles.User});
                userModel.ConfirmedAcc = true;
                userModel.Token = token;
                

                await appDbContext.Users.AddAsync(userModel);
                await appDbContext.SaveChangesAsync();

                return Ok(new Response{
                        Data = new {
                            Token = userModel.Token
                        },
                        Status = "Success"
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

        [HttpGet]
        [Authorize(Roles = Roles.Admin + "," + Roles.User + "," + Roles.Trial)]
        public IActionResult GetServicesList()
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);

                return Ok(new Response{
                        Data = appDbContext.Services.Where(x => x.OwnerId == owner.Id).ToList(),
                        Status = "Success"
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new Response{
                        Description = "Internal Error",
                        Status = "Failed"
                    });
                throw;
            }
        }
    
    
    }
}
