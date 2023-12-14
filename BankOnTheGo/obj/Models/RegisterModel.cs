﻿using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Models
{
    public class RegisterModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Key]
        public int ID_Number { get; set; }
        public string Password { get; set; }
        public string MyProperty { get; set; }
    }
}