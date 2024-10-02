using Newtonsoft.Json;
using System;

namespace TelonaiWebApi.Models
{

    public class PasswordChangeModel 
    {
        public string Code { get; set; }
        public string Username { get; set; }
        public string NewPassword { get; set; }

    }
}