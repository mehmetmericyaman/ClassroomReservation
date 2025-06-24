using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ceng382ClassroomReservation.Data;
using Ceng382ClassroomReservation.Models;

namespace Ceng382ClassroomReservation.Pages.Admin
{
    public class FeedbacksModel : PageModel
    {
        private readonly AppDbContext _context;

        public FeedbacksModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Feedback> FeedbackList { get; set; } = new();

        public double AverageRating { get; set; } = 0.0;

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToPage("/Login");

            FeedbackList = await _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            if (FeedbackList.Any())
            {
                AverageRating = FeedbackList.Average(f => f.Rating);
            }

            return Page();
        }
    }
}
