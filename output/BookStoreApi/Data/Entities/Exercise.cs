using System;
using System.Collections.Generic;

namespace BookStoreApi.Data.Entities
{
    public partial class Exercise
    {
        public int ExerciseId { get; set; }
        public int MuscleGroupId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
