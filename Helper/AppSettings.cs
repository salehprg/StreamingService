using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace streamingservice.Helper
{
    public class AppSettings
    {
        //------------------------------
        //React Env Variables
        public static string ConnectionDb { get; set; }
        public static string JWTSecret { get; set; }

    }
}
