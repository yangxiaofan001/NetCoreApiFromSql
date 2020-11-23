using System;
using System.Threading.Tasks;
using BookStoreApi.Data.Entities;

namespace BookStoreApi.Data
{
    public interface IBookStoreApiRepository
    {
        // general
        Task AddAsync<T>(T entity) where T : class;
        
        void Delete<T>(T entity) where T : class;
        
        Task<bool> SaveChangesAsync();
        
        // Author
        Task<int> GetAllAuthorsCountAsync();
        
        Task<EntityCollection<Author>> GetAllAuthorsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc");
        
        Task<bool> AuthorExistsAsync(int authorId);
        
        Task<Author> GetAuthorAsync(int authorId);
        
        Task<Author> GetAuthorByNickNameAsync(string nickName);
        
        // AuthorRevenue
        Task<int> GetAllAuthorRevenuesCountAsync();
        
        Task<EntityCollection<AuthorRevenue>> GetAllAuthorRevenuesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "");
        
        // Book
        Task<int> GetAllBooksCountAsync();
        
        Task<EntityCollection<Book>> GetAllBooksAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "BookId Desc");
        
        Task<bool> BookExistsAsync(int bookId);
        
        Task<Book> GetBookAsync(int bookId);
        
        Task<int> GetBooksByPubIdCountAsync(int pubId);
        
        Task<EntityCollection<Book>> GetBooksByPubIdAsync(int pubId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "BookId Desc");
        
        // BookAuthor
        Task<int> GetAllBookAuthorsCountAsync();
        
        Task<EntityCollection<BookAuthor>> GetAllBookAuthorsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc");
        
        Task<bool> BookAuthorExistsAsync(int authorId);
        
        Task<BookAuthor> GetBookAuthorAsync(int authorId);
        
        Task<int> GetBookAuthorsByAuthorIdCountAsync(int authorId);
        
        Task<EntityCollection<BookAuthor>> GetBookAuthorsByAuthorIdAsync(int authorId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc");
        
        Task<int> GetBookAuthorsByBookIdCountAsync(int bookId);
        
        Task<EntityCollection<BookAuthor>> GetBookAuthorsByBookIdAsync(int bookId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc");
        
        // BookRevenue
        Task<int> GetAllBookRevenuesCountAsync();
        
        Task<EntityCollection<BookRevenue>> GetAllBookRevenuesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "");
        
        // Exercise
        Task<int> GetAllExercisesCountAsync();
        
        Task<EntityCollection<Exercise>> GetAllExercisesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "ExerciseId Desc");
        
        Task<bool> ExerciseExistsAsync(int exerciseId);
        
        Task<Exercise> GetExerciseAsync(int exerciseId);
        
        // Job
        Task<int> GetAllJobsCountAsync();
        
        Task<EntityCollection<Job>> GetAllJobsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "JobId Desc");
        
        Task<bool> JobExistsAsync(short jobId);
        
        Task<Job> GetJobAsync(short jobId);
        
        // MuscleGroup
        Task<int> GetAllMuscleGroupsCountAsync();
        
        Task<EntityCollection<MuscleGroup>> GetAllMuscleGroupsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "MuscleGroupId Desc");
        
        Task<bool> MuscleGroupExistsAsync(int muscleGroupId);
        
        Task<MuscleGroup> GetMuscleGroupAsync(int muscleGroupId);
        
        // Publisher
        Task<int> GetAllPublishersCountAsync();
        
        Task<EntityCollection<Publisher>> GetAllPublishersAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "PubId Desc");
        
        Task<bool> PublisherExistsAsync(int pubId);
        
        Task<Publisher> GetPublisherAsync(int pubId);
        
        // RefreshToken
        Task<int> GetAllRefreshTokensCountAsync();
        
        Task<EntityCollection<RefreshToken>> GetAllRefreshTokensAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "TokenId Desc");
        
        Task<bool> RefreshTokenExistsAsync(int tokenId);
        
        Task<RefreshToken> GetRefreshTokenAsync(int tokenId);
        
        Task<int> GetRefreshTokensByUserIdCountAsync(int userId);
        
        Task<EntityCollection<RefreshToken>> GetRefreshTokensByUserIdAsync(int userId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "TokenId Desc");
        
        // Role
        Task<int> GetAllRolesCountAsync();
        
        Task<EntityCollection<Role>> GetAllRolesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "RoleId Desc");
        
        Task<bool> RoleExistsAsync(short roleId);
        
        Task<Role> GetRoleAsync(short roleId);
        
        // Sale
        Task<int> GetAllSalesCountAsync();
        
        Task<EntityCollection<Sale>> GetAllSalesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc");
        
        Task<bool> SaleExistsAsync(int saleId);
        
        Task<Sale> GetSaleAsync(int saleId);
        
        Task<int> GetSalesByBookIdCountAsync(int bookId);
        
        Task<EntityCollection<Sale>> GetSalesByBookIdAsync(int bookId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc");
        
        Task<int> GetSalesByStoreIdCountAsync(string storeId);
        
        Task<EntityCollection<Sale>> GetSalesByStoreIdAsync(string storeId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc");
        
        // Store
        Task<int> GetAllStoresCountAsync();
        
        Task<EntityCollection<Store>> GetAllStoresAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "StoreId Desc");
        
        Task<bool> StoreExistsAsync(string storeId);
        
        Task<Store> GetStoreAsync(string storeId);
        
        // User
        Task<int> GetAllUsersCountAsync();
        
        Task<EntityCollection<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc");
        
        Task<bool> UserExistsAsync(int userId);
        
        Task<User> GetUserAsync(int userId);
        
        Task<int> GetUsersByRoleIdCountAsync(short roleId);
        
        Task<EntityCollection<User>> GetUsersByRoleIdAsync(short roleId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc");
        
        Task<int> GetUsersByPubIdCountAsync(int pubId);
        
        Task<EntityCollection<User>> GetUsersByPubIdAsync(int pubId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc");
        
    }
}
