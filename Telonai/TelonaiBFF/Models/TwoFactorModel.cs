﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TelonaiWebApi.Models
{
    public class TwoFactoreModel : BaseUser
    {
        [JsonRequired]
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }
        [JsonRequired]
        public bool RememberMachine { get; set; }

    }
}
