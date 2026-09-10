namespace BookTable.Entities
{
    public class Reservation
    {

        public int Id { get; set; }
        public int TableId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Table Table { get; set; } = null!;

    }
}
