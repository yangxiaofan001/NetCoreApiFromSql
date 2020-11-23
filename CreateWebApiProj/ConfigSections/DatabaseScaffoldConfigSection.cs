namespace CreateWebApiProj.ConfigSections
{
    public class DatabaseScaffoldConfigSection
    {
        public string DbContextName { get; set; }

        public string ConnectionString { get; set; }

        public string [] IncludeTables{get;set;}

        public string [] ExcludeTables{get;set;}
    }
}