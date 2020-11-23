using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class AuthorForUpdate
    {
        public string LastName { get; set; }
        
        public string FirstName { get; set; }
        
        public string Phone { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }
        
        public string State { get; set; }
        
        public string Zip { get; set; }
        
        public string EmailAddress { get; set; }
        
        public string NickName { get; set; }
        
    }
}
