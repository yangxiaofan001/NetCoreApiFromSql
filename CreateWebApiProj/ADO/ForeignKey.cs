namespace CreateWebApiProj.ADO
{
    public class ForeignKey
    {
        public long ObjectId { get; set; }

        public string Name { get; set; }

        public long ParentTableObjectId { get; set; }

        // current table where the fk is defined - the child in the foreignket relationship
        public Table ParentTable { get; set; }

        public int ParentTableColumnId { get; set; }

        // Colmn in current table where the fk is defined - the child in the foreignkey relationship
        public Column ParentTableColumn { get; set; }

        public long ReferencedTableObjectId { get; set; }

        // referenced table - the parent in the foreignkey relationship
        public Table ReferencedTable { get; set;}

        public int ReferencedTableColumnId { get; set; }

        // referenced column in referenced table - the parent in the foreignkey relationship
        public Column ReferencedTableColumn { get; set; }

        public string GetByFkColumnApiRoute 
        {
            get
            {
                string route = "api/" + ReferencedTable.PlurizedEntityName + "/{" + ReferencedTableColumn.loweredPropertyName + "}/" 
                    + ParentTable.PlurizedEntityName;

                return route;
            }
        }
    }
}