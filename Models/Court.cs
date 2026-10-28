using System;
using System.Collections.Generic;
using System.Text;

namespace SportsCourtBooking.Models;

public class Court
{
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; }
}