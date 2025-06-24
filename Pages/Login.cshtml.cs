using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ceng382ClassroomReservation.Data;
using Ceng382ClassroomReservation.Models;
using Ceng382ClassroomReservation.Services;


namespace Ceng382ClassroomReservation.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;

        public LoginModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);

            if (user == null || user.PasswordHash != Password)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            HttpContext.Session.SetString("Username", user.Username);

            var logger = new DbLoggerService(_context);
            await logger.LogAsync(user.Username, "Login success");
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "Admin")
                return RedirectToPage("/Admin/ManageReservations");
            else
                return RedirectToPage("/Instructor/Reservations");
        }

        // 🔽 Feedback form gönderme işlemi
        public async Task<IActionResult> OnPostSendFeedback()
        {
            var category = Request.Form["Category"];
            var message = Request.Form["Message"];
            var username = HttpContext.Session.GetString("Username") ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(string.Empty, "Tüm alanları doldurun.");
                return Page();
            }

            var feedback = new Feedback
            {
                Username = username,
                Category = category,
                Message = message,
                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] = "Mesajınız başarıyla gönderildi!";
            return RedirectToPage();
        }
    }
}
