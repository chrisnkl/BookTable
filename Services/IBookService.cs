using BookTable.Entities;

namespace BookTable.Services
{
    public interface IBookService
    {
        Task<List<Reservation>> GetAllReservations();
        Task<Reservation?> GetById(int id);
        Task<Reservation> Create(Reservation reservation);
        Task<bool> Delete(int id);
        Task<ReservationResponse> BookTable(CreateReservationRequest request);
    }
}
