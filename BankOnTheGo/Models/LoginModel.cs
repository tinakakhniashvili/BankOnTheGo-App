﻿using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        [Key]
        public int Id { get; set; }
    }
}
