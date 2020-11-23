using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class MuscleGroup
    {
        public int MuscleGroupId { get; set; }
        
        public string Name { get; set; }
        
    }
}
