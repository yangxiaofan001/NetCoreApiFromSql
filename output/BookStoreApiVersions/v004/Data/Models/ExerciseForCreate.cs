using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Data.Models
{
    public partial class ExerciseForCreate
    {
        public int MuscleGroupId { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
    }
}
