using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiAuth.Models
{
    public class ToDo
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public bool IsDone { get; set; }

        public int UserId { get; set; }

    }
}
