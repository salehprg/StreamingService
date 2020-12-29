using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using streamingservice.Helper;
using Microsoft.AspNetCore.Identity;
using Models;
using Models.User;

///<summary>
///use this to Operate User in all Database in moodle , LDAP , SQL
///</summary>
public class UserService {
    UserManager<UserModel> userManager;
    AppDbContext appDbContext;

    public UserService(UserManager<UserModel> _userManager , AppDbContext _appDbContext = null)
    {
        userManager = _userManager;
        appDbContext = _appDbContext;
    }

    public UserModel GetUserModel (ClaimsPrincipal User)
    {
        try
        {
            string idNumber = userManager.GetUserId(User);
            UserModel userModel = appDbContext.Users.Where(x => x.UserName == idNumber).FirstOrDefault();

            if(userModel != null)
            {
                return userModel;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            return null;
            
        }
    }


#region Roles
    public bool HasRole(UserModel userModel , string RoleName , bool OnlyThisRole = false)
    {
        try
        {
            List<string> roles = GetUserRoles(userModel).Result;
            
            if(roles != null)
            {
                if(OnlyThisRole && roles.Count == 1)
                {
                    if(roles.FirstOrDefault() == RoleName)
                    {
                        return true;
                    }
                }
                else if(OnlyThisRole)
                {
                    return false;
                }

                if(!OnlyThisRole && !string.IsNullOrEmpty(roles.Where(x => x == RoleName).FirstOrDefault()))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            return false;
        }
    }

    ///<summary>
    ///Use this function if you have User Roles List to Improve performance
    ///</summary>
    public bool HasRole(UserModel userModel , string RoleName , List<string> UserRoles , bool OnlyThisRole = false)
    {
        try
        {
            List<string> roles = UserRoles;
            
            if(roles != null)
            {
                if(OnlyThisRole && roles.Count == 1)
                {
                    if(roles.FirstOrDefault() == RoleName)
                    {
                        return true;
                    }
                }
                else if(OnlyThisRole)
                {
                    return false;
                }

                if(!OnlyThisRole && !string.IsNullOrEmpty(roles.Where(x => x == RoleName).FirstOrDefault()))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            return false;
        }
    }

    public async Task<List<string>> GetUserRoles(UserModel userModel)
    {
        try
        {
            userModel = appDbContext.Users.Where(x => x.Id == userModel.Id).FirstOrDefault();
            List<string> roles = (await userManager.GetRolesAsync(userModel)).ToList();
            
            return roles;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            return null;
        }
    }

    public async Task<List<UserModel>> GetUsersInRole(string RoleName)
    {
        try
        {
            List<UserModel> users = (await userManager.GetUsersInRoleAsync(RoleName)).ToList();
            return users;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            throw;         
        }
        
    }
#endregion
}