using System;
using System.Security.Cryptography;
using System.Text;

public class SHA1Creator {
    public static string sha1Creator(string data)
    {
        string result = "";
        using(SHA1 sha1Hash = SHA1.Create())
        {
            //From String to byte array
            byte[] sourceBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
            result = BitConverter.ToString(hashBytes).Replace("-",String.Empty);
        }

        return result;
    }
}