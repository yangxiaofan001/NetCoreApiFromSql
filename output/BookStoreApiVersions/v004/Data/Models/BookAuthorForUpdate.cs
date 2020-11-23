using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class BookAuthorForUpdate
    {
        public int BookId { get; set; }
        
        public Byte AuthorOrder { get; set; }
        
        public int? RoyalityPercentage { get; set; }
        
    }
}
