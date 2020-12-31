using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml;
using Newtonsoft.Json;
using System.Web;
using System.Net;
using Models;
using System.Linq;

namespace streamingservice.Helper
{
    public class BBBApi {
        static HttpClient client;
        string bbbUrl = "";
        string bbbSecret = "";
        AppDbContext appDbContext;

        public BBBApi(AppDbContext _appDbContext , string URL , string secret)
        {
            client = new HttpClient(); 
            appDbContext = _appDbContext;

            SetConnectionInfo(URL , secret);
        }       

        async Task<string> sendData (string data , bool joinRoom = false)
        {
            try
            {
                //data should like this
                //getMeetings
                //Or
                //getMeetingInfo?meetingID=123
                //then add checksum=???? to the end
                data = "/" + data;
                string modifiedData = "";
                if(data.IndexOf("?") != -1) // if has any query in data
                {
                    modifiedData = data + "&";
                }
                else
                {
                    modifiedData = data + "?";
                }

                string checkSum = "";
                data = data.Replace("?" , "");
                
                checkSum = SHA1Creator.sha1Creator(data + bbbSecret);

                Uri uri = new Uri (bbbUrl + modifiedData + "checksum=" + checkSum.ToLower() );
                return uri.AbsoluteUri;
                
            }
            catch
            {
                return null;
            }
            
        }

#region ApiFunctions

        public async Task<bool> CheckStatus()
        {
            try
            {
                Uri uri = new Uri (bbbUrl);
                HttpResponseMessage response = client.GetAsync(uri).Result;  // Send data then get response

                XmlDocument xmlResponse = new XmlDocument();
                xmlResponse.Load(await response.Content.ReadAsStreamAsync());
                string jsonObj = JsonConvert.SerializeXmlNode(xmlResponse , Newtonsoft.Json.Formatting.None , true);

                string responseStr = "";

                if(jsonObj.Contains("?xml"))
                {
                    string[] results = jsonObj.Split("}{");

                    responseStr = "{" + results[1];
                }
                responseStr = jsonObj;

                var meetingsInfo = JsonConvert.DeserializeObject<MeetingsResponse>(responseStr);

                return meetingsInfo.returncode == "SUCCESS";
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return false;
            }
        }
        public async Task<MeetingsResponse> GetMeetings()
        {
            try
            {
                string FunctionName = "getMeetings";
                string data = FunctionName;

                string _response = await sendData(data);

                var meetingsInfo = JsonConvert.DeserializeObject<MeetingsResponse>(_response);

                return meetingsInfo;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return null;
            }

        }
        
        public async Task<RecordsResponse> GetMeetingRecords(string meetingID)
        {
            try
            {
                string FunctionName =  (meetingID != "0" ? string.Format("getRecordings?meetingID={0}" , meetingID) : "getRecordings");
                string data = FunctionName;

                string _response = await sendData(data);

                if(_response != null)
                {
                    var recordings = JsonConvert.DeserializeObject<RecordsResponse>(_response);

                    return recordings;
                }

                return null;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return null;
            }

        }
        
        public async Task<MeetingsResponse> CreateRoom(string name , string meetingId , string callbackUrl , int duration)
        {
            try
            {
                name = HttpUtility.UrlEncode(name).ToUpper();
                //https://myapp.example.com/callback?meetingID=test01

                string notify = "<a href='" + callbackUrl + "' target='_self'>معلم گرامی  برای اتمام کلاس و ورود به صفحه حضور و غیاب خودکار روی این لینک کلیک کنید</a>";

                string notifyEncoded = WebUtility.UrlEncode(notify);
                string urlEncoded = WebUtility.UrlEncode(callbackUrl);

                string FunctionName = string.Format("create?allowStartStopRecording=true&record=true&attendeePW=ap&meetingID={1}&moderatorPW=mp&name={0}&duration={2}&logoutURL={3}&welcome={4}"
                                                     , name , meetingId , duration.ToString(), urlEncoded , notifyEncoded );
                
                string data = FunctionName;

                string _response = await sendData(data);

                var meetingsInfo = JsonConvert.DeserializeObject<MeetingsResponse>(_response);
                
                return meetingsInfo;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

                return null;
            }

        }

        public async Task<string> JoinRoom(bool teacher , string meetingId , string fullname ,string userId)
        {
            try
            {
                string password = (teacher ? "password=mp" : "password=ap");
                fullname = HttpUtility.UrlEncode(fullname).ToUpper();

                string FunctionName = string.Format("join?fullName={2}&meetingID={0}&{1}&redirect=true&userID={3}" , meetingId , password , fullname , userId);
                string data = FunctionName;

                string url = await sendData(data , true);

                return url;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return null;
            }

        }
        
        public async Task<bool> EndRoom(string meetingId)
        {
            try
            {
                string FunctionName = string.Format("end?meetingID={0}&password=mp" , meetingId);
                string data = FunctionName;

                string response = await sendData(data);

                MeetingsResponse meeting = JsonConvert.DeserializeObject<MeetingsResponse>(response);
                
                return (meeting.returncode != "FAILED");
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return false;
            }

        }
        
#endregion

        public void SetConnectionInfo(string _bbbUrl , string _bbbSecret)
        {
            bbbUrl = _bbbUrl;
            bbbSecret = _bbbSecret;
        }
    
    }
}
