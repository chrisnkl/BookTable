using BookTable.Database;
using BookTable.Dtos;
using BookTable.Entities;
using BookTable.Patterns.CircuitBreaker.impl;
using BookTable.Patterns.Retry;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BookTable.Services.impl
{
    public class BookService : IBookService
    {
        private readonly DatabaseContext _context;
        private readonly CircuitBreaker _circuitBreaker;
        private readonly RetryPolicy _retryPolicy;
        private readonly IStaticContentService _staticContentService;
        private int attempts;
        public BookService(DatabaseContext context, IStaticContentService staticContentService)
        {
            _context = context;
            _staticContentService = staticContentService;
            _circuitBreaker = new CircuitBreaker();
            _retryPolicy = new RetryPolicy(retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(100));
        }

        #region Table Operations

        public async Task<List<TableResponse>> GetAllTablesAsync()
        {
            List<Table> tables = new();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                attempts++;

                Console.WriteLine($"Current attempt: #{attempts}");

                // Transient failure to simulate retry policy
                if (attempts < _retryPolicy.retryCount)
                {
                    throw new TimeoutException("Simulated transient failure for RetryPolicy");
                }

                _circuitBreaker.ExecuteAction(() =>
                {
                    tables = _context.Tables
                        .Include(t => t.Reservations)
                        .ToList();
                });

                await Task.CompletedTask;
            });

            var result = new List<TableResponse>();

            foreach (var t in tables)
            {
                result.Add(await MapTableResponseAsync(t));
            }

            return result;
        }

        public async Task<TableResponse?> GetTableByIdAsync(int id)
        {
            Table? table = null;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    table = _context.Tables
                        .Include(t => t.Reservations)
                        .FirstOrDefault(t => t.Id == id);
                });
                await Task.CompletedTask;
            });

            return table == null ? null : await MapTableResponseAsync(table);
        }

        public async Task<TableResponse> CreateTableAsync(CreateTableRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Number <= 0)
                throw new ArgumentException("Table number must be positive.", nameof(request.Number));

            if (request.Capacity <= 0)
                throw new ArgumentException("Table capacity must be positive.", nameof(request.Capacity));

            var table = new Table
            {
                Number = request.Number,
                Capacity = request.Capacity,
                BlobName = request.BlobName
            };

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    _context.Tables.Add(table);
                    _context.SaveChanges();
                });
                await Task.CompletedTask;
            });

            return await MapTableResponseAsync(table);
        }

        public async Task<TableResponse?> UploadTableImageAsync(int id, IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (file.Length == 0)
                throw new ArgumentException("Uploaded file is empty.", nameof(file));

            var table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == id);
            if (table == null)
                return null;

            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var safeOriginalName = string.IsNullOrWhiteSpace(originalFileName)
                ? "table"
                : new string(originalFileName.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());

            var blobName = $"{safeOriginalName}{Guid.NewGuid():N}{extension}";

            using var stream = file.OpenReadStream();
            await _staticContentService.UploadFileAsync(
                blobName,
                stream,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

            if (!string.IsNullOrEmpty(table.BlobName) && table.BlobName != blobName)
            {
                await _staticContentService.DeleteBlobAsync(table.BlobName);
            }

            table.BlobName = blobName;
            await _context.SaveChangesAsync();

            return await MapTableResponseAsync(table);
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            bool deleted = false;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    var table = _context.Tables
                        .Include(t => t.Reservations)
                        .FirstOrDefault(t => t.Id == id);

                    if (table != null)
                    {
                        _context.Tables.Remove(table);
                        _context.SaveChanges();
                        deleted = true;
                    }
                });
                await Task.CompletedTask;
            });

            return deleted;
        }

        #endregion

        #region Reservation Operations

        public async Task<List<ReservationResponse>> GetAllReservationsAsync()
        {
            List<Reservation> reservations = new();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    reservations = _context.Reservations
                        .Include(r => r.Table)
                        .ToList();
                });
                await Task.CompletedTask;
            });

            return reservations.Select(MapReservationResponse).ToList();
        }

        public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
        {
            Reservation? reservation = null;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    reservation = _context.Reservations
                        .Include(r => r.Table)
                        .FirstOrDefault(r => r.Id == id);
                });
                await Task.CompletedTask;
            });

            return reservation == null ? null : MapReservationResponse(reservation);
        }

        public async Task<ReservationResponse> BookTableAsync(CreateReservationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.StartTime >= request.EndTime)
                throw new ArgumentException("StartTime must be before EndTime.");

            Reservation reservation = null!;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    var table = _context.Tables
                        .Include(t => t.Reservations)
                        .FirstOrDefault(t => t.Id == request.TableId);

                    if (table == null)
                        throw new KeyNotFoundException($"Table with ID {request.TableId} not found.");

                    bool hasOverlap = table.Reservations.Any(r =>
                        r.StartTime < request.EndTime && r.EndTime > request.StartTime);

                    if (hasOverlap)
                        throw new InvalidOperationException("Table is already reserved for the selected time interval.");

                    reservation = new Reservation
                    {
                        TableId = request.TableId,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        Table = table
                    };

                    _context.Reservations.Add(reservation);
                    _context.SaveChanges();
                });
                await Task.CompletedTask;
            });

            return MapReservationResponse(reservation);
        }

        public async Task<bool> CancelReservationAsync(int id)
        {
            bool cancelled = false;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                _circuitBreaker.ExecuteAction(() =>
                {
                    var reservation = _context.Reservations.Find(id);
                    if (reservation != null)
                    {
                        _context.Reservations.Remove(reservation);
                        _context.SaveChanges();
                        cancelled = true;
                    }
                });
                await Task.CompletedTask;
            });

            return cancelled;
        }

        public async Task<ReservationsAndTablesResponse> GetReservationsAndTablesAsync()
        {
            var tables = await GetAllTablesAsync();
            var reservations = await GetAllReservationsAsync();
            return new ReservationsAndTablesResponse(tables, reservations);
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Maps a Table entity to TableResponse, resolving the floor plan blob URL
        /// from Azure Blob Storage (Static Content Hosting Pattern).
        /// </summary>
        private async Task<TableResponse> MapTableResponseAsync(Table table)
        {
            string? blobUrl = null;
            if (!string.IsNullOrEmpty(table.BlobName))
            {
                blobUrl = await _staticContentService.GetBlobUrlAsync(table.BlobName);
            }

            return new TableResponse(
                table.Id,
                table.Number,
                table.Capacity,
                blobUrl,
                table.Reservations?.Select(MapReservationResponse).ToList() ?? new List<ReservationResponse>()
            );
        }

        private static ReservationResponse MapReservationResponse(Reservation r)
        {
            return new ReservationResponse(
                r.Id,
                r.TableId,
                r.Table?.Number ?? 0,
                r.Table?.Capacity ?? 0,
                r.StartTime,
                r.EndTime
            );
        }

        #endregion
    }
}
