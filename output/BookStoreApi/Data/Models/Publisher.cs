using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class Publisher
    {
        public int PubId { get; set; }
        
        public string PublisherName { get; set; }
        
        public string City { get; set; }
        
        public string State { get; set; }
        
        public string Country { get; set; }
        
        public virtual ICollection<Book> Books { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
