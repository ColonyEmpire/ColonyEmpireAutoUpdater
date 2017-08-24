using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UpdateWebService.Models
{
    public class AdminLogin
    {
        public AdminLogin()
        {
            Files = new List<HttpPostedFileBase>();
        }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }
    }
}