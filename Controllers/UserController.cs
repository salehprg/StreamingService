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
    public class UserController : ControllerBase
    {
        private UserService userService;
        private SignInManager<UserModel> signInManager;
        AppDbContext appDbContext;
        StreamService StreamService;

        public UserController(AppDbContext _appDbContext , UserManager<UserModel> _userManager , SignInManager<UserModel> _signInManager)
        {
            appDbContext = _appDbContext;

            userService = new UserService(_userManager , _appDbContext);
            StreamService = new StreamService(appDbContext);
            signInManager = _signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string UserName , string Password)
        {
            try
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(UserName , Password , false , false);
                if(result.Succeeded)
                {
                    UserModel userModel = appDbContext.Users.Where(x => x.UserName == UserName).FirstOrDefault();

                    return Ok(new Response{
                        Data = new {
                            userModel
                        },
                        Status = Status.Success
                    });
                }

                return Ok(new Response{
                        Data = "نام کاربری یا رمز عبور اشتباه است" ,
                        Status = Status.Failed
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
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AddNewUser([FromBody]UserModel userModel , string Password)
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
                

                Response response = await userService.CreateUser(userModel , Password , new List<string>{Roles.User});

                if(response.Status == Status.Success)
                {
                    return Ok(new Response{
                            Data = new {
                                Token = userModel.Token
                            },
                            Status = Status.Success
                        });
                }

                return Ok(new Response{
                            Data = response.Data,
                            Status = Status.Failed
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
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EditUser([FromBody]UserModel userModel)
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);   

                if(owner.UserName != "admin")
                    return Unauthorized();

                if(string.IsNullOrEmpty(userModel.UserName) || userModel.Id == 0)
                    return BadRequest(new Response{
                        Description = "اطلاعات به درستی وارد نشده است\n UserName Or Id Empty",
                        Status = "Failed"
                    });
                
                UserModel orgUserModel = appDbContext.Users.Where(x => x.UserName == userModel.UserName || x.Id == userModel.Id).FirstOrDefault();

                orgUserModel.FirstName = userModel.FirstName;
                orgUserModel.LastName = userModel.LastName;
                orgUserModel.Email = userModel.Email;
                orgUserModel.PhoneNumber = userModel.PhoneNumber;
                orgUserModel.MelliCode = userModel.MelliCode;
                orgUserModel.LimitStream = userModel.LimitStream;
                
                appDbContext.Users.Update(orgUserModel);
                await appDbContext.SaveChangesAsync();

                return Ok(new Response{
                        Data = new {
                            orgUserModel
                        },
                        Status = Status.Success
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
    
        [HttpDelete]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RemoveUser(int UserId , string Username)
        {
            try
            {
                UserModel owner = userService.GetUserModel(User);   

                if(owner.UserName != "admin")
                    return Unauthorized();

                if(string.IsNullOrEmpty(Username) || UserId == 0)
                    return BadRequest(new Response{
                        Description = "اطلاعات به درستی وارد نشده است\n Username Or Id UserId",
                        Status = "Failed"
                    });
                
                UserModel orgUserModel = appDbContext.Users.Where(x => x.UserName == Username && x.Id == UserId).FirstOrDefault();
                
                if(orgUserModel == null)
                    return BadRequest(new Response{
                        Description = "کاربر با مشخصات داده شده یافت نشد",
                        Status = "Failed"
                    });

                appDbContext.Users.Remove(orgUserModel);
                await appDbContext.SaveChangesAsync();

                return Ok(new Response{
                        Data = new {
                            orgUserModel
                        },
                        Status = Status.Success
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