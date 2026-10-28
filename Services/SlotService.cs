using System;
using System.Collections.Generic;
using System.Text;
using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;

namespace SportsCourtBooking.Services;

public class SlotService
{
    private readonly SlotRepository _repository;

    public SlotService(SlotRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    // 1. Create an available one-hour slot.
    public async Task<int> CreateAsync(Slot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);

        if (slot.CourtId <= 0)
        {
            throw new ArgumentException(
                "Court ID must be greater than zero.");
        }

        if (slot.SlotDate.Date < DateTime.Today)
        {
            throw new ArgumentException(
                "Slot date cannot be in the past.");
        }

        if (slot.StartTime < TimeSpan.Zero ||
            slot.StartTime >= TimeSpan.FromHours(24) ||
            slot.StartTime.Ticks % TimeSpan.TicksPerHour != 0)
        {
            throw new ArgumentException(
                "Start time must be a valid whole-hour time.");
        }

        if (slot.EndTime != slot.StartTime.Add(TimeSpan.FromHours(1)))
        {
            throw new ArgumentException(
                "A slot must be exactly one hour long.");
        }

        if (slot.SlotStatus != "Available" || slot.BookingId is not null)
        {
            throw new ArgumentException(
                "A newly created slot must be Available and unbooked.");
        }

        return await _repository.CreateAsync(slot);
    }

    // 2. Get all slots.
    public async Task<List<Slot>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // 3. Get a slot by ID.
    public async Task<Slot?> GetByIdAsync(int slotId)
    {
        ValidateSlotId(slotId);

        return await _repository.GetByIdAsync(slotId);
    }

    // 4. Get available slots for a court and date.
    public async Task<List<Slot>> GetAvailableAsync(
        int courtId,
        DateTime slotDate)
    {
        if (courtId <= 0)
        {
            throw new ArgumentException(
                "Court ID must be greater than zero.");
        }

        if (slotDate.Date < DateTime.Today)
        {
            throw new ArgumentException(
                "Slot date cannot be in the past.");
        }

        return await _repository.GetAvailableAsync(courtId, slotDate);
    }

    // Private validation helper.
    private static void ValidateSlotId(int slotId)
    {
        if (slotId <= 0)
        {
            throw new ArgumentException(
                "Slot ID must be greater than zero.");
        }
    }
}
