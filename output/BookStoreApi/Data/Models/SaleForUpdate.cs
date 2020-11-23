using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class SaleForUpdate
    {
        public string StoreId { get; set; }
        
        public string OrderNum { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public short Quantity { get; set; }
        
        public string PayTerms { get; set; }
        
        public int BookId { get; set; }
        
    }
}
