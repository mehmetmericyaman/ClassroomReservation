using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ceng382ClassroomReservation.Models;
using Ceng382ClassroomReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace Ceng382ClassroomReservation.Pages.Admin
{
    public class TermsModel : PageModel
    {
        private readonly AppDbContext _context;

        public TermsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Term NewTerm { get; set; } = new();

        public List<Term> TermList { get; set; } = new();

        public async Task OnGetAsync()
        {
            TermList = await _context.Terms.OrderByDescending(t => t.StartDate).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TermList = await _context.Terms.OrderByDescending(t => t.StartDate).ToListAsync();
                return Page();
            }

            _context.Terms.Add(NewTerm);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
