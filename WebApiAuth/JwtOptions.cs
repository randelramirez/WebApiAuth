using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiAuth
{
    public class JwtOptions 
    {
        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

    }
}
