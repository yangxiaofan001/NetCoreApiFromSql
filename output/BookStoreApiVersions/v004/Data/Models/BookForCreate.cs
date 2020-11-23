using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class BookForCreate
    {
        public string Title { get; set; }
        
        public string Type { get; set; }
        
        public int PubId { get; set; }
        
        public decimal? Price { get; set; }
        
        public decimal? Advance { get; set; }
        
        public int? Royalty { get; set; }
        
        public int? YtdSales { get; set; }
        
        public string Notes { get; set; }
        
        public DateTime PublishedDate { get; set; }
        
    }
}
