using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.User;
using Models.Users.Roles;
using streamingservice.Services;

namespace streamingservice.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ServiceController : ControllerBase
    {
        private UserService userService;
        AppDbContext appDbContext;
        StreamService StreamService;

        public ServiceController(AppDbContext _appDbContext , UserManager<UserModel> _userManager)
        {
            appDbContext = _appDbContext;

            userService = new UserService(_userManager , _appDbContext);
            StreamService = new StreamService(appDbContext);
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
                        Status = Status.Success,
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
        [Authorize(Roles = Roles.Admin + "," + Roles.User + "," + Roles.Trial)]
        public async Task<IActionResult> EditServiceInfo([FromBody]MeetingServicesModel servicesModel , bool terminate)
        {
            try
            {
                if(string.IsNullOrEmpty(servicesModel.Service_URL) || string.IsNullOrEmpty(servicesModel.Service_Key))
                    return BadRequest(new Response{
                        Description = "اطلاعات به درستی وارد نشده است\n Service_URL Or Service_Key Empty",
                        Status = "Failed"
                    });

                UserModel owner = userService.GetUserModel(User);
                
                MeetingServicesModel orgServiceModel = appDbContext.Services.Where(x => x.Id == servicesModel.Id).FirstOrDefault();
                if(orgServiceModel == null || orgServiceModel.OwnerId != owner.Id)
                    return BadRequest(new Response{
                        Description = "شما اجازه دسترسی به این سرویس را ندارید",
                        Status = "Failed"
                    });

                 if(appDbContext.Services.Where(x => x.Service_URL == servicesModel.Service_URL && x.Id != orgServiceModel.Id).FirstOrDefault() != null)
                    return BadRequest(new Response{
                        Description = "Duplicate ServiceURL",
                        Status = "Failed"
                    });

                orgServiceModel.Service_URL = (!string.IsNullOrEmpty(servicesModel.Service_URL.Trim()) ? servicesModel.Service_URL : orgServiceModel.Service_URL) ;
                orgServiceModel.Service_Key = (!string.IsNullOrEmpty(servicesModel.Service_Key.Trim()) ? servicesModel.Service_Key : orgServiceModel.Service_Key) ;
                
                appDbContext.Services.Update(orgServiceModel);
                await appDbContext.SaveChangesAsync();

                if(terminate)
                {
                    List<Meeting> meetings = appDbContext.Meetings.Where(x => x.ServiceId == orgServiceModel.Id && !x.Finished).ToList();

                    foreach (var meeting in meetings)
                    {
                        await StreamService.EndRoom(meeting.MeetingId);
                    }
                }

                return Ok(new Response{
                        Status = Status.Success,
                        Data = new {
                            orgServiceModel
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

        [HttpDelete]
        [Authorize(Roles = Roles.Admin + "," + Roles.User + "," + Roles.Trial)]
        public async Task<IActionResult> RemoeServiceInfo(int serviceId)
        {
            try
            {
                if(serviceId == 0)
                    return BadRequest(new Response{
                        Description = "اطلاعات به درستی وارد نشده است\n serviceId is not set",
                        Status = "Failed"
                    });

                UserModel owner = userService.GetUserModel(User);
                
                MeetingServicesModel orgServiceModel = appDbContext.Services.Where(x => x.Id == serviceId).FirstOrDefault();
                if(orgServiceModel == null || orgServiceModel.OwnerId != owner.Id)
                    return BadRequest(new Response{
                        Description = "شما اجازه دسترسی به این سرویس را ندارید",
                        Status = "Failed"
                    });

                List<Meeting> meetings = appDbContext.Meetings.Where(x => x.ServiceId == orgServiceModel.Id && !x.Finished).ToList();
                foreach (var meeting in meetings)
                {
                    await StreamService.EndRoom(meeting.MeetingId);
                }

                appDbContext.Services.Remove(orgServiceModel);
                await appDbContext.SaveChangesAsync();

                return Ok(new Response{
                        Status = Status.Success,
                        Data = new {
                            orgServiceModel
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

        [HttpGet]
        [Authorize(Roles = Roles.Admin + "," + Roles.User + "," + Roles.Trial)]
        public IActionResult GetServicesList()
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);

                return Ok(new Response{
                        Data = appDbContext.Services.Where(x => x.OwnerId == owner.Id).ToList(),
                        Status = Status.Success
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