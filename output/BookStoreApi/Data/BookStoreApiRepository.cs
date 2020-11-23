using System;
using System.Linq;
using System.Threading.Tasks;
using BookStoreApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace BookStoreApi.Data
{
    public class BookStoreApiRepository : IBookStoreApiRepository
    {
        private readonly int maxPageSize = 20;
        private readonly int defaultPageSize = 20;
        private readonly BookStoreDBContext _dbContext;
        private readonly ILogger<BookStoreApiRepository> _logger;
        
        public BookStoreApiRepository(BookStoreDBContext dbContext, ILogger<BookStoreApiRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        // general
        public async Task AddAsync<T>(T entity) where T : class
        {
            _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            await _dbContext.AddAsync<T>(entity);
        }
        
        public void Delete<T>(T entity) where T : class
        {
            _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
            _dbContext.Remove<T>(entity);
        }
        
        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");
            return (await _dbContext.SaveChangesAsync() > 0);
        }
        
        public async Task<int> GetAllAuthorsCountAsync()
        {
            return await _dbContext.Authors.CountAsync();
        }
        
        public async Task<EntityCollection<Author>> GetAllAuthorsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            int totalCount = await GetAllAuthorsCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Author [] Authors =  await _dbContext.Authors.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Author> result = new EntityCollection<Author>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Authors,
            };
            return result;
        }
        
        public async Task<bool> AuthorExistsAsync(int authorId)
        {
            return await _dbContext.Authors.AnyAsync(e => e.AuthorId == authorId);
        }
        
        public async Task<Author> GetAuthorAsync(int authorId)
        {
            return await _dbContext.Authors.FirstOrDefaultAsync(e => e.AuthorId == authorId);
        }
        
        public async Task<Author> GetAuthorByNickNameAsync(string nickName)
        {
            return await _dbContext.Authors.FirstOrDefaultAsync(e => e.NickName == nickName);
        }
        
        public async Task<int> GetAllAuthorRevenuesCountAsync()
        {
            return await _dbContext.AuthorRevenues.CountAsync();
        }
        
        public async Task<EntityCollection<AuthorRevenue>> GetAllAuthorRevenuesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "")
        {
            int totalCount = await GetAllAuthorRevenuesCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            AuthorRevenue [] AuthorRevenues =  await _dbContext.AuthorRevenues.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<AuthorRevenue> result = new EntityCollection<AuthorRevenue>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = AuthorRevenues,
            };
            return result;
        }
        
        public async Task<int> GetAllBooksCountAsync()
        {
            return await _dbContext.Books.CountAsync();
        }
        
        public async Task<EntityCollection<Book>> GetAllBooksAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "BookId Desc")
        {
            int totalCount = await GetAllBooksCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Book [] Books =  await _dbContext.Books.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Book> result = new EntityCollection<Book>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Books,
            };
            return result;
        }
        
        public async Task<bool> BookExistsAsync(int bookId)
        {
            return await _dbContext.Books.AnyAsync(e => e.BookId == bookId);
        }
        
        public async Task<Book> GetBookAsync(int bookId)
        {
            return await _dbContext.Books.FirstOrDefaultAsync(e => e.BookId == bookId);
        }
        
        public async Task<int> GetBooksByPubIdCountAsync(int pubId)
        {
            return await _dbContext.Books.Where(b => b.PubId == pubId).CountAsync();
        }
        
        public async Task<EntityCollection<Book>> GetBooksByPubIdAsync(int pubId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "BookId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetBooksByPubIdCountAsync(pubId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Book [] Books =  await _dbContext.Books.Where(e => e.PubId == pubId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Book> result = new EntityCollection<Book>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Books,
            };
            return result;
        }
        
        public async Task<int> GetAllBookAuthorsCountAsync()
        {
            return await _dbContext.BookAuthors.CountAsync();
        }
        
        public async Task<EntityCollection<BookAuthor>> GetAllBookAuthorsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            int totalCount = await GetAllBookAuthorsCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            BookAuthor [] BookAuthors =  await _dbContext.BookAuthors.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<BookAuthor> result = new EntityCollection<BookAuthor>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = BookAuthors,
            };
            return result;
        }
        
        public async Task<bool> BookAuthorExistsAsync(int authorId)
        {
            return await _dbContext.BookAuthors.AnyAsync(e => e.AuthorId == authorId);
        }
        
        public async Task<BookAuthor> GetBookAuthorAsync(int authorId)
        {
            return await _dbContext.BookAuthors.FirstOrDefaultAsync(e => e.AuthorId == authorId);
        }
        
        public async Task<int> GetBookAuthorsByAuthorIdCountAsync(int authorId)
        {
            return await _dbContext.BookAuthors.Where(b => b.AuthorId == authorId).CountAsync();
        }
        
        public async Task<EntityCollection<BookAuthor>> GetBookAuthorsByAuthorIdAsync(int authorId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetBookAuthorsByAuthorIdCountAsync(authorId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            BookAuthor [] BookAuthors =  await _dbContext.BookAuthors.Where(e => e.AuthorId == authorId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<BookAuthor> result = new EntityCollection<BookAuthor>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = BookAuthors,
            };
            return result;
        }
        
        public async Task<int> GetBookAuthorsByBookIdCountAsync(int bookId)
        {
            return await _dbContext.BookAuthors.Where(b => b.BookId == bookId).CountAsync();
        }
        
        public async Task<EntityCollection<BookAuthor>> GetBookAuthorsByBookIdAsync(int bookId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetBookAuthorsByBookIdCountAsync(bookId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            BookAuthor [] BookAuthors =  await _dbContext.BookAuthors.Where(e => e.BookId == bookId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<BookAuthor> result = new EntityCollection<BookAuthor>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = BookAuthors,
            };
            return result;
        }
        
        public async Task<int> GetAllBookRevenuesCountAsync()
        {
            return await _dbContext.BookRevenues.CountAsync();
        }
        
        public async Task<EntityCollection<BookRevenue>> GetAllBookRevenuesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "")
        {
            int totalCount = await GetAllBookRevenuesCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            BookRevenue [] BookRevenues =  await _dbContext.BookRevenues.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<BookRevenue> result = new EntityCollection<BookRevenue>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = BookRevenues,
            };
            return result;
        }
        
        public async Task<int> GetAllExercisesCountAsync()
        {
            return await _dbContext.Exercises.CountAsync();
        }
        
        public async Task<EntityCollection<Exercise>> GetAllExercisesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "ExerciseId Desc")
        {
            int totalCount = await GetAllExercisesCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Exercise [] Exercises =  await _dbContext.Exercises.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Exercise> result = new EntityCollection<Exercise>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Exercises,
            };
            return result;
        }
        
        public async Task<bool> ExerciseExistsAsync(int exerciseId)
        {
            return await _dbContext.Exercises.AnyAsync(e => e.ExerciseId == exerciseId);
        }
        
        public async Task<Exercise> GetExerciseAsync(int exerciseId)
        {
            return await _dbContext.Exercises.FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);
        }
        
        public async Task<int> GetAllJobsCountAsync()
        {
            return await _dbContext.Jobs.CountAsync();
        }
        
        public async Task<EntityCollection<Job>> GetAllJobsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "JobId Desc")
        {
            int totalCount = await GetAllJobsCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Job [] Jobs =  await _dbContext.Jobs.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Job> result = new EntityCollection<Job>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Jobs,
            };
            return result;
        }
        
        public async Task<bool> JobExistsAsync(short jobId)
        {
            return await _dbContext.Jobs.AnyAsync(e => e.JobId == jobId);
        }
        
        public async Task<Job> GetJobAsync(short jobId)
        {
            return await _dbContext.Jobs.FirstOrDefaultAsync(e => e.JobId == jobId);
        }
        
        public async Task<int> GetAllMuscleGroupsCountAsync()
        {
            return await _dbContext.MuscleGroups.CountAsync();
        }
        
        public async Task<EntityCollection<MuscleGroup>> GetAllMuscleGroupsAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "MuscleGroupId Desc")
        {
            int totalCount = await GetAllMuscleGroupsCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            MuscleGroup [] MuscleGroups =  await _dbContext.MuscleGroups.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<MuscleGroup> result = new EntityCollection<MuscleGroup>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = MuscleGroups,
            };
            return result;
        }
        
        public async Task<bool> MuscleGroupExistsAsync(int muscleGroupId)
        {
            return await _dbContext.MuscleGroups.AnyAsync(e => e.MuscleGroupId == muscleGroupId);
        }
        
        public async Task<MuscleGroup> GetMuscleGroupAsync(int muscleGroupId)
        {
            return await _dbContext.MuscleGroups.FirstOrDefaultAsync(e => e.MuscleGroupId == muscleGroupId);
        }
        
        public async Task<int> GetAllPublishersCountAsync()
        {
            return await _dbContext.Publishers.CountAsync();
        }
        
        public async Task<EntityCollection<Publisher>> GetAllPublishersAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "PubId Desc")
        {
            int totalCount = await GetAllPublishersCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Publisher [] Publishers =  await _dbContext.Publishers.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Publisher> result = new EntityCollection<Publisher>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Publishers,
            };
            return result;
        }
        
        public async Task<bool> PublisherExistsAsync(int pubId)
        {
            return await _dbContext.Publishers.AnyAsync(e => e.PubId == pubId);
        }
        
        public async Task<Publisher> GetPublisherAsync(int pubId)
        {
            return await _dbContext.Publishers.FirstOrDefaultAsync(e => e.PubId == pubId);
        }
        
        public async Task<int> GetAllRefreshTokensCountAsync()
        {
            return await _dbContext.RefreshTokens.CountAsync();
        }
        
        public async Task<EntityCollection<RefreshToken>> GetAllRefreshTokensAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "TokenId Desc")
        {
            int totalCount = await GetAllRefreshTokensCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            RefreshToken [] RefreshTokens =  await _dbContext.RefreshTokens.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<RefreshToken> result = new EntityCollection<RefreshToken>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = RefreshTokens,
            };
            return result;
        }
        
        public async Task<bool> RefreshTokenExistsAsync(int tokenId)
        {
            return await _dbContext.RefreshTokens.AnyAsync(e => e.TokenId == tokenId);
        }
        
        public async Task<RefreshToken> GetRefreshTokenAsync(int tokenId)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(e => e.TokenId == tokenId);
        }
        
        public async Task<int> GetRefreshTokensByUserIdCountAsync(int userId)
        {
            return await _dbContext.RefreshTokens.Where(b => b.UserId == userId).CountAsync();
        }
        
        public async Task<EntityCollection<RefreshToken>> GetRefreshTokensByUserIdAsync(int userId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "TokenId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetRefreshTokensByUserIdCountAsync(userId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            RefreshToken [] RefreshTokens =  await _dbContext.RefreshTokens.Where(e => e.UserId == userId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<RefreshToken> result = new EntityCollection<RefreshToken>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = RefreshTokens,
            };
            return result;
        }
        
        public async Task<int> GetAllRolesCountAsync()
        {
            return await _dbContext.Roles.CountAsync();
        }
        
        public async Task<EntityCollection<Role>> GetAllRolesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "RoleId Desc")
        {
            int totalCount = await GetAllRolesCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Role [] Roles =  await _dbContext.Roles.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Role> result = new EntityCollection<Role>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Roles,
            };
            return result;
        }
        
        public async Task<bool> RoleExistsAsync(short roleId)
        {
            return await _dbContext.Roles.AnyAsync(e => e.RoleId == roleId);
        }
        
        public async Task<Role> GetRoleAsync(short roleId)
        {
            return await _dbContext.Roles.FirstOrDefaultAsync(e => e.RoleId == roleId);
        }
        
        public async Task<int> GetAllSalesCountAsync()
        {
            return await _dbContext.Sales.CountAsync();
        }
        
        public async Task<EntityCollection<Sale>> GetAllSalesAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc")
        {
            int totalCount = await GetAllSalesCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Sale [] Sales =  await _dbContext.Sales.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Sale> result = new EntityCollection<Sale>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Sales,
            };
            return result;
        }
        
        public async Task<bool> SaleExistsAsync(int saleId)
        {
            return await _dbContext.Sales.AnyAsync(e => e.SaleId == saleId);
        }
        
        public async Task<Sale> GetSaleAsync(int saleId)
        {
            return await _dbContext.Sales.FirstOrDefaultAsync(e => e.SaleId == saleId);
        }
        
        public async Task<int> GetSalesByBookIdCountAsync(int bookId)
        {
            return await _dbContext.Sales.Where(b => b.BookId == bookId).CountAsync();
        }
        
        public async Task<EntityCollection<Sale>> GetSalesByBookIdAsync(int bookId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetSalesByBookIdCountAsync(bookId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Sale [] Sales =  await _dbContext.Sales.Where(e => e.BookId == bookId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Sale> result = new EntityCollection<Sale>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Sales,
            };
            return result;
        }
        
        public async Task<int> GetSalesByStoreIdCountAsync(string storeId)
        {
            return await _dbContext.Sales.Where(b => b.StoreId == storeId).CountAsync();
        }
        
        public async Task<EntityCollection<Sale>> GetSalesByStoreIdAsync(string storeId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetSalesByStoreIdCountAsync(storeId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Sale [] Sales =  await _dbContext.Sales.Where(e => e.StoreId == storeId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Sale> result = new EntityCollection<Sale>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Sales,
            };
            return result;
        }
        
        public async Task<int> GetAllStoresCountAsync()
        {
            return await _dbContext.Stores.CountAsync();
        }
        
        public async Task<EntityCollection<Store>> GetAllStoresAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "StoreId Desc")
        {
            int totalCount = await GetAllStoresCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            Store [] Stores =  await _dbContext.Stores.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<Store> result = new EntityCollection<Store>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Stores,
            };
            return result;
        }
        
        public async Task<bool> StoreExistsAsync(string storeId)
        {
            return await _dbContext.Stores.AnyAsync(e => e.StoreId == storeId);
        }
        
        public async Task<Store> GetStoreAsync(string storeId)
        {
            return await _dbContext.Stores.FirstOrDefaultAsync(e => e.StoreId == storeId);
        }
        
        public async Task<int> GetAllUsersCountAsync()
        {
            return await _dbContext.Users.CountAsync();
        }
        
        public async Task<EntityCollection<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc")
        {
            int totalCount = await GetAllUsersCountAsync();
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            User [] Users =  await _dbContext.Users.OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<User> result = new EntityCollection<User>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Users,
            };
            return result;
        }
        
        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _dbContext.Users.AnyAsync(e => e.UserId == userId);
        }
        
        public async Task<User> GetUserAsync(int userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(e => e.UserId == userId);
        }
        
        public async Task<int> GetUsersByRoleIdCountAsync(short roleId)
        {
            return await _dbContext.Users.Where(b => b.RoleId == roleId).CountAsync();
        }
        
        public async Task<EntityCollection<User>> GetUsersByRoleIdAsync(short roleId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetUsersByRoleIdCountAsync(roleId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            User [] Users =  await _dbContext.Users.Where(e => e.RoleId == roleId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<User> result = new EntityCollection<User>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Users,
            };
            return result;
        }
        
        public async Task<int> GetUsersByPubIdCountAsync(int pubId)
        {
            return await _dbContext.Users.Where(b => b.PubId == pubId).CountAsync();
        }
        
        public async Task<EntityCollection<User>> GetUsersByPubIdAsync(int pubId, int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc")
        {
            pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);
            int totalCount = await GetUsersByPubIdCountAsync(pubId);
            int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);
            pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);
            int startRowIndex = (pageNumber - 1) * pageSize;
            int nextPage = Math.Min(pageNumber + 1, totalPages);
            int prevPage = Math.Max(1, pageNumber - 1);
            
            User [] Users =  await _dbContext.Users.Where(e => e.PubId == pubId).OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();
            EntityCollection<User> result = new EntityCollection<User>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                NextPageNumber = nextPage,
                PrevPageNumber = prevPage,
                Data = Users,
            };
            return result;
        }
        
    }
}
