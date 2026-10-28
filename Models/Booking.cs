using System;
using System.Collections.Generic;
using System.Text;

namespace SportsCourtBooking.Models;

public class Booking
{
    public int BookingId { get; set; }
    public int CourtId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string BookingStatus { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
}