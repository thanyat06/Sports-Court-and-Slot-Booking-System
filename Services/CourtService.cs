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

    // 1. Add a new court
    public async Task<int> AddAsync(Court court)
    {
        ArgumentNullException.ThrowIfNull(court);

        ValidateCourt(court);

        court.CourtName = court.CourtName.Trim();
        court.SportType = NormalizeSportType(court.SportType);

        return await _repository.AddAsync(court);
    }

    // 2. List all courts
    public async Task<List<Court>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // 3. Search for a court by ID
    public async Task<Court?> GetByIdAsync(int courtId)
    {
        ValidateCourtId(courtId);

        return await _repository.GetByIdAsync(courtId);
    }

    // 4. Search courts by name
    public async Task<List<Court>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException(
                "Search term cannot be empty.",
                nameof(searchTerm));
        }

        return await _repository.SearchAsync(searchTerm.Trim());
    }

    // 5. Update court details and hourly rate
    public async Task<bool> UpdateAsync(Court court)
    {
        ArgumentNullException.ThrowIfNull(court);

        ValidateCourtId(court.CourtId);
        ValidateCourt(court);

        Court? existingCourt =
            await _repository.GetByIdAsync(court.CourtId);

        if (existingCourt is null)
        {
            return false;
        }

        court.CourtName = court.CourtName.Trim();
        court.SportType = NormalizeSportType(court.SportType);

        return await _repository.UpdateAsync(court);
    }

    // 6. Activate a court
    public async Task<bool> ActivateAsync(int courtId)
    {
        ValidateCourtId(courtId);

        Court? court = await _repository.GetByIdAsync(courtId);

        if (court is null)
        {
            return false;
        }

        if (court.IsActive)
        {
            return true;
        }

        return await _repository.SetActiveAsync(courtId, true);
    }

    // 7. Deactivate a court instead of deleting it
    public async Task<bool> DeactivateAsync(int courtId)
    {
        ValidateCourtId(courtId);

        Court? court = await _repository.GetByIdAsync(courtId);

        if (court is null)
        {
            return false;
        }

        if (!court.IsActive)
        {
            return true;
        }

        return await _repository.SetActiveAsync(courtId, false);
    }

    // Shared validation for adding and updating courts
    private static void ValidateCourt(Court court)
    {
        if (string.IsNullOrWhiteSpace(court.CourtName))
        {
            throw new ArgumentException(
                "Court name cannot be empty.");
        }

        if (court.HourlyRate < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(court.HourlyRate),
                "Hourly rate cannot be negative.");
        }

        NormalizeSportType(court.SportType);
    }

    private static string NormalizeSportType(string? sportType)
    {
        if (string.Equals(
            sportType?.Trim(), "Turf",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Turf";
        }

        if (string.Equals(
            sportType?.Trim(), "Badminton",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Badminton";
        }

        throw new ArgumentException(
            "Sport type must be either Turf or Badminton.",
            nameof(sportType));
    }

    private static void ValidateCourtId(int courtId)
    {
        if (courtId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(courtId),
                "Court ID must be greater than zero.");
        }
    }
}
