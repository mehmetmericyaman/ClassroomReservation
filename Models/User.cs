namespace Ceng382ClassroomReservation.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Admin veya Instructor

        public ICollection<Reservation> Reservations { get; set; }
    }
}
