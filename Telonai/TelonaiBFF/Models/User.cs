using Newtonsoft.Json;
using System;

namespace TelonaiWebApi.Models
{

    public class User:BaseUser
    {
        [JsonRequired]
        public string ActivationCode { get; set; }
        [JsonRequired]
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        [JsonRequired]
        public string Lastname { get; set; }
        [JsonRequired]
        public string Username { get; set; }
        [JsonRequired]
        public string Email { get; set; }
        [JsonRequired]
        public string Password { get; set; }

        public string MobilePhone { get; set; }
    }
}