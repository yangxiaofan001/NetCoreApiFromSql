using System;
using System.Collections.Generic;

namespace BookStoreApi.Data.Entities
{
    public partial class BookRevenue
    {
        public string ISBN { get; set; }
        public string BookName { get; set; }
        public decimal? Revenue { get; set; }
    }
}
