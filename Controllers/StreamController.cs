﻿using System;
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
                    return BadRequest("شما اجازه انتخاب این سرویس را ندارید");

                string meetingId = await StreamService.CreateRoom(createRoom.Meetingname , servicesModel , owner.Id);

                if(string.IsNullOrEmpty(meetingId))
                    return BadRequest(null);

                return Ok(meetingId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return BadRequest(null);
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
                    return BadRequest("اطلاعات به درستی وارد نشده است");

                UserModel owner = userService.GetUserModel(User);

                servicesModel.OwnerId = owner.Id;
                servicesModel.ServiceType = ServiceType.BBB;
                
                await appDbContext.Services.AddAsync(servicesModel);
                await appDbContext.SaveChangesAsync();
                
                List<int> ids = owner.GetServiceList();
                ids.Add(servicesModel.Id);

                owner.SetServiceList(ids);
                appDbContext.Users.Update(owner);
                
                await appDbContext.SaveChangesAsync();

                return Ok(servicesModel.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return BadRequest(-1);
                throw;
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
                    return BadRequest("اطلاعات به درستی وارد نشده است");

                string token = TokenCreator.CreateToken(userModel.UserName , new List<string>{Roles.User});
                userModel.ConfirmedAcc = true;
                userModel.Token = token;
                

                await appDbContext.Users.AddAsync(userModel);
                await appDbContext.SaveChangesAsync();

                return Ok(userModel.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return BadRequest(null);
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

                return Ok(appDbContext.Services.Where(x => x.OwnerId == owner.Id).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest("مشکلی در دریافت لیست سرویس های شما بوجود آمد");
                throw;
            }
        }
    
    
    }
}