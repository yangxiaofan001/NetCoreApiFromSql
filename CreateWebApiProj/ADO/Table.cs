
using System.Collections.Generic;
using System.Linq;

namespace CreateWebApiProj.ADO
{
    public class Table
    {
        public long ObjectId {get; set; }

        public string Name {get; set;}

        public string EntityName { get; set; }

        public string LoweredEntityName 
        {
            get
            {
                return EntityName.ToLower()[0] + EntityName.Substring(1, EntityName.Length - 1);
            }
        }

        public string PlurizedEntityName
        {
            get
            {
                return new Helper().Pluralize(EntityName);
            }
        }
        public string ObjectType { get; set; }

        public List<Column> Columns { get; set; }

        public List<ForeignKey>  ForeignKeysAsChild { get; set; }

        public List<ForeignKey> ForeignKeysAsParent { get; set; }

        public TableIndex PrimaryKey { get; set; }

        public List<TableIndex> UniqueIndecies { get; set; }

        // default sort by - primary key columns (order by key_ordinal) desc
        public string DefaultSortBy
        {
            get
            {
                if (PrimaryKey == null) return "";

                string defaultSortBy = "";

                foreach(Column column in PrimaryKey.IndexColumns.Select(ic=>ic.Column))
                {
                    defaultSortBy = defaultSortBy + column.PropertyName + " Desc, ";
                }

                if (defaultSortBy.Length > 1)
                {
                    defaultSortBy = defaultSortBy.Substring(0, defaultSortBy.Length - 2);
                }        

                return defaultSortBy;
            }
        }

        
    }
}