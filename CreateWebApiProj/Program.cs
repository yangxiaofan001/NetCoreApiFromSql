using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CreateWebApiProj.ADO;
using CreateWebApiProj.ConfigSections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CreateWebApiProj
{
    class Program
    {
        static ApiProjectSection apiProjectConfig;
        static Helper helper;

        static void Main(string[] args)
        {
            // appsettings.json must be in the bin folder, same folder as CreateWebApiProj.dll
            IServiceCollection services = new ServiceCollection();
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            IConfiguration configuration = builder.Build();

            apiProjectConfig = configuration.GetSection("ApiProject").Get<ApiProjectSection>();

            helper = new Helper();

            CreateWebApiProject(apiProjectConfig);
        }

        private static void CreateWebApiProject(ApiProjectSection apiProjectConfig)
        {
            string projRoot = apiProjectConfig.OutputFolder + "/" + apiProjectConfig.ApiProjectName;
            string versionsRoot = projRoot + "Versions";

            // v001 - dotnet new webapi + serilog - xcopy
            Console.WriteLine("v001 - Copying template");
            GenerateVersion001(projRoot, versionsRoot);

            // v002 - scaffold database: dbContext class, entity classes
            Console.WriteLine("v002 - dbContext class, entity classes");
            List<Table> tables = GenerateVersion002(projRoot, versionsRoot);

            // v003 - repository classes
            Console.WriteLine("v003- repository class");
            GenerateVersion003(projRoot, versionsRoot, tables);

            // v004 - model classes, automapper
            Console.WriteLine("v004 - model classes and autoMapper");
            GenerateVersion004(projRoot, versionsRoot, tables);

            // v005 - controller classes
            GenerateVersion005(projRoot, versionsRoot, tables);

            // // v006 - add api version support
        }

        private static void GenerateVersion001(string projRoot, string versionsRoot)
        {
            // Prepare Project Folders
            if (System.IO.Directory.Exists(projRoot))
            {
                System.IO.Directory.Delete(projRoot, recursive:true);
            }
            System.IO.Directory.CreateDirectory(projRoot);

            if (System.IO.Directory.Exists(versionsRoot))
            {
                System.IO.Directory.Delete(versionsRoot, recursive:true);
            }
            System.IO.Directory.CreateDirectory(versionsRoot);

            // v001 - dotnet new webapi + serilog - xcopy
            helper.xcopy(apiProjectConfig.TemplateFolder, projRoot, originalName:false, apiProjectConfig.ApiProjectName);
            helper.AddContentToFile(filePath: projRoot + "/Startup.cs",
                placeHolder: "//namespace",
                new List<string>{
                    "namespace " + apiProjectConfig.ApiProjectName
                }
            );
            helper.AddContentToFile(filePath: projRoot + "/Program.cs",
                placeHolder: "//namespace",
                new List<string>{
                    "namespace " + apiProjectConfig.ApiProjectName
                }
            );


            helper.SaveAsVersionX(projRoot, versionsRoot, 1, apiProjectConfig.ApiProjectName);
        }

        private static List<Table> GenerateVersion002(string projRoot, string versionsRoot)
        {
            // v002 scaffold database: dbContext class, entity classes
            
            int version = 2;
            string versionString = string.Format("{0,22:D3}", version).Trim();
            string versionFolder = versionsRoot + "/v" + versionString;

            // add packages
            Console.WriteLine("\tv002 - add packages");
            helper.AddPackages(projRoot, templateVersion : version.ToString(), apiProjectConfig.PackageVersions, apiProjectConfig.ApiProjectName);

            // add connection string in appsettings.json
            Console.WriteLine("\tv002 - add connectionString to appsettings.json");
            helper.AddContentToFile(filePath: projRoot + "/appsettings.json", 
                placeHolder: "\"AddMoreConfigItem\" : \"\"", 
                new List<string>
                {
                    "  , \"ConnectionStrings\": {",
                    "    \"" + apiProjectConfig.DatabaseScaffold.DbContextName +  "\":\"" + apiProjectConfig.DatabaseScaffold.ConnectionString + "\"",
                    "  }"
                }
            );

            Console.WriteLine("\tv002 - parse database tables: tables, columns, foreignKeys, PK and indicies");
            ADO.DbUserTableParser parser = new ADO.DbUserTableParser(new ApiProjectDbContext(apiProjectConfig.DatabaseScaffold.ConnectionString));
            List<ADO.Table> tables = parser.ParseTables(apiProjectConfig.DatabaseScaffold.IncludeTables, apiProjectConfig.DatabaseScaffold.ExcludeTables);

            // generate Entity classes
            Console.WriteLine("\tv002 - generate Entity classes");
            GenerateEntityClasses(projRoot, tables);

            // Generate DBContext class
            Console.WriteLine("\tv002 - generate dbContext class");
            GenerateDbContextClass(projRoot, tables);

            // add dbContext to StartUp.ConfigureServices
            Console.WriteLine("\tv002 - add dbContext to StartUp.ConfigureServices");
            helper.AddContentToFile(filePath: projRoot + "/Startup.cs",
                placeHolder: "//AddUsingStatement",
                new List<string>{
                    "using " + apiProjectConfig.ApiProjectName + ".Data;",
                    "using Microsoft.EntityFrameworkCore;"
                }
            );
            helper.AddContentToFile(filePath: projRoot + "/Startup.cs",
                placeHolder: "// add dbContext",
                new List<string>{
                    helper.indentSpaces(6) + "services.AddDbContext<" + apiProjectConfig.DatabaseScaffold.DbContextName + ">(options =>{",
                    helper.indentSpaces(7) + "options.UseSqlServer(_configuration.GetConnectionString(\"" + apiProjectConfig.DatabaseScaffold.DbContextName + "\"));",
                    helper.indentSpaces(6) + "});",
                    helper.indentSpaces(6) + ""
                }
            );
            
            Console.WriteLine("\tv002 - save project as v002");
            helper.SaveAsVersionX(projRoot, versionsRoot, 2, apiProjectConfig.ApiProjectName);
            return tables;
        }

        private static void GenerateVersion003(string projRoot, string versionsRoot, List<Table> tables)
        {
            Console.WriteLine("v003 - generate repository class");
            helper.AddPackages(projRoot, "3", apiProjectConfig.PackageVersions, apiProjectConfig.ApiProjectName);

            helper.GenerateClassFromTemplace(
                apiProjectConfig.TemplateFolder + "/Data/Constants.cs.templ_",
                projRoot + "/Data/Constants.cs",
                new List<TemplateVariable> { new TemplateVariable{ Name = "$$ApiProjectName$$", Value = apiProjectConfig.ApiProjectName}}
            );
            System.IO.File.Delete(projRoot + "/Data/Constants.cs.templ_");

            helper.GenerateClassFromTemplace(
                apiProjectConfig.TemplateFolder + "/Data/EntityCollection.cs.templ_",
                projRoot + "/Data/EntityCollection.cs",
                new List<TemplateVariable> { new TemplateVariable{ Name = "$$ApiProjectName$$", Value = apiProjectConfig.ApiProjectName}}
            );
            System.IO.File.Delete(projRoot + "/Data/EntityCollection.cs.templ_");
            
            GenerateRepositoryClass(projRoot, tables);

            // add repository in startup.cs
            Console.WriteLine("v003 - add repository in startup.cs");
            string repositoryInterfaceName = "I" + apiProjectConfig.ApiProjectName + "Repository";
            string repositoryClassName = apiProjectConfig.ApiProjectName + "Repository";
            helper.AddContentToFile(filePath : projRoot + "/StartUp.cs",
                placeHolder : "// add repository",
                new List<string> { "services.AddScoped<" + repositoryInterfaceName + ", " + repositoryClassName + ">();" }
            );

            helper.SaveAsVersionX(projRoot, versionsRoot, 3, apiProjectConfig.ApiProjectName);
        }

        private static void GenerateVersion004(string projRoot, string versionsRoot, List<Table> tables)
        {
            // add package
            Console.WriteLine("v004 - add packages");
            helper.AddPackages(projRoot, "4", apiProjectConfig.PackageVersions, apiProjectConfig.ApiProjectName);

            helper.GenerateClassFromTemplace(
                apiProjectConfig.TemplateFolder + "/Data/ModelObjectCollection.cs.templ_",
                projRoot + "/Data/ModelObjectCollection.cs",
                new List<TemplateVariable> { new TemplateVariable { Name = "$$ApiProjectName$$", Value = apiProjectConfig.ApiProjectName}}
            );
            System.IO.File.Delete(projRoot  + "/Data/ModelObjectCollection.cs.templ_");

            // v003 - model classes, automapper
            Console.WriteLine("v003 - generate model classes");
            GenerateModelClasses(projRoot, tables);

            // automapper profile
            Console.WriteLine("v003 - generate auto mapper profile class");
            GenerateMapperProfile(projRoot, tables);

            // add automapper in startup.cs
            Console.WriteLine("v003 - add automapper in startup.cs");
            helper.AddContentToFile(filePath : projRoot + "/StartUp.cs",
                placeHolder : "//AddUsingStatement",
                new List<string> { "using AutoMapper;" }
            );

            helper.AddContentToFile(filePath : projRoot + "/StartUp.cs",
                placeHolder : "// add automapper",
                new List<string> { 
                    "            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());" 
                }
            );

            Console.WriteLine("v004 - save project as v004");
            helper.SaveAsVersionX(projRoot, versionsRoot, 4, apiProjectConfig.ApiProjectName);
        }

        private static void GenerateVersion005(string projRoot, string versionsRoot, List<Table> tables)
        {
            string controllersFolder = projRoot + "/Controllers";
            System.IO.File.Delete(controllersFolder + "/TestController.cs");
            if (!System.IO.Directory.Exists(controllersFolder))
            {
                System.IO.Directory.CreateDirectory(controllersFolder);
            }

            helper.AddPackages(projRoot, "5", apiProjectConfig.PackageVersions, apiProjectConfig.ApiProjectName);

            foreach(Table table in tables)
            {
                System.IO.StreamWriter sw = new StreamWriter(controllersFolder + "/" + table.PlurizedEntityName + "Controller.cs", false);
                sw.Close();

                int indent = 0;
                sw = new StreamWriter(controllersFolder + "/" + table.PlurizedEntityName + "Controller.cs", true);
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Linq;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Linq.Dynamic.Core.Exceptions;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Threading.Tasks;");
                sw.WriteLine(helper.indentSpaces(indent) + "using AutoMapper;");
                sw.WriteLine(helper.indentSpaces(indent) + "using " + apiProjectConfig.ApiProjectName + ".Data;");
                sw.WriteLine(helper.indentSpaces(indent) + "using Microsoft.AspNetCore.Http;");
                sw.WriteLine(helper.indentSpaces(indent) + "using " + apiProjectConfig.ApiProjectName + ".Data.Entities;");
                sw.WriteLine(helper.indentSpaces(indent) + "using Microsoft.AspNetCore.Mvc;");
                sw.WriteLine(helper.indentSpaces(indent) + "using Microsoft.Extensions.Logging;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System;");
                sw.WriteLine(helper.indentSpaces(indent) + "using Microsoft.AspNetCore.Routing;");
                sw.WriteLine(helper.indentSpaces(indent) + "using Microsoft.AspNetCore.JsonPatch;");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Controllers");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "[ApiController]");
                sw.WriteLine(helper.indentSpaces(indent) + "public class " + table.PlurizedEntityName + "Controller : ControllerBase");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "private readonly I" + apiProjectConfig.ApiProjectName + "Repository _repository;");
                sw.WriteLine(helper.indentSpaces(indent) + "private readonly IMapper _mapper;");
                sw.WriteLine(helper.indentSpaces(indent) + "private readonly ILogger<" + table.PlurizedEntityName + "Controller> _logger;");
                sw.WriteLine(helper.indentSpaces(indent) + "private readonly LinkGenerator _linkgenerator;");

                // ctor
                sw.WriteLine(helper.indentSpaces(indent) + "public " + table.PlurizedEntityName + "Controller(I" + apiProjectConfig.ApiProjectName + "Repository repository, IMapper mapper, ILogger<" + table.PlurizedEntityName + "Controller> logger, LinkGenerator linkgenerator)");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "_repository = repository;");
                sw.WriteLine(helper.indentSpaces(indent) + "_mapper = mapper;");
                sw.WriteLine(helper.indentSpaces(indent) + "_logger = logger;");
                sw.WriteLine(helper.indentSpaces(indent) + "_linkgenerator = linkgenerator;");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.WriteLine(helper.indentSpaces(indent) + "");

                #region getAll
                sw.WriteLine(helper.indentSpaces(indent) + "// GetAll" + table.PlurizedEntityName);
                sw.WriteLine(helper.indentSpaces(indent) + "[HttpGet]");
                sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + table.PlurizedEntityName + "\")]");
                sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<" + table.EntityName + "[]>> "
                    + "GetAll" + table.PlurizedEntityName 
                    + "(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, "
                    + "string sortBy = \"" + table.DefaultSortBy + "\")");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "EntityCollection<" + table.EntityName + "> db" + table.PlurizedEntityName  + "= null;");
                sw.WriteLine(helper.indentSpaces(indent) + "try");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "db" + table.PlurizedEntityName + "= await _repository.GetAll" + table.PlurizedEntityName + "Async(pageNumber, pageSize, sortBy);");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.WriteLine(helper.indentSpaces(indent) + "catch (ParseException ex)");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "return BadRequest(\"Request format is invalid: \" + ex.Message);");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.WriteLine(helper.indentSpaces(indent) + "catch(Exception ex)");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "return StatusCode(StatusCodes.Status500InternalServerError, ex);");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "if (db" + table.PlurizedEntityName + " == null)");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.WriteLine(helper.indentSpaces(indent) + "");

                sw.WriteLine(helper.indentSpaces(indent) + "Data.ModelObjectCollection<Data.Models." + table.EntityName 
                    + "> " + helper.Pluralize(table.LoweredEntityName)
                    + " = new ModelObjectCollection<Data.Models." + table.EntityName + ">");

                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "TotalCount = db" + table.PlurizedEntityName + ".TotalCount,");
                sw.WriteLine(helper.indentSpaces(indent) + "PageNumber = db" + table.PlurizedEntityName + ".PageNumber,");
                sw.WriteLine(helper.indentSpaces(indent) + "PageSize = db" + table.PlurizedEntityName + ".PageSize,");
                sw.WriteLine(helper.indentSpaces(indent) + "TotalPages = db" + table.PlurizedEntityName + ".TotalPages,");
                sw.WriteLine(helper.indentSpaces(indent) + "SortBy = db" + table.PlurizedEntityName + ".SortBy,");
                sw.WriteLine(helper.indentSpaces(indent) + "NextPageNumber = db" + table.PlurizedEntityName + ".NextPageNumber,");
                sw.WriteLine(helper.indentSpaces(indent) + "PrevPageNumber = db" + table.PlurizedEntityName + ".PrevPageNumber,");
                sw.WriteLine(helper.indentSpaces(indent) + "NextPageUrl = \"\",");
                sw.WriteLine(helper.indentSpaces(indent) + "PrevPageUrl = \"\",");
                sw.WriteLine(helper.indentSpaces(indent) + "Data = _mapper.Map<Data.Models." + table.EntityName + " []>(db" + table.PlurizedEntityName + ".Data)");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "};");
                sw.WriteLine(helper.indentSpaces(indent) + "");

                string modelVarName = helper.Pluralize(table.PlurizedEntityName);

                sw.WriteLine(helper.indentSpaces(indent) + modelVarName + ".NextPageUrl = (" 
                    + modelVarName + ".PageNumber == " + modelVarName + ".TotalPages) ? \"\" : "
                    + "(\"api/" + modelVarName + "?pageNumber\" + " + modelVarName + ".NextPageNumber.ToString())");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "+\"&pageSize=\" + " + modelVarName + ".PageSize.ToString()");
                sw.WriteLine(helper.indentSpaces(indent) + "+\"&sortBy=\" + " + modelVarName + ".SortBy;");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + modelVarName + ".PrevPageUrl = ("
                    + modelVarName + ".PageNumber == 1) ? \"\" : "
                    + "(\"api/" + modelVarName + "?pageNumber\" + " + modelVarName + ".PrevPageNumber.ToString())");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "+\"&pageSize=\" + " + modelVarName + ".PageSize.ToString()");
                sw.WriteLine(helper.indentSpaces(indent) + "+\"&sortBy=\" + " + modelVarName + ".SortBy;");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "return Ok(" + modelVarName + ");");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                #endregion

                // Get one by primary key
                if (table.PrimaryKey != null)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "// GetByPk");
                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpGet]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + table.PlurizedEntityName + "/" + table.PrimaryKey.GetByUixApiRoute + "\")]");
                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<" + table.EntityName + ">> "
                        + "Get" + table.EntityName + "By" + table.PrimaryKey.IndexColumnNames + "Async(" + table.PrimaryKey.ParamList + ")");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) +  table.EntityName + " db" + table.EntityName + " = await _repository.Get" + table.EntityName + "Async(" + table.PrimaryKey.ParamValueList + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (db" + table.EntityName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "return Ok(_mapper.Map<Data.Models." + table.EntityName + ">(db" + table.EntityName + "));");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                }

                // Get one by unique index
                foreach(TableIndex uix in table.UniqueIndecies)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "// GetByUniqueIndex " + uix.Name);
                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpGet]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + table.PlurizedEntityName + "/" + uix.GetByUixApiRoute + "\")]");
                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<" + table.EntityName + ">> Get" + table.EntityName + "By" + uix.IndexColumnNames + "Async(" + uix.ParamList + ")");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + table.EntityName + " db" + table.EntityName + " = await _repository.Get" + table.EntityName + "By" + uix.IndexColumnNames + "Async(" + uix.ParamValueList + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (db" + table.EntityName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "return Ok(_mapper.Map<Data.Models." + table.EntityName + ">(db" + table.EntityName + "));");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                }

                #region get multiple by fk
                foreach(ForeignKey fk in table.ForeignKeysAsChild)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "// GetByFk, where current entity is child");
                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpGet]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"" + fk.GetByFkColumnApiRoute + "\")]");
                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<" + table.EntityName + "[]>> "
                        + "Get" + table.PlurizedEntityName + "By" + fk.ParentTableColumn.PropertyName
                        + "Async(" + fk.ParentTableColumn.EntityDataType + " " + fk.ParentTableColumn.loweredPropertyName 
                        + ", int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = \"" + table.DefaultSortBy + "\")");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "EntityCollection<" + table.EntityName + "> db" +table.PlurizedEntityName + " = null;");
                    sw.WriteLine(helper.indentSpaces(indent) + "try");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "db" + table.PlurizedEntityName + " = await _repository."
                        + "Get" + table.PlurizedEntityName + "By" + fk.ParentTableColumn.PropertyName
                        + "Async(" + fk.ParentTableColumn.loweredPropertyName 
                        + ", pageNumber, pageSize, sortBy);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "catch (ParseException ex)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return BadRequest(\"Request format is invalid: \" + ex.Message);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "catch(Exception ex)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return StatusCode(StatusCodes.Status500InternalServerError, ex);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (db" + table.PlurizedEntityName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.ModelObjectCollection<Data.Models." + table.EntityName 
                    + "> " + helper.Pluralize(table.LoweredEntityName)
                    + " = new ModelObjectCollection<Data.Models." + table.EntityName + ">");

                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "TotalCount = db" + table.PlurizedEntityName + ".TotalCount,");
                    sw.WriteLine(helper.indentSpaces(indent) + "PageNumber = db" + table.PlurizedEntityName + ".PageNumber,");
                    sw.WriteLine(helper.indentSpaces(indent) + "PageSize = db" + table.PlurizedEntityName + ".PageSize,");
                    sw.WriteLine(helper.indentSpaces(indent) + "TotalPages = db" + table.PlurizedEntityName + ".TotalPages,");
                    sw.WriteLine(helper.indentSpaces(indent) + "SortBy = db" + table.PlurizedEntityName + ".SortBy,");
                    sw.WriteLine(helper.indentSpaces(indent) + "NextPageNumber = db" + table.PlurizedEntityName + ".NextPageNumber,");
                    sw.WriteLine(helper.indentSpaces(indent) + "PrevPageNumber = db" + table.PlurizedEntityName + ".PrevPageNumber,");
                    sw.WriteLine(helper.indentSpaces(indent) + "NextPageUrl = \"\",");
                    sw.WriteLine(helper.indentSpaces(indent) + "PrevPageUrl = \"\",");
                    sw.WriteLine(helper.indentSpaces(indent) + "Data = _mapper.Map<Data.Models." + table.EntityName + " []>(db" + table.PlurizedEntityName + ".Data)");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "};");
                    sw.WriteLine(helper.indentSpaces(indent) + "");

                    modelVarName = helper.Pluralize(table.PlurizedEntityName);

                    sw.WriteLine(helper.indentSpaces(indent) + modelVarName + ".NextPageUrl = (" 
                        + modelVarName + ".PageNumber == " + modelVarName + ".TotalPages) ? \"\" : "
                        + "(\"api/" + modelVarName + "?pageNumber\" + " + modelVarName + ".NextPageNumber.ToString())");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "+ \"&pageSize=\" + " + modelVarName + ".PageSize.ToString()");
                    sw.WriteLine(helper.indentSpaces(indent) + "+ \"&sortBy=\" + " + modelVarName + ".SortBy;");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + modelVarName + ".PrevPageUrl = ("
                        + modelVarName + ".PageNumber == 1) ? \"\" : "
                        + "(\"api/" + modelVarName + "?pageNumber\" + " + modelVarName + ".PrevPageNumber.ToString())");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "+ \"&pageSize=\" + " + modelVarName + ".PageSize.ToString()");
                    sw.WriteLine(helper.indentSpaces(indent) + "+ \"&sortBy=\" + " + modelVarName + ".SortBy;");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "return Ok(" + modelVarName + ");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                }
                #endregion

                #region HttpPost create new
                if (table.PrimaryKey != null)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "// HttpPost create new");

                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpPost]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + helper.Pluralize(table.LoweredEntityName) + "\")]");
                    string newModelVarName = "new" + table.EntityName;
                    string newEntityVarName = "dbNew" + table.EntityName;

                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<Data.Models." + table.EntityName + ">> CreateNew" + table.EntityName + "(Data.Models." + table.EntityName + "ForCreate " + newModelVarName + ")");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Entities." + table.EntityName + " " + newEntityVarName + " = null;");
                    sw.WriteLine(helper.indentSpaces(indent) + "try");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + newEntityVarName + " = _mapper.Map<Data.Entities." + table.EntityName + ">(" + newModelVarName + ");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "catch(Exception ex)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return BadRequest(\"Input is in invalid format: \" + ex.Message);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (" + newEntityVarName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return BadRequest(\"Input is in invalid format\");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "await _repository.AddAsync<Data.Entities." + table.EntityName + ">(" + newEntityVarName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "await _repository.SaveChangesAsync();");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Models." + table.EntityName + " added" + table.EntityName + " = _mapper.Map<Data.Models." + table.EntityName + ">(" + newEntityVarName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "");

                    sw.WriteLine(helper.indentSpaces(indent) + "var url = _linkgenerator.GetPathByAction(HttpContext, " 
                        + "\"Get" + table.EntityName + "By" + table.PrimaryKey.IndexColumnNames
                        + "\", \"" + table.PlurizedEntityName + "\", " + " added" + table.EntityName  + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "return this.Created(url, "  + "added" + table.EntityName + ");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                }
                #endregion

                #region HttpPut full update
                if (table.PrimaryKey != null)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "// HttpPut full update");
                    string updatedModelVarName = "updated" + table.EntityName;
                    string entityVarName = "db" + table.EntityName;
                    string savedModelVarName = "saved" + table.EntityName;

                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpPut]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + table.PlurizedEntityName + "/" + table.PrimaryKey.GetByUixApiRoute + "\")]");
                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<Data.Models." + table.EntityName + ">> Update" + table.EntityName 
                        + "(" + table.PrimaryKey.ParamList + ", Data.Models." +  table.EntityName + "ForUpdate " + updatedModelVarName + ")");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "try");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Entities." + table.EntityName + " " + entityVarName + " = await _repository.Get" + table.EntityName + "Async(" + table.PrimaryKey.ParamValueList + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (" + entityVarName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "_mapper.Map(" + updatedModelVarName + ", " + entityVarName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (await _repository.SaveChangesAsync())");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Models." + table.EntityName + " " + savedModelVarName + " = _mapper.Map<Data.Models." + table.EntityName + ">(" + entityVarName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "return Ok(" + savedModelVarName + ");            ");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "else");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return BadRequest(\"Failed to update.\");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "catch(Exception ex)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return StatusCode(StatusCodes.Status500InternalServerError, \"Database exception: \" + ex.Message);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                }  
                #endregion

                #region HttpPatch partial update
                if (table.PrimaryKey != null)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "// HttpPatch partial update");
                    string updatedModelVarName = "updated" + table.EntityName;
                    string entityVarName = "db" + table.EntityName;
                    string savedModelVarName = "saved" + table.EntityName;
                    // start
                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpPatch]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + table.PlurizedEntityName + "/" + table.PrimaryKey.GetByUixApiRoute + "\")]");
                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<ActionResult<Data.Models." + table.EntityName + ">> Patch" + table.EntityName 
                    + "(" + table.PrimaryKey.ParamList + ", JsonPatchDocument<Data.Models." + table.EntityName + "ForUpdate> patchDocument)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "try");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Entities." + table.EntityName + " " + entityVarName + " = await _repository.Get" +table.EntityName + "Async(" + table.PrimaryKey.ParamValueList + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (" + entityVarName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                    indent--;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "var " + updatedModelVarName + " = _mapper.Map<Data.Models." + table.EntityName + "ForUpdate>(" + entityVarName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "patchDocument.ApplyTo(" + updatedModelVarName + ", ModelState);");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "_mapper.Map(" + updatedModelVarName + ", " + entityVarName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (await _repository.SaveChangesAsync())");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Models." + table.EntityName + " " + savedModelVarName + " = _mapper.Map<Data.Models." + table.EntityName + ">(await _repository.Get" + table.EntityName + "Async(" + table.PrimaryKey.ParamValueList + "));");
                    sw.WriteLine(helper.indentSpaces(indent) + "return Ok(" + savedModelVarName + ");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "else");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return StatusCode(StatusCodes.Status500InternalServerError, \"Unable to save to database\");");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "catch(Exception ex)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return StatusCode(StatusCodes.Status500InternalServerError, \"Unable to patch " + table.LoweredEntityName + " \" + ex.Message);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                }
                #endregion

                #region HttpDelete delete
                if (table.PrimaryKey != null)
                {
                    // start
                    sw.WriteLine(helper.indentSpaces(indent) + "[HttpDelete]");
                    sw.WriteLine(helper.indentSpaces(indent) + "[Route(\"api/" + table.PlurizedEntityName + "/" + table.PrimaryKey.GetByUixApiRoute + "\")]");
                    sw.WriteLine(helper.indentSpaces(indent) + "public async Task<IActionResult> Delete" + table.EntityName + "(" + table.PrimaryKey.ParamList + ")");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "try");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "Data.Entities." + table.EntityName + " db" + table.EntityName + " = await _repository.Get" + table.EntityName + "Async(" + table.PrimaryKey.ParamValueList + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "if (db" + table.EntityName + " == null)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return NotFound();");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "_repository.Delete<Data.Entities." + table.EntityName + ">(db" + table.EntityName + ");");
                    sw.WriteLine(helper.indentSpaces(indent) + "await _repository.SaveChangesAsync();");
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "return NoContent();");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    sw.WriteLine(helper.indentSpaces(indent) + "catch(Exception ex)");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + "return StatusCode(StatusCodes.Status500InternalServerError, \"Database exception: \" + ex.Message);");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                    // end
                }
                #endregion





                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.Close();
            }



            helper.SaveAsVersionX(projRoot, versionsRoot, 5, apiProjectConfig.ApiProjectName);
        }
#region v002 methods - GenerateDbContextClass, GenerateEntityClasses
        static void GenerateDbContextClass(string projRoot, List<Table> tables)
        {
            string dbContextFile = projRoot + "/Data/" + apiProjectConfig.DatabaseScaffold.DbContextName + ".cs";
            System.IO.StreamWriter sw = new StreamWriter(dbContextFile, false);
            sw.Close();

            sw = new StreamWriter(dbContextFile, true);
            int indent = 0;
            sw.WriteLine(helper.indentSpaces(indent) + "using Microsoft.EntityFrameworkCore;");
            sw.WriteLine(helper.indentSpaces(indent) + "using " + apiProjectConfig.ApiProjectName + ".Data.Entities;");
            sw.WriteLine(helper.indentSpaces(indent) + "");
            sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Data");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "public partial class " + apiProjectConfig.DatabaseScaffold.DbContextName + " : DbContext" );
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "public " + apiProjectConfig.DatabaseScaffold.DbContextName + "()");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            sw.WriteLine(helper.indentSpaces(indent) + "public " + apiProjectConfig.DatabaseScaffold.DbContextName + "(DbContextOptions<" + apiProjectConfig.DatabaseScaffold.DbContextName + "> options) : base(options)");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            sw.WriteLine(helper.indentSpaces(indent) + "}");

            foreach(Table table in tables.OrderBy(t => t.Name))
            {
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "public virtual DbSet<" + table.EntityName + "> " + helper.Pluralize(table.EntityName) + " { get; set; }");
            }

            sw.WriteLine(helper.indentSpaces(indent) + "protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "if (!optionsBuilder.IsConfigured)");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "optionsBuilder.UseSqlServer(\"Name=" + apiProjectConfig.DatabaseScaffold.DbContextName + "\");");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            sw.WriteLine(helper.indentSpaces(indent) + "");
            sw.WriteLine(helper.indentSpaces(indent) + "protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "base.OnModelCreating(modelBuilder);");
            
            sw.WriteLine(helper.indentSpaces(indent) + "// skip fluent validation rules for now");
            sw.WriteLine(helper.indentSpaces(indent) + "// wrong, that's not fluent validation, it is mapping for table, view, column, pk, fk");

            foreach(Table table in tables.OrderBy(t => t.Name))
            {
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "modelBuilder.Entity<" + table.EntityName + ">(entity => {");

                // table/view name mapping
                indent ++;
                if (table.ObjectType.ToLower() == "u")
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "entity.ToTable(\"" + table.EntityName + "\");");
                }
                else if (table.ObjectType.ToLower() == "v")
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "entity.ToView(\"" + table.EntityName + "\");");
                    sw.WriteLine(helper.indentSpaces(indent) + "entity.HasNoKey();");
                }

                // primary key mapping
                if (table.PrimaryKey != null)
                {
                    string propertyNameList = "";
                    foreach(Column c in table.PrimaryKey.IndexColumns.Select(ic => ic.Column))
                    {
                        propertyNameList = propertyNameList + "e." + c.PropertyName + ", ";
                    }
                    propertyNameList = propertyNameList.Substring(0, propertyNameList.Length - 2);

                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "entity.HasKey(e => new { " + propertyNameList + " });");
                }

                // column mappings
                foreach(Column column in table.Columns.OrderBy(c => c.Name))
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "");
                    sw.WriteLine(helper.indentSpaces(indent) + "entity.Property(e => e." + column.PropertyName + ")");
                    indent ++;
                    sw.WriteLine(helper.indentSpaces(indent) + ".HasColumnName(\"" +column.Name + "\")");
                    if (column.Required)
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + ".IsRequired()");
                    }

                    if (column.EntityDataType == "string")
                    {
                        if (column.MaxLength > 0)
                        {
                            sw.WriteLine(helper.indentSpaces(indent) + ".HasMaxLength(" + column.MaxLength.ToString() + ")");
                        }
                    }
                    else if (column.DataType == "money")
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + ".HasColumnType(\"money\")");
                    }
                    else if (column.DataType.ToLower() == "datetime")
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + ".HasColumnType(\"datetime\")");
                    }

                    if (column.FixedLength)
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + ".IsFixedLength()");
                    }

                    sw.WriteLine(helper.indentSpaces(indent) + ";");

                    indent --;
                }

                // foreign keys mapping
                foreach(ForeignKey fk in table.ForeignKeysAsChild.OrderBy(f => f.Name))
                {
                    string dependantEntityName = table.EntityName;
                    string dependantPropertyName = table.Columns.FirstOrDefault(c => c.ColumnId == fk.ParentTableColumnId).PropertyName;
                    string parentEntityName = tables.FirstOrDefault(t => t.ObjectId == fk.ReferencedTableObjectId).EntityName;
                    string fkName = fk.Name;

                    sw.WriteLine(helper.indentSpaces(indent) + "");

                    if (table.ForeignKeysAsChild.Where(ff => ff.ReferencedTableObjectId == fk.ReferencedTableObjectId).Count() == 1)
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + "entity.HasOne(d => d." + parentEntityName + ")");
                        indent ++;
                        sw.WriteLine(helper.indentSpaces(indent) + ".WithMany(p => p." + helper.Pluralize(dependantEntityName) + ")");
                    }
                    else
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + "entity.HasOne(d => d." + parentEntityName + "_" + fk.Name + ")");
                        indent ++;
                        sw.WriteLine(helper.indentSpaces(indent) + ".WithMany(p => p." + helper.Pluralize(dependantEntityName) + "_" + fk.Name + ")");
                    }
                    
                    sw.WriteLine(helper.indentSpaces(indent) + ".HasForeignKey(d => d." + dependantPropertyName + ")");
                    sw.WriteLine(helper.indentSpaces(indent) + ".HasConstraintName(\"" + fkName + "\");");
                    indent --;
                }

                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "});");
            }

            sw.WriteLine(helper.indentSpaces(indent) + "");
            sw.WriteLine(helper.indentSpaces(indent) + "OnModelCreatingPartial(modelBuilder);");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            sw.WriteLine(helper.indentSpaces(indent) + "");
            sw.WriteLine(helper.indentSpaces(indent) + "partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
            sw.WriteLine(helper.indentSpaces(indent) + "");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            sw.Close();
        }
        static void GenerateEntityClasses(string projRoot, List<Table> tables)
        {
            // prepare entity classes folder
            string projectDataFolder = projRoot + "/Data";
            if (!System.IO.Directory.Exists(projectDataFolder))
            {
                System.IO.Directory.CreateDirectory(projectDataFolder);
            }

            string entityClassesFolder = projRoot + "/Data/Entities";
            if (System.IO.Directory.Exists(entityClassesFolder))
            {
                System.IO.Directory.Delete(entityClassesFolder, recursive:true);
            }
            System.IO.Directory.CreateDirectory(entityClassesFolder);

            // create entity classes
            foreach(Table table in tables)
            {
                string tableClassFile = entityClassesFolder + "/" + table.EntityName + ".cs";

                System.IO.StreamWriter sw = new StreamWriter(tableClassFile, false);
                sw.Close();

                int indent = 0;

                sw = new StreamWriter(tableClassFile, true);
                sw.WriteLine(helper.indentSpaces(indent) + "using System;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Collections.Generic;");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Data.Entities");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "public partial class " + table.EntityName);
                sw.WriteLine(helper.indentSpaces(indent) + "{");

                indent ++;
                List<long> parentTableObjectIdList = table.ForeignKeysAsParent.Select(fk => fk.ParentTableObjectId).Distinct().ToList();

                if (table.ForeignKeysAsParent.Count > 0)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "public " + table.EntityName + "()");
                    sw.WriteLine(helper.indentSpaces(indent) + "{");
                    indent ++;

                    foreach(long parentTableObjectId in parentTableObjectIdList)
                    {
                        List<ForeignKey> fks = table.ForeignKeysAsParent.Where(fk => fk.ParentTableObjectId == parentTableObjectId).ToList();

                        if (fks.Count == 1)
                        {
                            ForeignKey fk = fks[0];
                            string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;
                            sw.WriteLine(helper.indentSpaces(indent) + helper.Pluralize(childTableName)  + " = new HashSet<" + childTableName + ">();");
                        }
                        else
                        {
                            foreach(ForeignKey fk in fks)
                            {
                                string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;
                                sw.WriteLine(helper.indentSpaces(indent) + helper.Pluralize(childTableName) + "_" + fk.Name + " = new HashSet<" + childTableName + ">();");
                            }
                        }
                    }

                    indent --;
                    sw.WriteLine(helper.indentSpaces(indent) + "}");
                }

                foreach(Column column in table.Columns)
                {
                    sw.WriteLine(helper.indentSpaces(indent) + "public " + column.EntityDataType + " " + column.PropertyName + " { get; set; }");
                }





                List<long> referencedTableObjectIdList = table.ForeignKeysAsChild.Select(fk => fk.ReferencedTableObjectId).Distinct().ToList();
                //foreach(ForeignKey fk in table.ForeignKeysAsChild)
                foreach(long referencedTableObjectId in referencedTableObjectIdList)
                {
                    List<ForeignKey> fks = table.ForeignKeysAsChild.Where(fk => fk.ReferencedTableObjectId == referencedTableObjectId).ToList();

                    if (fks.Count == 1)
                    {
                        ForeignKey fk = fks[0];
                        string parentTableName = tables.FirstOrDefault(t => fk.ReferencedTableObjectId == t.ObjectId).EntityName;
                        sw.WriteLine(helper.indentSpaces(indent) + "public virtual " + parentTableName + " " + parentTableName + " { get; set; }");
                    }
                    else
                    {
                        foreach(ForeignKey fk in fks)
                        {
                            string parentTableName = tables.FirstOrDefault(t => fk.ReferencedTableObjectId == t.ObjectId).EntityName;
                            sw.WriteLine(helper.indentSpaces(indent) + "public virtual " + parentTableName + " " + parentTableName + "_" + fk.Name + " { get; set; }");
                        }
                    }
                }
                foreach(long parentTableObjectId in parentTableObjectIdList)
                {
                    List<ForeignKey> fks = table.ForeignKeysAsParent.Where(fk => fk.ParentTableObjectId == parentTableObjectId).ToList();

                    if (fks.Count == 1)
                    {
                        ForeignKey fk = fks[0];
                        string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;
                        sw.WriteLine(helper.indentSpaces(indent) + "public virtual ICollection<" + childTableName + "> " + helper.Pluralize(childTableName) + " { get; set; }");
                    }
                    else
                    {
                        foreach(ForeignKey fk in fks)
                        {
                            
                            string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;
                            sw.WriteLine(helper.indentSpaces(indent) + "public virtual ICollection<" + childTableName + "> " + helper.Pluralize(childTableName) + "_" + fk.Name + " { get; set; }");
                        }
                    }
                }

                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");

                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");

                sw.Close();
            }
        }
#endregion

#region v003 methods - GenerateRepositoryClass
        private static void GenerateRepositoryClass(string projRoot, List<Table> tables)
        {
            string repositoryClassName = apiProjectConfig.ApiProjectName + "Repository";
            string repositoryInterfaceName = "I" + apiProjectConfig.ApiProjectName + "Repository";
            string repositoryClassFilePath = projRoot + "/Data/" + repositoryClassName + ".cs";
            string repositoryInterfaceFilePath = projRoot + "/Data/" + repositoryInterfaceName + ".cs";

            // interface
            System.IO.StreamWriter swInterface = new StreamWriter(repositoryInterfaceFilePath, false);
            swInterface.Close();
            System.IO.StreamWriter swClass = new StreamWriter(repositoryClassFilePath, false);
            swClass.Close();

            int ii = 0;
            int ic = 0;

            swInterface = new StreamWriter(repositoryInterfaceFilePath, true);
            swClass = new StreamWriter(repositoryClassFilePath, true);

            // using statements
            swInterface.WriteLine(helper.indentSpaces(ii) + "using System;");
            swInterface.WriteLine(helper.indentSpaces(ii) + "using System.Threading.Tasks;");
            swInterface.WriteLine(helper.indentSpaces(ii) + "using " + apiProjectConfig.ApiProjectName + ".Data.Entities;");
            swInterface.WriteLine(helper.indentSpaces(ii) + "");

            swClass.WriteLine(helper.indentSpaces(ic) + "using System;");
            swClass.WriteLine(helper.indentSpaces(ic) + "using System.Linq;");
            swClass.WriteLine(helper.indentSpaces(ic) + "using System.Threading.Tasks;");
            swClass.WriteLine(helper.indentSpaces(ic) + "using " + apiProjectConfig.ApiProjectName + ".Data.Entities;");
            swClass.WriteLine(helper.indentSpaces(ic) + "using Microsoft.EntityFrameworkCore;");
            swClass.WriteLine(helper.indentSpaces(ic) + "using Microsoft.Extensions.Logging;");
            swClass.WriteLine(helper.indentSpaces(ic) + "using System.Linq.Dynamic.Core;");
            swClass.WriteLine(helper.indentSpaces(ic) + "");

            // interface / class opening
            swInterface.WriteLine(helper.indentSpaces(ii) + "namespace " + apiProjectConfig.ApiProjectName + ".Data");
            swInterface.WriteLine(helper.indentSpaces(ii) + "{");
            ii ++;
            swInterface.WriteLine(helper.indentSpaces(ii) + "public interface " + repositoryInterfaceName);
            swInterface.WriteLine(helper.indentSpaces(ii) + "{");

            swClass.WriteLine(helper.indentSpaces(ic) + "namespace " + apiProjectConfig.ApiProjectName + ".Data");
            swClass.WriteLine(helper.indentSpaces(ic) + "{");
            ic ++;
            swClass.WriteLine(helper.indentSpaces(ic) + "public class " + repositoryClassName + " : " + repositoryInterfaceName);
            swClass.WriteLine(helper.indentSpaces(ic) + "{");

            // general methods - add, delete, save; class constructor
            ii ++;
            swInterface.WriteLine(helper.indentSpaces(ii) + "// general");
            swInterface.WriteLine(helper.indentSpaces(ii) + "Task AddAsync<T>(T entity) where T : class;");
            swInterface.WriteLine(helper.indentSpaces(ii) + "");
            swInterface.WriteLine(helper.indentSpaces(ii) + "void Delete<T>(T entity) where T : class;");
            swInterface.WriteLine(helper.indentSpaces(ii) + "");
            swInterface.WriteLine(helper.indentSpaces(ii) + "Task<bool> SaveChangesAsync();");
            swInterface.WriteLine(helper.indentSpaces(ii) + "");

            ic ++;
            swClass.WriteLine(helper.indentSpaces(ic) + "private readonly int maxPageSize = 20;");
            swClass.WriteLine(helper.indentSpaces(ic) + "private readonly int defaultPageSize = 20;");
            swClass.WriteLine(helper.indentSpaces(ic) + "private readonly " + apiProjectConfig.DatabaseScaffold.DbContextName + " _dbContext;");
            swClass.WriteLine(helper.indentSpaces(ic) + "private readonly ILogger<" + repositoryClassName + "> _logger;");
            swClass.WriteLine(helper.indentSpaces(ic) + "");
            swClass.WriteLine(helper.indentSpaces(ic) + "public " + repositoryClassName + "(" + apiProjectConfig.DatabaseScaffold.DbContextName + " dbContext, ILogger<" + repositoryClassName + "> logger)");
            swClass.WriteLine(helper.indentSpaces(ic) + "{");
            ic ++;
            swClass.WriteLine(helper.indentSpaces(ic) + "_dbContext = dbContext;");
            swClass.WriteLine(helper.indentSpaces(ic) + "_logger = logger;");
            ic --;
            swClass.WriteLine(helper.indentSpaces(ic) + "}");
            swClass.WriteLine(helper.indentSpaces(ic) + "");

            swClass.WriteLine(helper.indentSpaces(ic) + "// general");
            swClass.WriteLine(helper.indentSpaces(ic) + "public async Task AddAsync<T>(T entity) where T : class");
            swClass.WriteLine(helper.indentSpaces(ic) + "{");
            ic ++;
            swClass.WriteLine(helper.indentSpaces(ic) + "_logger.LogInformation($\"Adding an object of type {entity.GetType()} to the context.\");");
            swClass.WriteLine(helper.indentSpaces(ic) + "await _dbContext.AddAsync<T>(entity);");
            ic --;
            swClass.WriteLine(helper.indentSpaces(ic) + "}");
            swClass.WriteLine(helper.indentSpaces(ic) + "");

            swClass.WriteLine(helper.indentSpaces(ic) + "public void Delete<T>(T entity) where T : class");
            swClass.WriteLine(helper.indentSpaces(ic) + "{");
            ic ++;
            swClass.WriteLine(helper.indentSpaces(ic) + "_logger.LogInformation($\"Removing an object of type {entity.GetType()} to the context.\");");
            swClass.WriteLine(helper.indentSpaces(ic) + "_dbContext.Remove<T>(entity);");
            ic --;
            swClass.WriteLine(helper.indentSpaces(ic) + "}");
            swClass.WriteLine(helper.indentSpaces(ic) + "");

            swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<bool> SaveChangesAsync()");
            swClass.WriteLine(helper.indentSpaces(ic) + "{");
            ic ++;
            swClass.WriteLine(helper.indentSpaces(ic) + "_logger.LogInformation($\"Attempitng to save the changes in the context\");");
            swClass.WriteLine(helper.indentSpaces(ic) + "return (await _dbContext.SaveChangesAsync() > 0);");
            ic --;
            swClass.WriteLine(helper.indentSpaces(ic) + "}");
            swClass.WriteLine(helper.indentSpaces(ic) + "");

            // various get methods for each table
            // GetAllCount, GetAll
            // GetOneByPk, GetOneByUniqueIndex, ExistsByPk
            // GetMutipleByFkCount, GetMultipleByFk
            foreach(Table table in tables.OrderBy(t => t.Name))
            {
                swInterface.WriteLine(helper.indentSpaces(ii) + "// " + table.EntityName);
                // getAllEntityCount methods
                swInterface.WriteLine(helper.indentSpaces(ii) + "Task<int> GetAll" + helper.Pluralize(table.EntityName) + "CountAsync();");
                swInterface.WriteLine(helper.indentSpaces(ii) + "");

                swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<int> GetAll" + helper.Pluralize(table.EntityName) + "CountAsync()");
                swClass.WriteLine(helper.indentSpaces(ic) + "{");
                ic++;
                swClass.WriteLine(helper.indentSpaces(ic) + "return await _dbContext." + helper.Pluralize(table.EntityName) + ".CountAsync();");
                ic--;
                swClass.WriteLine(helper.indentSpaces(ic) + "}");
                swClass.WriteLine(helper.indentSpaces(ic) + "");

                // getAllEntities methods
                swInterface.WriteLine(helper.indentSpaces(ii) + 
                    "Task<EntityCollection<" + table.EntityName + ">> "
                    + "GetAll" + helper.Pluralize(table.EntityName) + "Async("
                    + "int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = \""
                    + table.DefaultSortBy + "\");");
                swInterface.WriteLine(helper.indentSpaces(ii) + "");

                swClass.WriteLine(helper.indentSpaces(ic) + 
                    "public async Task<EntityCollection<" + table.EntityName + ">> "
                    + "GetAll" + helper.Pluralize(table.EntityName) + "Async("
                    + "int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = \""
                    + table.DefaultSortBy + "\")");
                swClass.WriteLine(helper.indentSpaces(ic) + "{");
                ic ++;
                swClass.WriteLine(helper.indentSpaces(ic) + "int totalCount = await GetAll" + helper.Pluralize(table.EntityName) + "CountAsync();");
                swClass.WriteLine(helper.indentSpaces(ic) + "pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);");
                swClass.WriteLine(helper.indentSpaces(ic) + "int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);");
                swClass.WriteLine(helper.indentSpaces(ic) + "pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);");
                swClass.WriteLine(helper.indentSpaces(ic) + "int startRowIndex = (pageNumber - 1) * pageSize;");
                swClass.WriteLine(helper.indentSpaces(ic) + "int nextPage = Math.Min(pageNumber + 1, totalPages);");
                swClass.WriteLine(helper.indentSpaces(ic) + "int prevPage = Math.Max(1, pageNumber - 1);");
                swClass.WriteLine(helper.indentSpaces(ic) + "");

                swClass.WriteLine(helper.indentSpaces(ic) + table.EntityName + " [] " + helper.Pluralize(table.LoweredEntityName) + " = " 
                    + " await _dbContext." + helper.Pluralize(table.EntityName) 
                    + ".OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();");

                swClass.WriteLine(helper.indentSpaces(ic) + "EntityCollection<" + table.EntityName + "> result = new EntityCollection<" + table.EntityName + ">");
                swClass.WriteLine(helper.indentSpaces(ic) + "{");
                ic ++;
                swClass.WriteLine(helper.indentSpaces(ic) + "TotalCount = totalCount,");
                swClass.WriteLine(helper.indentSpaces(ic) + "TotalPages = totalPages,");
                swClass.WriteLine(helper.indentSpaces(ic) + "PageNumber = pageNumber,");
                swClass.WriteLine(helper.indentSpaces(ic) + "PageSize = pageSize,");
                swClass.WriteLine(helper.indentSpaces(ic) + "SortBy = sortBy,");
                swClass.WriteLine(helper.indentSpaces(ic) + "NextPageNumber = nextPage,");
                swClass.WriteLine(helper.indentSpaces(ic) + "PrevPageNumber = prevPage,");
                swClass.WriteLine(helper.indentSpaces(ic) + "Data = " + helper.Pluralize(table.LoweredEntityName) + ",");
                ic--;
                swClass.WriteLine(helper.indentSpaces(ic) + "};");
                swClass.WriteLine(helper.indentSpaces(ic) + "return result;");

                ic --;
                swClass.WriteLine(helper.indentSpaces(ic) + "}");
                swClass.WriteLine(helper.indentSpaces(ic) + "");

                if (table.PrimaryKey != null)
                {
                    // primary key methods

                    // exists by primary key
                    swInterface.WriteLine(helper.indentSpaces(ii) + "Task<bool> " + table.EntityName + "ExistsAsync(" + table.PrimaryKey.ParamList + ");");
                    swInterface.WriteLine(helper.indentSpaces(ii) + "");

                    swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<bool> " + table.EntityName + "ExistsAsync(" + table.PrimaryKey.ParamList + ")");
                    swClass.WriteLine(helper.indentSpaces(ic) + "{");
                    ic ++;
                    swClass.WriteLine(helper.indentSpaces(ic) + "return await _dbContext." + helper.Pluralize(table.EntityName) + ".AnyAsync(e => " + table.PrimaryKey.FindCondition + ");");
                    ic --;
                    swClass.WriteLine(helper.indentSpaces(ic) + "}");
                    swClass.WriteLine(helper.indentSpaces(ic) + "");


                    // find entity by primary key
                    swInterface.WriteLine(helper.indentSpaces(ii) + "Task<" + table.EntityName + "> Get" + table.EntityName + "Async(" + table.PrimaryKey.ParamList + ");");
                    swInterface.WriteLine(helper.indentSpaces(ii) + "");

                    swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<" + table.EntityName + "> Get" + table.EntityName + "Async(" + table.PrimaryKey.ParamList + ")");
                    swClass.WriteLine(helper.indentSpaces(ic) + "{");
                    ic ++;
                    swClass.WriteLine(helper.indentSpaces(ic) + "return await _dbContext." + helper.Pluralize(table.EntityName) + ".FirstOrDefaultAsync(e => " + table.PrimaryKey.FindCondition + ");");
                    ic --;
                    swClass.WriteLine(helper.indentSpaces(ic) + "}");
                    swClass.WriteLine(helper.indentSpaces(ic) + "");
                }

                // for each Unique Index 
                foreach(TableIndex uix in table.UniqueIndecies)
                {
                    // GetEntityByUniqueIndexColumns
                    swInterface.WriteLine(helper.indentSpaces(ii) + "Task<" + table.EntityName + "> Get" + table.EntityName + "By" + uix.IndexColumnNames + "Async(" + uix.ParamList + ");");
                    swInterface.WriteLine(helper.indentSpaces(ii) + "");

                    swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<" + table.EntityName + "> Get" + table.EntityName + "By" + uix.IndexColumnNames + "Async(" + uix.ParamList + ")");
                    swClass.WriteLine(helper.indentSpaces(ic) + "{");
                    ic ++;
                    swClass.WriteLine(helper.indentSpaces(ic) + "return await _dbContext." + helper.Pluralize(table.EntityName) + ".FirstOrDefaultAsync(e => " + uix.FindCondition + ");");
                    ic --;
                    swClass.WriteLine(helper.indentSpaces(ic) + "}");
                    swClass.WriteLine(helper.indentSpaces(ic) + "");
                }

                foreach(ForeignKey fk in table.ForeignKeysAsChild)
                {
                    Column column = table.Columns.FirstOrDefault(c => c.ColumnId == fk.ParentTableColumnId);
                    string columnName = column.PropertyName;
                    string loweredColumnName = column.loweredPropertyName;

                    // getEntitiesByFkCount
                    swInterface.WriteLine(helper.indentSpaces(ii) + "Task<int> "
                        + "Get" + helper.Pluralize(table.EntityName) + "By" + columnName + "CountAsync(" 
                        + column.EntityDataType + " " + loweredColumnName 
                        + ");");
                    swInterface.WriteLine(helper.indentSpaces(ii) + "");

                    swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<int> "
                        + "Get" + helper.Pluralize(table.EntityName) + "By" + columnName + "CountAsync(" 
                        + column.EntityDataType + " " + loweredColumnName 
                        + ")");
                    swClass.WriteLine(helper.indentSpaces(ic) + "{");
                    ic ++;
                    swClass.WriteLine(helper.indentSpaces(ic) + "return await _dbContext." + helper.Pluralize(table.EntityName) + ".Where(b => b." + columnName + " == " + loweredColumnName + ").CountAsync();");
                    ic --;
                    swClass.WriteLine(helper.indentSpaces(ic) + "}");
                    swClass.WriteLine(helper.indentSpaces(ic) + "");

                    // getEntitiesByFk
                    swInterface.WriteLine(helper.indentSpaces(ii) + "Task<EntityCollection<" + table.EntityName + ">> "
                        + "Get" + helper.Pluralize(table.EntityName) + "By" + columnName + "Async(" 
                        + column.EntityDataType + " " + loweredColumnName 
                        + ", int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = \"" + table.DefaultSortBy + "\");");
                    swInterface.WriteLine(helper.indentSpaces(ii) + "");

                    swClass.WriteLine(helper.indentSpaces(ic) + "public async Task<EntityCollection<" + table.EntityName + ">> "
                        + "Get" + helper.Pluralize(table.EntityName) + "By" + columnName + "Async(" 
                        + column.EntityDataType + " " + loweredColumnName 
                        + ", int pageNumber = 1, int pageSize = Constants.Paging.DefaultPageSize, string sortBy = \"" + table.DefaultSortBy + "\")");
                    swClass.WriteLine(helper.indentSpaces(ic) + "{");
                    ic ++;

                    swClass.WriteLine(helper.indentSpaces(ic) + "pageSize = (pageSize <= 0) ? defaultPageSize : Math.Min(maxPageSize, pageSize);");
                    swClass.WriteLine(helper.indentSpaces(ic) + "int totalCount = await Get" + helper.Pluralize(table.EntityName) + "By" + columnName + "CountAsync(" + loweredColumnName +");");
                    swClass.WriteLine(helper.indentSpaces(ic) + "int totalPages = totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1);");
                    swClass.WriteLine(helper.indentSpaces(ic) + "pageNumber = (pageNumber <= 0) ? 1 : Math.Min(totalPages, pageNumber);");
                    swClass.WriteLine(helper.indentSpaces(ic) + "int startRowIndex = (pageNumber - 1) * pageSize;");
                    swClass.WriteLine(helper.indentSpaces(ic) + "int nextPage = Math.Min(pageNumber + 1, totalPages);");
                    swClass.WriteLine(helper.indentSpaces(ic) + "int prevPage = Math.Max(1, pageNumber - 1);");
                    swClass.WriteLine(helper.indentSpaces(ic) + "");

                    swClass.WriteLine(helper.indentSpaces(ic) + table.EntityName + " [] " + helper.Pluralize(table.LoweredEntityName) + " = " 
                        + " await _dbContext." + helper.Pluralize(table.EntityName) 
                        + ".Where(e => e." + columnName + " == " + loweredColumnName + ")"
                        + ".OrderBy(sortBy).Skip(startRowIndex).Take(pageSize).ToArrayAsync();");

                    swClass.WriteLine(helper.indentSpaces(ic) + "EntityCollection<" + table.EntityName + "> result = new EntityCollection<" + table.EntityName + ">");
                    swClass.WriteLine(helper.indentSpaces(ic) + "{");
                    ic ++;
                    swClass.WriteLine(helper.indentSpaces(ic) + "TotalCount = totalCount,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "TotalPages = totalPages,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "PageNumber = pageNumber,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "PageSize = pageSize,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "SortBy = sortBy,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "NextPageNumber = nextPage,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "PrevPageNumber = prevPage,");
                    swClass.WriteLine(helper.indentSpaces(ic) + "Data = " + helper.Pluralize(table.LoweredEntityName) + ",");
                    ic--;
                    swClass.WriteLine(helper.indentSpaces(ic) + "};");
                    swClass.WriteLine(helper.indentSpaces(ic) + "return result;");
                    ic--;
                    swClass.WriteLine(helper.indentSpaces(ic) + "}");
                    swClass.WriteLine(helper.indentSpaces(ic) + "");
                }
            }
            ii --;
            swInterface.WriteLine(helper.indentSpaces(ii) + "}");
            ii --;
            swInterface.WriteLine(helper.indentSpaces(ii) + "}");
            swInterface.Close();

            ic --;
            swClass.WriteLine(helper.indentSpaces(ic) + "}");
            ic --;
            swClass.WriteLine(helper.indentSpaces(ic) + "}");
            swClass.Close();
        }
#endregion
 
#region v004 methods - GenerateModelClasses, GenerateMapperProfile
        static void GenerateModelClasses(string projRoot, List<Table> tables)
        {
            string projectDataFolder = projRoot + "/Data";
            if (!System.IO.Directory.Exists(projectDataFolder))
            {
                System.IO.Directory.CreateDirectory(projectDataFolder);
            }

            // Models

            string modelClassesFolder = projRoot + "/Data/Models";
            if (System.IO.Directory.Exists(modelClassesFolder))
            {
                System.IO.Directory.Delete(modelClassesFolder, recursive:true);
            }
            System.IO.Directory.CreateDirectory(modelClassesFolder);

            System.IO.StreamWriter sw = null;
            foreach(Table table in tables)
            {
                // out facing contract
                string contracModelClassFile = modelClassesFolder + "/" + table.EntityName + ".cs";
                sw = new StreamWriter(contracModelClassFile, false);
                sw.Close();
                sw = new StreamWriter(contracModelClassFile, true);
                int indent = 0;
                sw.WriteLine(helper.indentSpaces(indent) + "using System;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Collections.Generic;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.ComponentModel.DataAnnotations;");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Data.Models");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "public partial class " + table.EntityName);
                sw.WriteLine(helper.indentSpaces(indent) + "{");

                indent ++;

                foreach(Column column in table.Columns)
                {
                    // if (table.PrimaryKey == null || !table.PrimaryKey.IndexColumns.Select(ic => ic.Column).Any(pkc => pkc.ColumnId == column.ColumnId))
                    // {
                        sw.WriteLine(helper.indentSpaces(indent) + "public " + column.EntityDataType + " " + column.PropertyName + " { get; set; }");
                        sw.WriteLine(helper.indentSpaces(indent) + "");
                    // }
                }

                // foreach(ForeignKey fk in table.ForeignKeysAsParent)
                // {
                //     string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;

                //     sw.WriteLine(helper.indentSpaces(indent) + "public virtual ICollection<" + childTableName + "> " + helper.Pluralize(childTableName) + " { get; set; }");

                // }

                List<long> parentTableObjectIdList = table.ForeignKeysAsParent.Select(fk => fk.ParentTableObjectId).Distinct().ToList();
                foreach(long parentTableObjectId in parentTableObjectIdList)
                {
                    List<ForeignKey> fks = table.ForeignKeysAsParent.Where(fk => fk.ParentTableObjectId == parentTableObjectId).ToList();

                    if (fks.Count == 1)
                    {
                        ForeignKey fk = fks[0];
                        string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;
                        sw.WriteLine(helper.indentSpaces(indent) + "public virtual ICollection<" + childTableName + "> " + helper.Pluralize(childTableName) + " { get; set; }");
                    }
                    else
                    {
                        foreach(ForeignKey fk in fks)
                        {
                            
                            string childTableName = tables.FirstOrDefault(t => fk.ParentTableObjectId == t.ObjectId).EntityName;
                            sw.WriteLine(helper.indentSpaces(indent) + "public virtual ICollection<" + childTableName + "> " + helper.Pluralize(childTableName) + "_" + fk.Name + " { get; set; }");
                        }
                    }
                }

                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.Close();

                // dto for create
                string dtoForCreateClassName = table.EntityName + "ForCreate";
                string dtoForCreateFile = modelClassesFolder + "/" + dtoForCreateClassName + ".cs";
                sw = new StreamWriter(dtoForCreateFile, false);
                sw.Close();
                sw = new StreamWriter(dtoForCreateFile, true);
                indent = 0;
                sw.WriteLine(helper.indentSpaces(indent) + "using System;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Collections.Generic;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.ComponentModel.DataAnnotations;");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Data.Models");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "public partial class " + dtoForCreateClassName);
                sw.WriteLine(helper.indentSpaces(indent) + "{");



                indent ++;
                foreach(Column column in table.Columns)
                {
                    if (table .PrimaryKey == null || ! table.PrimaryKey.IndexColumns.Select(ic => ic.Column).Any(pkc => pkc.ColumnId == column.ColumnId))
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + "public " + column.EntityDataType + " " + column.PropertyName + " { get; set; }");
                        sw.WriteLine(helper.indentSpaces(indent) + "");
                    }
                }
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.Close();

                // dto for update
                string dtoForUpdateClassName = table.EntityName + "ForUpdate";
                string dtoForUpdateFile = modelClassesFolder + "/" + dtoForUpdateClassName + ".cs";
                sw = new StreamWriter(dtoForUpdateFile, false);
                sw.Close();
                sw = new StreamWriter(dtoForUpdateFile, true);
                indent = 0;
                sw.WriteLine(helper.indentSpaces(indent) + "using System;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.Collections.Generic;");
                sw.WriteLine(helper.indentSpaces(indent) + "using System.ComponentModel.DataAnnotations;");
                sw.WriteLine(helper.indentSpaces(indent) + "");
                sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Data.Models");
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                sw.WriteLine(helper.indentSpaces(indent) + "public partial class " + dtoForUpdateClassName);
                sw.WriteLine(helper.indentSpaces(indent) + "{");
                indent ++;
                foreach(Column column in table.Columns)
                {
                    if (table .PrimaryKey == null || ! table.PrimaryKey.IndexColumns.Select(ic => ic.Column).Any(pkc => pkc.ColumnId == column.ColumnId))
                    {
                        sw.WriteLine(helper.indentSpaces(indent) + "public " + column.EntityDataType + " " + column.PropertyName + " { get; set; }");
                        sw.WriteLine(helper.indentSpaces(indent) + "");
                    }
                }
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                indent --;
                sw.WriteLine(helper.indentSpaces(indent) + "}");
                sw.Close();
            }
        }

        static void GenerateMapperProfile(string projRoot, List<Table> tables)
        {
            string mapperProfileClassName = apiProjectConfig.ApiProjectName + "MapperProfile";
            string mapperProfileFilePath = projRoot + "/Data/" + mapperProfileClassName + ".cs";

            System.IO.StreamWriter sw = new StreamWriter(mapperProfileFilePath, false);
            sw.Close();

            int indent = 0;
            sw = new StreamWriter(mapperProfileFilePath, true);
            sw.WriteLine(helper.indentSpaces(indent) + "using AutoMapper;");
            sw.WriteLine(helper.indentSpaces(indent) + "");
            sw.WriteLine(helper.indentSpaces(indent) + "namespace " + apiProjectConfig.ApiProjectName + ".Data");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "public class " + mapperProfileClassName + " : Profile");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;
            sw.WriteLine(helper.indentSpaces(indent) + "public " + mapperProfileClassName + "()");
            sw.WriteLine(helper.indentSpaces(indent) + "{");
            indent ++;

            foreach(Table table in tables)
            {
                // out facing contract
                sw.WriteLine(helper.indentSpaces(indent) + "this.CreateMap<Entities." + table.EntityName + ", Models." + table.EntityName + ">();");
                //dtoForCreation
                sw.WriteLine(helper.indentSpaces(indent) + "this.CreateMap<Models." + table.EntityName + "ForCreate, Entities." + table.EntityName + ">();");
                // dtoForUpdate
                sw.WriteLine(helper.indentSpaces(indent) + "this.CreateMap<Models." + table.EntityName + "ForUpdate, Entities." + table.EntityName + ">().ReverseMap();");
            }

            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");
            indent --;
            sw.WriteLine(helper.indentSpaces(indent) + "}");

            sw.Close();
        }
#endregion
    }
}
