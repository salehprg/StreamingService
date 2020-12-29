using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace streamingservice.Helper
{
    public class TokenCreator
    {
        public static string CreateToken(string username , List<string> roles)
        {
            try
            {
                List<Claim> authClaims = new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.GivenName, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),          
                };

                foreach (var item in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, item)); // Add Users role
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.JWTSecret));

                var token = new JwtSecurityToken(
                    issuer: "https://localhost:5001",
                    audience: "https://localhost:5001",
                    claims: authClaims,  // Add user Roles to token
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    ) ;

                string result = new JwtSecurityTokenHandler().WriteToken(token);
                
                return result;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
        }
    }
}