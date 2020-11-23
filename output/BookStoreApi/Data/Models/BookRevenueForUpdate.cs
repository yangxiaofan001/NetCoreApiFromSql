using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class BookRevenueForUpdate
    {
        public string ISBN { get; set; }
        
        public string BookName { get; set; }
        
        public decimal? Revenue { get; set; }
        
    }
}
