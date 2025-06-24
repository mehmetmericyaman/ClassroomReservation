using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ceng382ClassroomReservation.Data;
using Ceng382ClassroomReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace Ceng382ClassroomReservation.Pages.Admin
{
    public class ClassroomsModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClassroomsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Classroom NewClassroom { get; set; } = new();

        public List<Classroom> ClassroomList { get; set; } = new();

        public async Task OnGetAsync()
        {
            ClassroomList = await _context.Classrooms.OrderBy(c => c.Name).ToListAsync();
        }

       public async Task<IActionResult> OnPostAsync()
{
    Console.WriteLine(">> POST tetiklendi");
    Console.WriteLine($"Classroom: {NewClassroom?.Name} / {NewClassroom?.Capacity}");

    if (!ModelState.IsValid)
    {
        Console.WriteLine(">> ModelState geçersiz");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine("Model error: " + error.ErrorMessage);
        }

        ClassroomList = await _context.Classrooms.ToListAsync();
        return Page();
    }

    _context.Classrooms.Add(NewClassroom);
    await _context.SaveChangesAsync();
    Console.WriteLine(">> Kayıt eklendi");

    return RedirectToPage();
}

    }
}
