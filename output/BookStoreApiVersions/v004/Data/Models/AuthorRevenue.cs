using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class AuthorRevenue
    {
        public int AuthorId { get; set; }
        
        public string AuthorName { get; set; }
        
        public decimal? Revenue { get; set; }
        
    }
}
