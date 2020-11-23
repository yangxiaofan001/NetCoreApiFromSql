using Microsoft.EntityFrameworkCore;
using BookStoreApi.Data.Entities;

namespace BookStoreApi.Data
{
    public partial class BookStoreDBContext : DbContext
    {
        public BookStoreDBContext()
        {
        }
        public BookStoreDBContext(DbContextOptions<BookStoreDBContext> options) : base(options)
        {
        }
        
        public virtual DbSet<Author> Authors { get; set; }
        
        public virtual DbSet<AuthorRevenue> AuthorRevenues { get; set; }
        
        public virtual DbSet<Book> Books { get; set; }
        
        public virtual DbSet<BookAuthor> BookAuthors { get; set; }
        
        public virtual DbSet<BookRevenue> BookRevenues { get; set; }
        
        public virtual DbSet<Exercise> Exercises { get; set; }
        
        public virtual DbSet<Job> Jobs { get; set; }
        
        public virtual DbSet<MuscleGroup> MuscleGroups { get; set; }
        
        public virtual DbSet<Publisher> Publishers { get; set; }
        
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        
        public virtual DbSet<Role> Roles { get; set; }
        
        public virtual DbSet<Sale> Sales { get; set; }
        
        public virtual DbSet<Store> Stores { get; set; }
        
        public virtual DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=BookStoreDBContext");
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // skip fluent validation rules for now
            // wrong, that's not fluent validation, it is mapping for table, view, column, pk, fk
            
            modelBuilder.Entity<Author>(entity => {
                entity.ToTable("Author");
                
                entity.HasKey(e => new { e.AuthorId });
                
                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(40)
                    ;
                
                entity.Property(e => e.AuthorId)
                    .HasColumnName("author_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.EmailAddress)
                    .HasColumnName("email_address")
                    .HasMaxLength(100)
                    ;
                
                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .IsRequired()
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .IsRequired()
                    .HasMaxLength(40)
                    ;
                
                entity.Property(e => e.NickName)
                    .HasColumnName("nick_name")
                    .HasMaxLength(50)
                    ;
                
                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsFixedLength()
                    ;
                
                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    ;
                
                entity.Property(e => e.Zip)
                    .HasColumnName("zip")
                    .HasMaxLength(5)
                    .IsFixedLength()
                    ;
            });
            
            modelBuilder.Entity<AuthorRevenue>(entity => {
                entity.ToView("AuthorRevenue");
                entity.HasNoKey();
                
                entity.Property(e => e.AuthorId)
                    .HasColumnName("author_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.AuthorName)
                    .HasColumnName("author_name")
                    .IsRequired()
                    .HasMaxLength(30)
                    ;
                
                entity.Property(e => e.Revenue)
                    .HasColumnName("revenue")
                    ;
            });
            
            modelBuilder.Entity<Book>(entity => {
                entity.ToTable("Book");
                
                entity.HasKey(e => new { e.BookId });
                
                entity.Property(e => e.Advance)
                    .HasColumnName("advance")
                    .HasColumnType("money")
                    ;
                
                entity.Property(e => e.BookId)
                    .HasColumnName("book_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.Notes)
                    .HasColumnName("notes")
                    .HasMaxLength(200)
                    ;
                
                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("money")
                    ;
                
                entity.Property(e => e.PubId)
                    .HasColumnName("pub_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.PublishedDate)
                    .HasColumnName("published_date")
                    .IsRequired()
                    .HasColumnType("datetime")
                    ;
                
                entity.Property(e => e.Royalty)
                    .HasColumnName("royalty")
                    ;
                
                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .IsRequired()
                    .HasMaxLength(80)
                    ;
                
                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsFixedLength()
                    ;
                
                entity.Property(e => e.YtdSales)
                    .HasColumnName("ytd_sales")
                    ;
                
                entity.HasOne(d => d.Publisher)
                    .WithMany(p => p.Books)
                    .HasForeignKey(d => d.PubId)
                    .HasConstraintName("FK__Book__pub_id__6383C8BA");
            });
            
            modelBuilder.Entity<BookAuthor>(entity => {
                entity.ToTable("BookAuthor");
                
                entity.HasKey(e => new { e.AuthorId });
                
                entity.Property(e => e.AuthorId)
                    .HasColumnName("author_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.AuthorOrder)
                    .HasColumnName("author_order")
                    ;
                
                entity.Property(e => e.BookId)
                    .HasColumnName("book_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.RoyalityPercentage)
                    .HasColumnName("royality_percentage")
                    ;
                
                entity.HasOne(d => d.Author)
                    .WithMany(p => p.BookAuthors)
                    .HasForeignKey(d => d.AuthorId)
                    .HasConstraintName("FK__BookAutho__autho__6477ECF3");
                
                entity.HasOne(d => d.Book)
                    .WithMany(p => p.BookAuthors)
                    .HasForeignKey(d => d.BookId)
                    .HasConstraintName("FK__BookAutho__book___656C112C");
            });
            
            modelBuilder.Entity<BookRevenue>(entity => {
                entity.ToView("BookRevenue");
                entity.HasNoKey();
                
                entity.Property(e => e.BookName)
                    .HasColumnName("book_name")
                    .IsRequired()
                    .HasMaxLength(100)
                    ;
                
                entity.Property(e => e.ISBN)
                    .HasColumnName("ISBN")
                    .IsRequired()
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.Revenue)
                    .HasColumnName("revenue")
                    ;
            });
            
            modelBuilder.Entity<Exercise>(entity => {
                entity.ToTable("Exercise");
                
                entity.HasKey(e => new { e.ExerciseId });
                
                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(2000)
                    ;
                
                entity.Property(e => e.ExerciseId)
                    .HasColumnName("exercise_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.MuscleGroupId)
                    .HasColumnName("muscle_group_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .IsRequired()
                    .HasMaxLength(80)
                    ;
            });
            
            modelBuilder.Entity<Job>(entity => {
                entity.ToTable("Job");
                
                entity.HasKey(e => new { e.JobId });
                
                entity.Property(e => e.JobDesc)
                    .HasColumnName("job_desc")
                    .IsRequired()
                    .HasMaxLength(50)
                    ;
                
                entity.Property(e => e.JobId)
                    .HasColumnName("job_id")
                    .IsRequired()
                    ;
            });
            
            modelBuilder.Entity<MuscleGroup>(entity => {
                entity.ToTable("MuscleGroup");
                
                entity.HasKey(e => new { e.MuscleGroupId });
                
                entity.Property(e => e.MuscleGroupId)
                    .HasColumnName("muscle_group_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(80)
                    ;
            });
            
            modelBuilder.Entity<Publisher>(entity => {
                entity.ToTable("Publisher");
                
                entity.HasKey(e => new { e.PubId });
                
                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasMaxLength(30)
                    ;
                
                entity.Property(e => e.PubId)
                    .HasColumnName("pub_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.PublisherName)
                    .HasColumnName("publisher_name")
                    .HasMaxLength(40)
                    ;
                
                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    ;
            });
            
            modelBuilder.Entity<RefreshToken>(entity => {
                entity.ToTable("RefreshToken");
                
                entity.HasKey(e => new { e.TokenId });
                
                entity.Property(e => e.ExpiryDate)
                    .HasColumnName("expiry_date")
                    .IsRequired()
                    .HasColumnType("datetime")
                    ;
                
                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .IsRequired()
                    .HasMaxLength(200)
                    ;
                
                entity.Property(e => e.TokenId)
                    .HasColumnName("token_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired()
                    ;
                
                entity.HasOne(d => d.User)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__RefreshTo__user___66603565");
            });
            
            modelBuilder.Entity<Role>(entity => {
                entity.ToTable("Role");
                
                entity.HasKey(e => new { e.RoleId });
                
                entity.Property(e => e.RoleDesc)
                    .HasColumnName("role_desc")
                    .IsRequired()
                    .HasMaxLength(50)
                    ;
                
                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .IsRequired()
                    ;
            });
            
            modelBuilder.Entity<Sale>(entity => {
                entity.ToTable("Sale");
                
                entity.HasKey(e => new { e.SaleId });
                
                entity.Property(e => e.BookId)
                    .HasColumnName("book_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.OrderDate)
                    .HasColumnName("order_date")
                    .IsRequired()
                    .HasColumnType("datetime")
                    ;
                
                entity.Property(e => e.OrderNum)
                    .HasColumnName("order_num")
                    .IsRequired()
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.PayTerms)
                    .HasColumnName("pay_terms")
                    .IsRequired()
                    .HasMaxLength(12)
                    ;
                
                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.SaleId)
                    .HasColumnName("sale_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.StoreId)
                    .HasColumnName("store_id")
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsFixedLength()
                    ;
                
                entity.HasOne(d => d.Book)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.BookId)
                    .HasConstraintName("FK__Sale__book_id__6754599E");
                
                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK__Sale__store_id__68487DD7");
            });
            
            modelBuilder.Entity<Store>(entity => {
                entity.ToTable("Store");
                
                entity.HasKey(e => new { e.StoreId });
                
                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    ;
                
                entity.Property(e => e.StoreAddress)
                    .HasColumnName("store_address")
                    .HasMaxLength(40)
                    ;
                
                entity.Property(e => e.StoreId)
                    .HasColumnName("store_id")
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsFixedLength()
                    ;
                
                entity.Property(e => e.StoreName)
                    .HasColumnName("store_name")
                    .HasMaxLength(40)
                    ;
                
                entity.Property(e => e.Zip)
                    .HasColumnName("zip")
                    .HasMaxLength(5)
                    .IsFixedLength()
                    ;
            });
            
            modelBuilder.Entity<User>(entity => {
                entity.ToTable("User");
                
                entity.HasKey(e => new { e.UserId });
                
                entity.Property(e => e.EmailAddress)
                    .HasColumnName("email_address")
                    .IsRequired()
                    .HasMaxLength(100)
                    ;
                
                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(20)
                    ;
                
                entity.Property(e => e.HireDate)
                    .HasColumnName("hire_date")
                    .HasColumnType("datetime")
                    ;
                
                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    ;
                
                entity.Property(e => e.MiddleName)
                    .HasColumnName("middle_name")
                    .HasMaxLength(1)
                    .IsFixedLength()
                    ;
                
                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .IsRequired()
                    .HasMaxLength(100)
                    ;
                
                entity.Property(e => e.PubId)
                    .HasColumnName("pub_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .IsRequired()
                    ;
                
                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .IsRequired()
                    .HasMaxLength(100)
                    ;
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired()
                    ;
                
                entity.HasOne(d => d.Publisher)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.PubId)
                    .HasConstraintName("FK__User__pub_id__6A30C649");
                
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__User__role_id__693CA210");
            });
            
            OnModelCreatingPartial(modelBuilder);
        }
        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        
    }
}
