using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiAuth.Models
{
    public class WebApiAuthUser : IdentityUser
    {
        public string Locale { get; set; } = "en-US";

        public string OrganizationId { get; set; }
    }
}
