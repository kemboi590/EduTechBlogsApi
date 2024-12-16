﻿using System.ComponentModel.DataAnnotations;

namespace EduTechBlogsApi.Models.ViewModels
{
    public class LoginVM
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}