
namespace BookStoreApi.Data
{
    public class EntityCollection <T> where T : class
    {
        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int PageNumber { get; set; }

        public string SortBy { get; set; }

        public int NextPageNumber { get; set; }

        public int PrevPageNumber { get; set; }

        public T[] Data { get; set;}
    }
}
