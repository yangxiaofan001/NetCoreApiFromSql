using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        
        public string EmailAddress { get; set; }
        
        public string Password { get; set; }
        
        public string Source { get; set; }
        
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public string LastName { get; set; }
        
        public short RoleId { get; set; }
        
        public int PubId { get; set; }
        
        public DateTime? HireDate { get; set; }
        
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
