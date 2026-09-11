namespace BookTable.Entities
{
    public class Reservation
    {

        public int Id { get; set; }
        public int TableId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(1);
        public Table Table { get; set; } = null!;

    }
}
