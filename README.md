# NetCoreApiFromSql

### A tool to get your aspnet web api project started
    - From existing sql server database tables, generate asp.net core (3.1) apis
    - The generated project is a plain aspnet core webapi project. Modify / delete code / add code as needed.

### configuration
    - the items are self-explanatory
    - change the values of the folder paths, connectionString, and choose desired apiProjectName, dbContextName
    - if the database has many tables an views, use "includeTables" or "excludeTables"
        - if "includeTables" is not empty, then "excludeTables" is ignored
        - if both "includeTables" and "excludeTables", all tables in the database will be processed
    
  "apiProject" : {
    "templateFolder" : "/Volumes/macintosh-HD/git/NetCoreApiFromSql/CreateWebApiProj/ApiProjectTemplate",
    "apiProjectName": "BookStoreApi",
    "outputFolder" : "/Volumes/macintosh-HD/git/NetCoreApiFromSql/output",
    "DatabaseScaffold" : {
      "dbContextName": "BookStoreDBContext",
      "connectionString" : "Data Source=ITL-002;Initial Catalog=BookStore;User ID=sa;Password=********",
      "includeTables" : [],
      "excludeTables" : []
    },
    "packageVersions": [ ...]   // skipped here. see the appSettings.json file in the CreateWebApiProj project
  }

### From existing sql server database tables, generate asp.net core (3.1) apis
    - dbContext class
    - entity classes
        - one for each table
    - model classes
        - three for each table: api return model, forCreate and forUpdate
    - db operations repository interface and class
    - api controllers
        - [HttpGet]
            - GetAll                Route   api/Books
            - GetOneByPrimaryKey    Route   api/Books/{bookId}
            - GetOneByUniqueIndex   Route   api/Authors/{Nickname}
            - GetMultipleByFK       Route   api/publishers/{PubId}/Books
        - [HttpPost]
            - CreateNew             Route   api/books
        - [HttpPut]
            - Full update           Route   api/books/{bookId}
        - [HttpPatch]
            - Partial update        Route   api/books/{bookId}
        - [HttpDelete]
            - Delete               Route   api/books/{bookId}
    - FK will not be considered if 'ParentTable' or 'ReferencedTable' (sys.foreign_keys) is not included (config item "IncludeTables" "ExcludeTables")

### using the tool
- update values in appSettings.json
- dotnet run 
- it can be run again, it will delete the output project and generate again


