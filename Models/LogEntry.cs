using System;

namespace Ceng382ClassroomReservation.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public bool IsError { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
