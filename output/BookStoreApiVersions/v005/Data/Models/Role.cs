using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class Role
    {
        public short RoleId { get; set; }
        
        public string RoleDesc { get; set; }
        
        public virtual ICollection<User> Users { get; set; }
    }
}
