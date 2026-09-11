namespace BookTable.Entities
{
    public class Table
    {
        public int Id { get; set; }
        public int Capacity { get; set; }
        public string? BlobName { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
