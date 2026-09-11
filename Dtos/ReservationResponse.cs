namespace BookTable.Dtos
{
    public record ReservationResponse(
        int Id,
        int TableId,
        int Capacity,
        DateTime StartTime,
        DateTime EndTime
    );
}
