using System.Threading.Tasks;
using Ceng382ClassroomReservation.Data;
using Ceng382ClassroomReservation.Models;

namespace Ceng382ClassroomReservation.Services
{
    public class DbLoggerService
    {
        private readonly AppDbContext _context;

        public DbLoggerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string username, string action, bool isError = false)
        {
            var log = new LogEntry
            {
                Username = username,
                Action = action,
                IsError = isError,
                Timestamp = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
