using System;
using System.Collections.Generic;
using System.Text;
using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;
using SportsCourtBooking.Services;

var courtRepository = new CourtRepository();
var courtService = new CourtService(courtRepository);

try
{
    var court = new Court
    {
        CourtName = "Green Turf A",
        SportType = "Turf",
        Location = "North Ground",
        HourlyRate = 1500m,
        IsActive = true
    };

    int courtId = await courtService.AddAsync(court);

    Console.WriteLine($"Court created successfully. ID: {courtId}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Operation failed: {ex.Message}");
}