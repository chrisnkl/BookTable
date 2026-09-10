using BookTable.Database;
using BookTable.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookTable.Services
{
    public class BookService : IBookService
    {

        private readonly DatabaseContext _context;
        public BookService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Table>> GetAllReservations()
        {
            return await _context.Tables.ToListAsync();
        }

        public async Task<Table?> GetById(int id)
        {
            return await _context.Tables.FindAsync(id);
        }

        public async Task<Table> Create(Table table)
        {
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return table;
        }

        public async Task<bool> Delete(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null) return false;

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
