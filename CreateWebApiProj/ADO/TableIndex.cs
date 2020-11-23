using System.Linq;
using System.Collections.Generic;

namespace CreateWebApiProj.ADO
{
    public class TableIndex
    {
        public long TableObjectId { get; set; }

        public int IndexId { get; set; }

        public string Name { get; set; }

        public List<IndexColumn> IndexColumns { get; set; }

        // repository and api GetSingle method parameter list
        public string ParamList
        {
            get
            {
                string paramList = "";

                foreach(Column column in IndexColumns.Select(ic=>ic.Column))
                {
                    paramList = paramList + column.EntityDataType + " " + column.loweredPropertyName + ", ";
                }

                if (paramList.Length > 1)
                {
                    paramList = paramList.Substring(0, paramList.Length - 2);
                }

                return paramList;
                
            }
        }

        public string ParamValueList
        {
            get
            {
                string paramList = "";

                foreach(Column column in IndexColumns.Select(ic=>ic.Column))
                {
                    paramList = paramList + column.loweredPropertyName + ", ";
                }

                if (paramList.Length > 1)
                {
                    paramList = paramList.Substring(0, paramList.Length - 2);
                }

                return paramList;
                
            }
        }

        // DbSet.Find or DbSet.FirstOrDefault linq condition
        public string FindCondition
        {
            get
            {
                string findCondition = "";
                foreach(Column column in IndexColumns.Select(ic=>ic.Column))
                {
                    findCondition = findCondition + "e." + column.PropertyName + " == " + column.loweredPropertyName + " && ";
                }

                if (findCondition.Length > 1)
                {
                    findCondition = findCondition.Substring(0, findCondition.Length - 4);
                }

                return findCondition;
            }
        }

        public string IndexColumnNames
        {
            get
            {
                string indecColumnNames = "";

                foreach(Column column in IndexColumns.OrderBy(ic => ic.KeyOrdinal).Select(ic=>ic.Column))
                {
                    indecColumnNames = indecColumnNames + column.PropertyName;
                }

                return indecColumnNames;
            }
        }        

        public string GetByUixApiRoute
        {
            get
            {
                string route = "";

                foreach(Column column in IndexColumns.Select(ic=>ic.Column))
                {
                    route = route + "{" + column.loweredPropertyName + "}";
                }

                return route;
            }
        }
    }
}