using Newtonsoft.Json;
using System;

namespace TelonaiWebApi.Models
{

    public class UserChangePasswordModel
    {
        [JsonRequired]
        public string OldPassword { get; set; }
        [JsonRequired]
        public string NewPassword { get; set; }
       
        [JsonRequired]
        public string Username { get; set; }
      
    }
}