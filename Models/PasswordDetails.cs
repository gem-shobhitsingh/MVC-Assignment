using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace User_Management.Models
{
    public class PasswordDetails
    {
        [Key, ForeignKey("UserDetails")]
        public int UserID { get; set; }
        public string Password { get; set; }
        public virtual UserDetails UserDetails { get; set; }
    }
}