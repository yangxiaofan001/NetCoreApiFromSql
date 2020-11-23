
namespace CreateWebApiProj.ConfigSections
{
    public class ApiProjectSection
    {
        public string TemplateFolder { get; set; }

        public string ApiProjectName {get;set;}
        
        public string OutputFolder {get;set;}

        public DatabaseScaffoldConfigSection DatabaseScaffold { get; set;}

        public PackageVersion [] PackageVersions { get; set; }
    }
}