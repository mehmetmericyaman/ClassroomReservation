using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ceng382ClassroomReservation.Models;
using Ceng382ClassroomReservation.Data;
using System.Collections.Generic;
using System.Linq;

namespace Ceng382ClassroomReservation.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly AppDbContext _context;

        public UsersModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InstructorInputModel Input { get; set; } = new();

        public List<User> InstructorList { get; set; } = new();

        public void OnGet()
        {
            InstructorList = _context.Users
                .Where(u => u.Role == "Instructor")
                .ToList();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            if (_context.Users.Any(u => u.Username == Input.Username))
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                OnGet();
                return Page();
            }

            var user = new User
            {
                Username = Input.Username,
                PasswordHash = Input.Password,
                Role = "Instructor"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "Instructor");
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        public class InstructorInputModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
