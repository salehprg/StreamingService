using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.User;

namespace streamingservice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StreamController : ControllerBase
    {
        private UserService userService;

        public StreamController(AppDbContext _appDbContext , UserManager<UserModel> _userManager)
        {
            userService = new UserService(_userManager , _appDbContext);
        }

        [HttpGet]
        public IActionResult GetServicesList()
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("مشکلی در دریافت لیست سرویس های شما بوجود آمد");
                throw;
            }
        }
    }
}
