using System.ComponentModel.DataAnnotations;

namespace Ceng382ClassroomReservation.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public int TermId { get; set; }
        public Term Term { get; set; }

        public int DayOfWeek { get; set; } // 1 = Pazartesi
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected

        [Required]
        public string ReservationType { get; set; } = "Weekly"; // varsayılan: Sezonluk

        public DateTime? Date { get; set; } // Nullable çünkü sadece Single için geçerli
        


    }
}
