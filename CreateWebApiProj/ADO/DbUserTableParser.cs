
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CreateWebApiProj.ADO
{
    public class DbUserTableParser
    {
        ApiProjectDbContext _dbContext;
        Helper helper;
        public DbUserTableParser(ApiProjectDbContext dbContext)
        {
            _dbContext = dbContext;
            helper = new Helper();
        }

        public List<Table> ParseTables(string[] includeTableNames = null, string[] excludeTableNames = null)
        {
            List<Table> tables = GetUserTables(includeTableNames, excludeTableNames);
            List<long> tableObjectIds = tables.Select(t => t.ObjectId).ToList();

            foreach(long tableObjectId in tableObjectIds)
            {
                Table table = tables.FirstOrDefault(t => t.ObjectId == tableObjectId);
                Console.WriteLine("DbUserTablePaser - parse table " + table.Name);
                table.Columns = GetTableColumns(table);
                table.PrimaryKey = GetPrimaryKey(table);
            }

            foreach(long tableObjectId in tableObjectIds)
            {
                Table table = tables.FirstOrDefault(t => t.ObjectId == tableObjectId);
                table.UniqueIndecies = GetUniqueIndecies(table);
                table.ForeignKeysAsChild = GetForeignKeys(table, asChild:true, tables);
                table.ForeignKeysAsParent = GetForeignKeys(table, asChild:false, tables);
            }

            return tables;
        }

        public List<Table> GetUserTables(string[] includeTableNames = null, string[] excludeTableNames = null)
        {
            Console.WriteLine("DbUserTablePaser - get user tables");

            List<Table> tables = new List<Table>();
            string userTablesQueryCommandText = @"select object_id, name, [type] from sys.objects where [type] = 'U' or [type] = 'V'";

            if (!(includeTableNames is null) && includeTableNames.Length > 0)
            {
                userTablesQueryCommandText = userTablesQueryCommandText + " and name in (";
                foreach(string name in includeTableNames)
                {
                    userTablesQueryCommandText = userTablesQueryCommandText + "'" + name + "', ";
                }

                userTablesQueryCommandText = userTablesQueryCommandText.Substring(0, userTablesQueryCommandText.Length - 2);
                userTablesQueryCommandText = userTablesQueryCommandText + ")";
            }
            else if (!(excludeTableNames is null) && excludeTableNames.Length > 0)
            {
                userTablesQueryCommandText = userTablesQueryCommandText + " and name not in (";
                foreach(string name in excludeTableNames)
                {
                    userTablesQueryCommandText = userTablesQueryCommandText + "'" + name + "', ";
                }

                userTablesQueryCommandText = userTablesQueryCommandText.Substring(0, userTablesQueryCommandText.Length - 2);
                userTablesQueryCommandText = userTablesQueryCommandText + ")";
            }

            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();
            }
            using(var command = conn.CreateCommand())
            {
                command.CommandText = userTablesQueryCommandText;
                DbDataReader dataReader = command.ExecuteReader();
                while(dataReader.Read())
                {
                    IDataRecord record = (IDataRecord)dataReader;
                    tables.Add(new Table 
                    {
                        ObjectId = Convert.ToInt64(record[0]),
                        Name = record[1].ToString().Trim(),
                        EntityName = helper.ToCamelCase(record[1].ToString().Trim()),
                        ObjectType = record[2].ToString().Trim(),
                        Columns = new List<Column>(),
                        ForeignKeysAsChild = new List<ForeignKey>(),
                        ForeignKeysAsParent = new List<ForeignKey>()
                    });
                }
                dataReader.Close();
            }
            
            if (conn.State.Equals(ConnectionState.Open))
            {
                conn.Close();
            }

            return tables;
        }

        public List<Column> GetTableColumns(Table table)
        {
            Console.WriteLine("DbUserTablePaser - parse columns in table " + table.Name);
            List<Column> columns = new List<Column>();

            string tableColumnsQueryCommandText = 
            "select c.name, c.column_id , tp.name, c.user_type_id, c.max_length, c.[precision], c.scale, c.is_nullable, c.is_identity "
            + "from sys.columns c, sys.types tp "
            + " where c.object_id = " + table.ObjectId.ToString()
            + " and c.user_type_id = tp.user_type_id"
            + " order by c.column_id";

            // Console.WriteLine("Query:");
            // Console.WriteLine(tableColumnsQueryCommandText);

            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();
            }

            using(var command = conn.CreateCommand())
            {
                command.CommandText = tableColumnsQueryCommandText;
                //@"select t.name, t.object_id, c.name, c.column_id , tp.name, c.user_type_id, c.max_length, c.[precision], c.scale, c.is_nullable, c.is_identity, c.*
                // from sys.objects t, sys.columns c, sys.types tp
                // where t.object_id = c.object_id
                //     and c.user_type_id = tp.user_type_id
                //     and t.[type] = 'U'
                // order by t.object_id, c.column_id";

                DbDataReader dataReader = command.ExecuteReader();

                while(dataReader.Read())
                {
                    IDataRecord record = (IDataRecord)dataReader;

                    bool fixedLength = false;
                    bool isMoney = false;
                    bool required = !Convert.ToBoolean(record[7]);

                    string dataType = record[2].ToString().Trim();
                    string entityDataType = dataType;

                    if (dataType.ToLower() == "varchar" || dataType.ToLower() == "nvarchar" || dataType.ToLower() == "ntext")
                    {
                        entityDataType = "string";
                    }
                    else if (dataType.ToLower() == "char")
                    {
                        entityDataType = "string";
                        fixedLength = true;
                    }
                    else if (dataType.ToLower() == "bit")
                    {
                        entityDataType = "bool";
                    }
                    else if (dataType.ToLower() == "tinyint")
                    {
                        entityDataType = "Byte";
                    }
                    else if (dataType.ToLower() == "number")
                    {
                        int scale = Convert.ToInt32(record[6]);
                        int precision = Convert.ToInt32(record[5]);
                        if (scale > 0)
                        {
                            entityDataType = "decimal";
                        }
                        else if (precision > 8)
                        {
                            entityDataType =  "long";
                        }
                        else
                        {
                            entityDataType = "int";
                        }
                    }
                    else if (dataType.ToLower() == "datetime")
                    {
                        entityDataType = "DateTime";
                    }
                    else if (dataType.ToLower() == "real")
                    {
                        entityDataType = "decimal";
                    }
                    else if (dataType.ToLower() == "smallint")
                    {
                        entityDataType = "short";
                    }
                    else if (dataType.ToLower() == "bigint")
                    {
                        entityDataType = "long";
                    }
                    else if (dataType.ToLower() == "money")
                    {
                        entityDataType = "decimal";
                        isMoney = true;
                    }
                    else if (dataType.ToLower() == "uniqueidentifier")
                    {
                        entityDataType = "Guid";
                    }

                    List<string> nullableDataTypes = new List<string>{"bool", "int", "short", "long", "decimal", "double", "datetime", "guid"};
                    if (nullableDataTypes.Contains(entityDataType.ToLower()) )
                    {
                        entityDataType = required ? entityDataType : (entityDataType + "?");
                    }



                    columns.Add(new Column
                    {
                        ObjectId = table.ObjectId,
                        ColumnId = Convert.ToInt32(record[1]),
                        Name = record[0].ToString().Trim(),
                        PropertyName = helper.ToCamelCase(record[0].ToString().Trim()),
                        DataType = dataType,
                        EntityDataType = entityDataType,
                        MaxLength = Convert.ToInt32(record[4]),
                        Required = !Convert.ToBoolean(record[7]),
                        FixedLength = fixedLength,
                        IsMoney = isMoney
                    });
                }

                dataReader.Close();
            }


            if (conn.State.Equals(ConnectionState.Open))
            {
                conn.Close();
            }

            Console.WriteLine("Total of " + columns.Count + " columns in table " + table.Name);

            return columns;
        }

        public TableIndex GetPrimaryKey(Table table)
        {
            Console.WriteLine("DbUserTablePaser - parse PrimaryKey in table " + table.Name);
            TableIndex pk = null;

            string primaryKeyQueryCommandText = 
                "select name, index_id "
                + "from sys.indexes "
                + "where object_id  = " + table.ObjectId + " "
                + "and is_primary_key = 1 ";

            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();
            }

            using(var command = conn.CreateCommand())
            {
                command.CommandText = primaryKeyQueryCommandText;

                DbDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    IDataRecord record = (IDataRecord)dataReader;
                    pk = new TableIndex
                    {
                        TableObjectId = table.ObjectId,
                        Name = record[0].ToString().Trim(),
                        IndexId = Convert.ToInt32(record[1]),
                        IndexColumns = new List<IndexColumn>()
                    };
                }

                dataReader.Close();
            }

            if (pk != null)
            {
                string primaryKeyColumnsQueryCommandText = 
                    "select key_ordinal, column_id "
                    + "from sys.index_columns "
                    + "where object_id = " + table.ObjectId + " " 
                    + "and index_id = " + pk.IndexId;

                using(var command = conn.CreateCommand())
                {
                    command.CommandText = primaryKeyColumnsQueryCommandText;

                    DbDataReader dataReader = command.ExecuteReader();

                    if (dataReader.Read())
                    {
                        IDataRecord record = (IDataRecord)dataReader;
                        pk.IndexColumns.Add( new IndexColumn
                        {
                            TableObjectId = table.ObjectId,
                            IndexId = pk.IndexId,
                            KeyOrdinal = Convert.ToInt32(record[0]),
                            Column = table.Columns.FirstOrDefault(c => c.ColumnId == Convert.ToInt32(record[1]))
                        });
                    }

                    dataReader.Close();
                }

                if (conn.State.Equals(ConnectionState.Open))
                {
                    conn.Close();
                }
            }

            return pk;
        }

        public List<TableIndex> GetUniqueIndecies(Table table)
        {
            Console.WriteLine("DbUserTablePaser - parse unique indecies in table " + table.Name);
            List<TableIndex> uniqueIndecies = new List<TableIndex>();

            string primaryKeyQueryCommandText = 
                "select name, index_id "
                + "from sys.indexes "
                + "where object_id  = " + table.ObjectId + " "
                + "and is_unique = 1"
                + "and is_primary_key <> 1 ";

            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();
            }

            using(var command = conn.CreateCommand())
            {
                command.CommandText = primaryKeyQueryCommandText;

                DbDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    IDataRecord record = (IDataRecord)dataReader;
                    uniqueIndecies.Add(new TableIndex
                    {
                        TableObjectId = table.ObjectId,
                        IndexId = Convert.ToInt32(record[1]),
                        Name = record[0].ToString().Trim(),
                        IndexColumns = new List<IndexColumn>()
                    });
                }

                dataReader.Close();
            }

            foreach(TableIndex uix in uniqueIndecies)
            {
                string UniqueIndexColumnsQueryCommandText = 
                    "select key_ordinal, column_id "
                    + "from sys.index_columns "
                    + "where object_id = " + table.ObjectId + " " 
                    + "and index_id = " + uix.IndexId;

                using(var command = conn.CreateCommand())
                {
                    command.CommandText = UniqueIndexColumnsQueryCommandText;

                    DbDataReader dataReader = command.ExecuteReader();

                    if (dataReader.Read())
                    {
                        IDataRecord record = (IDataRecord)dataReader;
                        uix.IndexColumns.Add( new IndexColumn
                        {
                            TableObjectId = table.ObjectId,
                            IndexId = uix.IndexId,
                            KeyOrdinal = Convert.ToInt32(record[0]),
                            Column = table.Columns.FirstOrDefault(c => c.ColumnId == Convert.ToInt32(record[1]))
                        });
                    }

                    dataReader.Close();
                }
            }

            if (conn.State.Equals(ConnectionState.Open))
            {
                conn.Close();
            }

            return uniqueIndecies;
        }

        public List<ForeignKey> GetForeignKeys(Table table, bool asChild, List<Table> allTables)
        {
            Console.WriteLine("DbUserTablePaser - parse ForeignKeys in table " + table.Name + " where " + table.Name + " is " + (asChild?"child":"parent"));

            List<ForeignKey> foreignKeys = new List<ForeignKey>();
            string tableFKsQueryCommandText = "select Name, object_id, parent_object_id, referenced_object_id "
                + "from sys.foreign_keys";

            if (asChild)
            {
                tableFKsQueryCommandText = tableFKsQueryCommandText + " where parent_object_id = " + table.ObjectId;
            }
            else
            {
                tableFKsQueryCommandText = tableFKsQueryCommandText + " where referenced_Object_id = " + table.ObjectId;
            }


            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();
            }

            using(var command = conn.CreateCommand())
            {
                command.CommandText = tableFKsQueryCommandText;

                DbDataReader dataReader = command.ExecuteReader();

                while(dataReader.Read())
                {
                    IDataRecord record = (IDataRecord)dataReader;
                    long parentTableObjectId = Convert.ToInt64(record[2]);
                    long referencedTableObjectId = Convert.ToInt64(record[3]);

                    if (allTables.Any(t => t.ObjectId == parentTableObjectId) && allTables.Any(t => t.ObjectId == referencedTableObjectId))
                    {
                        foreignKeys.Add(new ForeignKey
                        {
                            Name = record[0].ToString().Trim(),
                            ObjectId = Convert.ToInt64(record[1]),
                            ParentTableObjectId = Convert.ToInt64(record[2]),
                            ReferencedTableObjectId = Convert.ToInt32(record[3]),
                        });
                    }
                }
                dataReader.Close();
            }

            if (conn.State.Equals(ConnectionState.Open))
            {
                conn.Close();
            }

            foreach(ForeignKey fk in foreignKeys)
            {
                string tableFKColumnsQueryCommandText = "select parent_column_id, referenced_column_id "
                + "from sys.foreign_key_columns "
                + "where constraint_object_id = " + fk.ObjectId.ToString();

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();
                }

                using(var command = conn.CreateCommand())
                {
                    command.CommandText = tableFKColumnsQueryCommandText;

                    DbDataReader dataReader = command.ExecuteReader();

                    while(dataReader.Read())
                    {
                        IDataRecord record = (IDataRecord)dataReader;
                        int parentTableColumnId = Convert.ToInt32(record[0]);
                        int referencedTableColumntId = Convert.ToInt32(record[1]);

                        fk.ParentTableColumnId = parentTableColumnId;
                        fk.ReferencedTableColumnId = referencedTableColumntId;
                        fk.ParentTable = allTables.FirstOrDefault(t => t.ObjectId == fk.ParentTableObjectId);
                        fk.ReferencedTable = allTables.FirstOrDefault(t => t.ObjectId == fk.ReferencedTableObjectId);
                        fk.ParentTableColumn = fk.ParentTable.Columns.FirstOrDefault(c => c.ColumnId == parentTableColumnId);
                        fk.ReferencedTableColumn = fk.ReferencedTable.Columns.FirstOrDefault(c => c.ColumnId == referencedTableColumntId);
                    }
                    dataReader.Close();
                }

                if (conn.State.Equals(ConnectionState.Open))
                {
                    conn.Close();
                }
            }

            Console.WriteLine("total of " + foreignKeys.Count + " in table " + table.Name + " where " + table.Name + " is " + (asChild?"child":"parent"));

            return foreignKeys;
        }
    }
}