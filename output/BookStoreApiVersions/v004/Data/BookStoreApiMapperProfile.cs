using AutoMapper;

namespace BookStoreApi.Data
{
    public class BookStoreApiMapperProfile : Profile
    {
        public BookStoreApiMapperProfile()
        {
            this.CreateMap<Entities.BookRevenue, Models.BookRevenue>();
            this.CreateMap<Models.BookRevenueForCreate, Entities.BookRevenue>();
            this.CreateMap<Models.BookRevenueForUpdate, Entities.BookRevenue>().ReverseMap();
            this.CreateMap<Entities.AuthorRevenue, Models.AuthorRevenue>();
            this.CreateMap<Models.AuthorRevenueForCreate, Entities.AuthorRevenue>();
            this.CreateMap<Models.AuthorRevenueForUpdate, Entities.AuthorRevenue>().ReverseMap();
            this.CreateMap<Entities.Author, Models.Author>();
            this.CreateMap<Models.AuthorForCreate, Entities.Author>();
            this.CreateMap<Models.AuthorForUpdate, Entities.Author>().ReverseMap();
            this.CreateMap<Entities.Book, Models.Book>();
            this.CreateMap<Models.BookForCreate, Entities.Book>();
            this.CreateMap<Models.BookForUpdate, Entities.Book>().ReverseMap();
            this.CreateMap<Entities.BookAuthor, Models.BookAuthor>();
            this.CreateMap<Models.BookAuthorForCreate, Entities.BookAuthor>();
            this.CreateMap<Models.BookAuthorForUpdate, Entities.BookAuthor>().ReverseMap();
            this.CreateMap<Entities.Job, Models.Job>();
            this.CreateMap<Models.JobForCreate, Entities.Job>();
            this.CreateMap<Models.JobForUpdate, Entities.Job>().ReverseMap();
            this.CreateMap<Entities.Publisher, Models.Publisher>();
            this.CreateMap<Models.PublisherForCreate, Entities.Publisher>();
            this.CreateMap<Models.PublisherForUpdate, Entities.Publisher>().ReverseMap();
            this.CreateMap<Entities.RefreshToken, Models.RefreshToken>();
            this.CreateMap<Models.RefreshTokenForCreate, Entities.RefreshToken>();
            this.CreateMap<Models.RefreshTokenForUpdate, Entities.RefreshToken>().ReverseMap();
            this.CreateMap<Entities.Role, Models.Role>();
            this.CreateMap<Models.RoleForCreate, Entities.Role>();
            this.CreateMap<Models.RoleForUpdate, Entities.Role>().ReverseMap();
            this.CreateMap<Entities.Sale, Models.Sale>();
            this.CreateMap<Models.SaleForCreate, Entities.Sale>();
            this.CreateMap<Models.SaleForUpdate, Entities.Sale>().ReverseMap();
            this.CreateMap<Entities.Store, Models.Store>();
            this.CreateMap<Models.StoreForCreate, Entities.Store>();
            this.CreateMap<Models.StoreForUpdate, Entities.Store>().ReverseMap();
            this.CreateMap<Entities.User, Models.User>();
            this.CreateMap<Models.UserForCreate, Entities.User>();
            this.CreateMap<Models.UserForUpdate, Entities.User>().ReverseMap();
            this.CreateMap<Entities.MuscleGroup, Models.MuscleGroup>();
            this.CreateMap<Models.MuscleGroupForCreate, Entities.MuscleGroup>();
            this.CreateMap<Models.MuscleGroupForUpdate, Entities.MuscleGroup>().ReverseMap();
            this.CreateMap<Entities.Exercise, Models.Exercise>();
            this.CreateMap<Models.ExerciseForCreate, Entities.Exercise>();
            this.CreateMap<Models.ExerciseForUpdate, Entities.Exercise>().ReverseMap();
        }
    }
}
