using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.WPFs.Modules.Auths.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
