using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class RefreshToken
    {
        public int TokenId { get; set; }
        
        public int UserId { get; set; }
        
        public string Token { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
    }
}
