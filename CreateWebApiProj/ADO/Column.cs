namespace CreateWebApiProj.ADO
{
    public class Column
    {
        public long ObjectId {get; set; }

        public int ColumnId { get; set; }
        
        public string Name { get; set; }

        public string PropertyName { get; set; }

        public string loweredPropertyName
        {
            get
            {
                return PropertyName.ToLower()[0] + PropertyName.Substring(1, PropertyName.Length - 1);
            }
        }

        public string DataType { get; set; }

        public string EntityDataType { get; set;}

        public bool IsMoney { get; set; }

        public int MaxLength { get; set;}

        public bool Required { get; set;}

        public bool FixedLength { get; set; }
    }
}