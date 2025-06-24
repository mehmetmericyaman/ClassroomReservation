using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ceng382ClassroomReservation.Data;
using Ceng382ClassroomReservation.Models;
using Ceng382ClassroomReservation.Helpers;
using Ceng382ClassroomReservation.Services;
using System.Text.Json;

namespace Ceng382ClassroomReservation.Pages.Admin
{
    public class ManageReservationsModel : PageModel
    {
        private readonly AppDbContext _context;

        public ManageReservationsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Reservation> AllReservations { get; set; } = new();
        public List<DateTime> Holidays { get; set; } = new();

        public async Task OnGetAsync()
        {
            AllReservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Term)
                .Include(r => r.Classroom)
                .OrderBy(r => r.Status)
                .ThenBy(r => r.TermId)
                .ToListAsync();

            // 🔹 Google Calendar API üzerinden tatilleri çek
            var apiKey = "AIzaSyDYQEgHoQNPCdvrRZyF4YkwuePiHmW4p8A";
            var calendarId = "tr.turkish%23holiday%40group.v.calendar.google.com";
            var url = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events?key={apiKey}";

            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("start", out var start) &&
                    start.TryGetProperty("date", out var dateEl) &&
                    DateTime.TryParse(dateEl.GetString(), out var holidayDate))
                {
                    Holidays.Add(holidayDate.Date);
                }
            }
        }

        private async Task LoadHolidaysAsync()
        {
            try
            {
                var apiKey = "YOUR_API_KEY_HERE";
                var calendarId = "tr.turkish%23holiday%40group.v.calendar.google.com";
                var url = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events?key={apiKey}";

                using var httpClient = new HttpClient();
                var result = await httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(result);
                var items = doc.RootElement.GetProperty("items");

                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("start", out var start) &&
                        start.TryGetProperty("date", out var dateElement) &&
                        DateTime.TryParse(dateElement.GetString(), out var holidayDate))
                    {
                        Holidays.Add(holidayDate);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tatiller alınamadı: " + ex.Message);
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res != null)
            {
                res.Status = "Approved";
                await _context.SaveChangesAsync();

                // ✅ Log ekle
                var username = HttpContext.Session.GetString("Username") ?? "Unknown";
                var logger = new DbLoggerService(_context);
                await logger.LogAsync(username, $"Approved reservation #{id}");

                // ✅ E-mail gönder
                await EmailService.SendEmailAsync(
                    toEmail: $"{res.User.Username}@gmail.com",
                    subject: "Rezervasyon Onayı",
                    body: $"Merhaba {res.User.Username},\n\nRezervasyonun onaylandı.\nDerslik: {res.Classroom.Name}\nSaat: {res.StartTime} - {res.EndTime}\n\nİyi dersler!"
                );
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res != null)
            {
                res.Status = "Rejected";
                await _context.SaveChangesAsync();

                // ✅ Log ekle
                var username = HttpContext.Session.GetString("Username") ?? "Unknown";
                var logger = new DbLoggerService(_context);
                await logger.LogAsync(username, $"Rejected reservation #{id}");

                // ✅ E-mail gönder
                await EmailService.SendEmailAsync(
                    toEmail: $"{res.User.Username}@gmail.com",
                    subject: "Rezervasyon Reddedildi",
                    body: $"Merhaba {res.User.Username},\n\nRezervasyonun maalesef reddedildi.\nDerslik: {res.Classroom.Name}\nSaat: {res.StartTime} - {res.EndTime}\n\nİyi günler."
                );
            }

            return RedirectToPage();
        }
    }
}
