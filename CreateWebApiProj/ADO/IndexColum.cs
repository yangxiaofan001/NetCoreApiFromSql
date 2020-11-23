
namespace CreateWebApiProj.ADO
{
    public class IndexColumn
    {
        public long TableObjectId { get; set; }

        public int IndexId { get; set; }

        public int KeyOrdinal { get; set; }

        public Column Column { get; set; }
    }
}