using System;
using System.Collections.Generic;
using System.Text;

namespace SportsCourtBooking.Models;

public class Slot
{
    public int SlotId { get; set; }
    public int CourtId { get; set; }
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string SlotStatus { get; set; } = "Available";
    public int? BookingId { get; set; }
}