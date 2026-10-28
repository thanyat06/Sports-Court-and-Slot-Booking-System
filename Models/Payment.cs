using System;
using System.Collections.Generic;
using System.Text;

namespace SportsCourtBooking.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string PaymentStatus { get; set; } = "Pending";
    public DateTime? CollectedAt { get; set; }
    public string? Notes { get; set; }
}