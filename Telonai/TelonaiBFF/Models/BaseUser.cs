using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace TelonaiWebApi.Models
{

    public class BaseUser
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public bool? RememberMe { get; set; }
    }
}