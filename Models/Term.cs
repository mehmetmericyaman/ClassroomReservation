namespace Ceng382ClassroomReservation.Models
{
    public class Term
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
    }
}
