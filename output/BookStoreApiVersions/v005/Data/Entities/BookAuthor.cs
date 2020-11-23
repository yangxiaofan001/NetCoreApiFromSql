using System;
using System.Collections.Generic;

namespace BookStoreApi.Data.Entities
{
    public partial class BookAuthor
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public Byte AuthorOrder { get; set; }
        public int? RoyalityPercentage { get; set; }
        public virtual Author Author { get; set; }
        public virtual Book Book { get; set; }
    }
}
