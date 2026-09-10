namespace BookTable.Dtos
{
    public record ReservationResponse(
        int Id,
        int TableId,
        int TableNumber,
        int Capacity,
        DateTime StartTime,
        DateTime EndTime
    );
}
