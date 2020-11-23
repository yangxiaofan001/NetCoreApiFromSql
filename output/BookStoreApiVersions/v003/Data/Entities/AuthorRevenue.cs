using System;
using System.Collections.Generic;

namespace BookStoreApi.Data.Entities
{
    public partial class AuthorRevenue
    {
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public decimal? Revenue { get; set; }
    }
}
