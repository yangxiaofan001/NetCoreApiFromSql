using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class Store
    {
        public string StoreId { get; set; }
        
        public string StoreName { get; set; }
        
        public string StoreAddress { get; set; }
        
        public string City { get; set; }
        
        public string State { get; set; }
        
        public string Zip { get; set; }
        
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
