using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class Job
    {
        public short JobId { get; set; }
        
        public string JobDesc { get; set; }
        
    }
}
