namespace BookTable.Dtos
{
    public record CreateReservationRequest(
        int TableId,
        DateTime StartTime,
        DateTime EndTime
    );
}
