using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAuth.Models;

namespace WebApiAuth
{
    public class DataSeeder
    {
        private readonly DataContext context;

        public DataSeeder(DataContext context)
        {
            this.context = context;
        }

        public void SeedTodos()
        {
            var todos = new List<ToDo>();
            todos.Add(new ToDo { Title = "Create JWT Token", IsDone = false, UserId = 1 });
            todos.Add(new ToDo { Title = "Code Review", IsDone = true, UserId =1 });
            todos.Add(new ToDo { Title = "Study State Management React", IsDone = true, UserId = 1 });
            todos.Add(new ToDo { Title = "Finish CSS Course", IsDone = false, UserId = 1 });
            todos.Add(new ToDo { Title = "Finish Responsive Web Design Course", IsDone = false, UserId = 1 });

            this.context.AddRange(todos);
            this.context.SaveChanges();
        }
    }
}
