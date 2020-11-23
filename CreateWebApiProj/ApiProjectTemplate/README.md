
### version 001 - empty webapi template, add SeriLog
- `dornet new webapi`
- example api
    - dotnet run
    - `https://localhost:5001/WeatherForeCast`

- add packages
    - Serilog
    - Serilog.AspNetCore
    - Serilog.Sinks.Console
    - Serilog.Sinks.Files
- in controllers, pass ILogger in constructor, `_logger.logInormation`
- in referenced classes, `Log.Information`

### version 002 - database scaffold DbContext and Entity classes
- add packages
    - Microsoft.EntityFrameworkCore
    - Microsoft.EntityFrameworkCore.Design
    - Microsoft.EntityFrameworkCore.SqlServer
- add connection string in appsettings.json
- generate DBContext class
    - parse tables
        - nullable column : int? DateTime?
        - dbTableName / EntityName; dbColumnName / PropertyName
    - OnModelCreating
        - base.OnModelCreating(modelBuilder)
        - for each table
            - modelBuilder.Entity<Author>(entity => {});
                - table mapping `entity.ToTable("Author");`
                - view `entity.ToView("AuthorRevenue");`
                - view `entity.HasNoKey();`
                - primary key: 
                    - column count > 1
                        - `entity.HasKey(e => new { e.AuthorId, e.BookId });`
                    - (column count = 1 and columnName != 'id' and columnName != tableName + 'id')
                        - `entity.HasKey(e => e.PubId);`
                - column mapping `entity.Property(e => e.AuthorId)`
                    - column name `.HasColumnName("author_id")`
                    - not null `.IsRequired()`
                    - data type 
                        - string maxLength `.HasMaxLength(40)`
                        - char (isfixedlength) `.IsFixedLength()`
                        - money `.HasColumnType("money")`
                        - datetime `.HasColumnType("datetime")`
                - foreignKey
                ```
                entity.HasOne(d => d.Pub)
                    .WithMany(p => p.Books)
                    .HasForeignKey(d => d.PubId)
                    .HasConstraintName("FK__Book__pub_id__6383C8BA");
                ```
## v003 - repository
- add package
    - `<PackageReference Include="Microsoft.EntityFrameworkCore.DynamicLinq" Version="3.2.5" />`

- add repository in startup.cs
```
// add repository
services.AddScoped<IBookStoreApiRepository, BookStoreApiRepository>();
```
- IRepository
    - general : add, delete, save, (update is not needed)
    ```
    Task AddAsync<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;
    // update is not needed in repository. Map from model class to entoty class, then saveAsync
    Task<bool> SaveAsync();
    ```
    - foreach table
    ```
    // foreach table
    Task<int> GetAllBooksCountAsync();

    Task<Book[]> GetAllBooksAsync(int pageNumber = 1, int rowsPerPages = 10, string sortBy = "BookId Desc");

    Task<bool> BookExistsAsync(int id) - pk(s)

    Task<Book> GetBookByPkAsnc(int id); - pk(s)

    // for each Unique Index 
        // Task<Book> GetBookByUixAsnc(string notes);

    // for each foreignkey as child
        // Task<int> GetBooksByFkCountAsync(int publisherId);

        // Task<Book[]> GetBooksByFkAsync(int publisherId, int pageNumber = 1, int rowsPerPages = 10, string sortBy = "BookId Desc");
    ```







- generate Entity classes
    - CamelCase column property

- add DBContext in startup.cs

### version 003 - model classes, automapper, build on top of version 003

- use DBContext in sample controller