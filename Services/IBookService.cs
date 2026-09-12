using BookTable.Dtos;

namespace BookTable.Services
{
    public interface IBookService
    {
        Task<List<TableResponse>> GetAllTablesAsync();
        Task<TableResponse?> GetTableByIdAsync(int id);
        Task<TableResponse> CreateTableAsync(CreateTableRequest request);
        Task<TableResponse?> UploadTableImageAsync(int id, IFormFile file);
        Task<bool> DeleteTableAsync(int id);


        Task<List<ReservationResponse>> GetAllReservationsAsync();
        Task<ReservationResponse?> GetReservationByIdAsync(int id);
        Task<ReservationResponse> BookTableAsync(CreateReservationRequest request);
        Task<bool> CancelReservationAsync(int id);

        Task<ReservationsAndTablesResponse> GetReservationsAndTablesAsync();
    }
}
