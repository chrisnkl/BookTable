namespace BookTable.Dtos
{
    public record CreateTableRequest(
        int Number,
        int Capacity,
        string? BlobName = null
    );

    public record TableResponse(
        int Id,
        int Number,
        int Capacity,
        string? BlobUrl,
        List<ReservationResponse> Reservations
    );

    public record ReservationsAndTablesResponse(
        List<TableResponse> Tables,
        List<ReservationResponse> Reservations
    );
}
