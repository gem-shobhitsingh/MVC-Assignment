using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace User_Management.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserDetails> Users { get; set; }
        public DbSet<PasswordDetails> Passwords { get; set; }

        public ApplicationDbContext() : base("name=ApplicationDbContext")
        {
        }
    }
}