using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ceng382ClassroomReservation.Data;
using Ceng382ClassroomReservation.Models;
using System.Net.Http;
using System.Text.Json;
using Ceng382ClassroomReservation.Services;

namespace Ceng382ClassroomReservation.Pages.Instructor
{
    public class ReservationsModel : PageModel
    {
        private readonly AppDbContext _context;

        public ReservationsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reservation NewReservation { get; set; } = new();

        public List<Reservation> MyReservations { get; set; } = new();
        public List<Classroom> ClassroomList { get; set; } = new();
        public List<Term> TermList { get; set; } = new();
        public List<DateTime> Holidays { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToPage("/Login");

            NewReservation.UserId = user.Id;
            NewReservation.Status = "Pending";

            var term = await _context.Terms.FirstOrDefaultAsync(t => t.Id == NewReservation.TermId);
            if (term == null)
            {
                ModelState.AddModelError(string.Empty, "Selected term is invalid.");
                await LoadPageDataAsync();
                return Page();
            }

            DateTime dateToCheck;
            if (NewReservation.ReservationType == "Single")
            {
                if (NewReservation.Date == null)
                {
                    ModelState.AddModelError(string.Empty, "Please select a date for single-time reservation.");
                    await LoadPageDataAsync();
                    return Page();
                }
                dateToCheck = NewReservation.Date.Value;
            }
            else
            {
                var baseDate = term.StartDate;
                var selectedDay = (int)NewReservation.DayOfWeek;
                var offset = (selectedDay - (int)baseDate.DayOfWeek + 7) % 7;
                dateToCheck = baseDate.AddDays(offset);
            }

            // Tatil kontrolü
            var apiKey = "AIzaSyDYQEgHoQNPCdvrRZyF4YkwuePiHmW4p8A";
            var calendarId = "tr.turkish%23holiday%40group.v.calendar.google.com";
            var url = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events?key={apiKey}";

            using var httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(result);
            var items = doc.RootElement.GetProperty("items");

            bool isHoliday = false;
            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("start", out var start) &&
                    start.TryGetProperty("date", out var dateElement) &&
                    DateTime.TryParse(dateElement.GetString(), out var holidayDate))
                {
                    if (holidayDate.Date == dateToCheck.Date)
                    {
                        isHoliday = true;
                        break;
                    }
                }
            }

            if (isHoliday)
            {
                if (NewReservation.ReservationType == "Single")
                {
                    ModelState.AddModelError(string.Empty, $"📅 {dateToCheck:dd.MM.yyyy} is a public holiday. Reservation not allowed.");
                    await LoadPageDataAsync();
                    return Page();
                }
                else
                {
                    TempData["HolidayWarning"] = $"⚠️ {dateToCheck:dd.MM.yyyy} is a holiday, but weekly reservations are allowed.";
                }
            }

            if (NewReservation.StartTime >= NewReservation.EndTime)
            {
                ModelState.AddModelError(string.Empty, "⏰ End time must be after start time.");
                await LoadPageDataAsync();
                return Page();
            }

            int currentDayOfWeek = NewReservation.ReservationType == "Weekly"
                ? NewReservation.DayOfWeek
                : (NewReservation.Date.HasValue ? (int)NewReservation.Date.Value.DayOfWeek : -1);

            // 🔐 Instructor çakışma kontrolü
            var instructorReservations = await _context.Reservations
                .Where(r => r.UserId == user.Id &&
                            r.TermId == NewReservation.TermId &&
                            r.Status != "Rejected")
                .ToListAsync();

            foreach (var r in instructorReservations)
            {
                bool sameDay = false;

                if (NewReservation.ReservationType == "Single")
                {
                    if (r.ReservationType == "Single" && r.Date == NewReservation.Date)
                        sameDay = true;
                    else if (r.ReservationType == "Weekly" &&
                            NewReservation.Date.HasValue &&
                            r.DayOfWeek == (int)NewReservation.Date.Value.DayOfWeek)
                        sameDay = true;
                }
                else
                {
                    if (r.ReservationType == "Weekly" && r.DayOfWeek == NewReservation.DayOfWeek)
                        sameDay = true;
                    else if (r.ReservationType == "Single" &&
                            r.Date.HasValue &&
                            (int)r.Date.Value.DayOfWeek == NewReservation.DayOfWeek)
                        sameDay = true;
                }

                if (sameDay)
                {
                    bool timeOverlap =
                        (NewReservation.StartTime >= r.StartTime && NewReservation.StartTime < r.EndTime) ||
                        (NewReservation.EndTime > r.StartTime && NewReservation.EndTime <= r.EndTime) ||
                        (NewReservation.StartTime <= r.StartTime && NewReservation.EndTime >= r.EndTime);

                    if (timeOverlap)
                    {
                        ModelState.AddModelError(string.Empty, "❌ You already have another reservation at this time.");
                        await LoadPageDataAsync();
                        return Page();
                    }
                }
            }

            // 🧱 Classroom çakışma kontrolü
            var reservations = await _context.Reservations
                .Where(r => r.ClassroomId == NewReservation.ClassroomId &&
                            r.TermId == NewReservation.TermId &&
                            r.Status != "Rejected")
                .ToListAsync();

            bool conflict = false;

            foreach (var r in reservations)
            {
                bool sameDay = false;

                if (NewReservation.ReservationType == "Single")
                {
                    if (r.ReservationType == "Single" && r.Date == NewReservation.Date)
                        sameDay = true;
                    else if (r.ReservationType == "Weekly" &&
                            NewReservation.Date.HasValue &&
                            r.DayOfWeek == (int)NewReservation.Date.Value.DayOfWeek)
                        sameDay = true;
                }
                else
                {
                    if (r.ReservationType == "Weekly" && r.DayOfWeek == NewReservation.DayOfWeek)
                        sameDay = true;
                    else if (r.ReservationType == "Single" &&
                            r.Date.HasValue &&
                            (int)r.Date.Value.DayOfWeek == NewReservation.DayOfWeek)
                        sameDay = true;
                }

                if (sameDay)
                {
                    bool timeOverlap =
                        (NewReservation.StartTime >= r.StartTime && NewReservation.StartTime < r.EndTime) ||
                        (NewReservation.EndTime > r.StartTime && NewReservation.EndTime <= r.EndTime) ||
                        (NewReservation.StartTime <= r.StartTime && NewReservation.EndTime >= r.EndTime);

                    if (timeOverlap)
                    {
                        conflict = true;
                        break;
                    }
                }
            }

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "⚠️ There is a conflicting reservation at this time.");
                await LoadPageDataAsync();
                return Page();
            }

            _context.Reservations.Add(NewReservation);
            await _context.SaveChangesAsync();

            var reservationDateInfo = NewReservation.ReservationType == "Single"
                ? NewReservation.Date?.ToString("yyyy-MM-dd")
                : $"Day {NewReservation.DayOfWeek}";

            var logger = new DbLoggerService(_context);
            await logger.LogAsync(
                user.Username,
                $"Created {NewReservation.ReservationType} reservation for Room {NewReservation.ClassroomId} on {reservationDateInfo} from {NewReservation.StartTime} to {NewReservation.EndTime}"
            );

            TempData["SuccessMessage"] = "🎉 Reservation submitted successfully!";
            return RedirectToPage();
        }


        private async Task LoadPageDataAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return;

            ClassroomList = await _context.Classrooms.ToListAsync();
            TermList = await _context.Terms.ToListAsync();

            MyReservations = await _context.Reservations
                .Include(r => r.Classroom)
                .Include(r => r.Term)
                .Where(r => r.UserId == user.Id)
                .OrderBy(r => r.DayOfWeek)
                .ToListAsync();

            Holidays.Clear();
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
    }
}
