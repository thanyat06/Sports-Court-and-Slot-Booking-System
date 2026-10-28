using System;
using System.Collections.Generic;
using System.Text;
using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;

namespace SportsCourtBooking.Services;

public class CourtService
{
    private readonly CourtRepository _repository;

    public CourtService(CourtRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    // 1. Add a court.
    public async Task<int> AddAsync(Court court)
    {
        ArgumentNullException.ThrowIfNull(court);

        court.CourtName = court.CourtName.Trim();
        court.SportType = court.SportType.Trim();

        if (string.IsNullOrWhiteSpace(court.CourtName))
        {
            throw new ArgumentException("Court name is required.");
        }

        if (court.SportType is not ("Turf" or "Badminton"))
        {
            throw new ArgumentException(
                "Sport type must be Turf or Badminton.");
        }

        if (court.HourlyRate < 0)
        {
            throw new ArgumentException(
                "Hourly rate cannot be negative.");
        }

        return await _repository.AddAsync(court);
    }

    // 2. Get all courts.
    public async Task<List<Court>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // 3. Get a court by ID.
    public async Task<Court?> GetByIdAsync(int courtId)
    {
        ValidateCourtId(courtId);

        return await _repository.GetByIdAsync(courtId);
    }

    // 4. Update a court.
    public async Task<bool> UpdateAsync(Court court)
    {
        ArgumentNullException.ThrowIfNull(court);
        ValidateCourtId(court.CourtId);

        court.CourtName = court.CourtName.Trim();
        court.SportType = court.SportType.Trim();

        if (string.IsNullOrWhiteSpace(court.CourtName))
        {
            throw new ArgumentException("Court name is required.");
        }

        if (court.SportType is not ("Turf" or "Badminton"))
        {
            throw new ArgumentException(
                "Sport type must be Turf or Badminton.");
        }

        if (court.HourlyRate < 0)
        {
            throw new ArgumentException(
                "Hourly rate cannot be negative.");
        }

        return await _repository.UpdateAsync(court);
    }

    // Private helper; doesn't count toward the four public methods.
    private static void ValidateCourtId(int courtId)
    {
        if (courtId <= 0)
        {
            throw new ArgumentException(
                "Court ID must be greater than zero.");
        }
    }
}
