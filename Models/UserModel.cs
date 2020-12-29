using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Models.User
{
    
    public class UserModel : IdentityUser<int> {
        
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string MelliCode {get; set;}    
        public string Token {get;set;}
        public string ServiceIds {get;set;}
        public int LimitStream {get; set;}
        public bool ConfirmedAcc {get; set;}


        public List<int> GetServiceList()
        {
            try
            {
                List<string> ids = ServiceIds.Split(',').ToList();
                List<int> serviceIds = new List<int>();

                foreach (var id in ids)
                {
                    int result = -1;
                    if(int.TryParse(id , out result))
                    {
                        serviceIds.Add(result);
                    }
                }

                return serviceIds;
            }
            catch (Exception ex)
            {
                return new List<int>();
                throw;
            }
        }
    
        public string SetServiceList(List<int> serviceIds)
        {
            try
            {
                string ids = "";

                foreach (var id in serviceIds)
                {
                    if(id != 0)
                    {
                        ids += id.ToString() + ",";
                    }
                }

                ServiceIds = ids;
                return ids;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
        }
    
    
    
    }

}