using System;
using System.Collections.Generic;
using System.Text;
using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;

namespace SportsCourtBooking.Services;

public class SlotService
{
    private readonly SlotRepository _repository;
    private readonly CourtRepository _courtRepository;

    public SlotService(
        SlotRepository repository,
        CourtRepository courtRepository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));

        _courtRepository = courtRepository
            ?? throw new ArgumentNullException(nameof(courtRepository));
    }

    // 1. Create a single slot
    public async Task<int> CreateAsync(Slot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);

        ValidateSlot(slot);

        var court = await _courtRepository.GetByIdAsync(slot.CourtId);

        if (court is null)
            throw new ArgumentException("Court does not exist.");

        if (!court.IsActive)
            throw new InvalidOperationException(
                "Cannot create slots for an inactive court.");

        slot.SlotStatus = "Available";
        slot.BookingId = null;

        return await _repository.CreateAsync(slot);
    }

    // 2. View all slots
    public async Task<List<Slot>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // 3. Find a slot by ID
    public async Task<Slot?> GetByIdAsync(int slotId)
    {
        if (slotId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(slotId), "Slot ID must be greater than zero.");

        return await _repository.GetByIdAsync(slotId);
    }

    // 4. View slots for a particular court and date
    public async Task<List<Slot>> GetByCourtAndDateAsync(
        int courtId, DateTime slotDate)
    {
        if (courtId <= 0)
            throw new ArgumentOutOfRangeException(nameof(courtId));

        if (slotDate.Date < DateTime.Today)
            throw new ArgumentException("Cannot view slots for a past date.");

        return await _repository.GetByCourtAndDateAsync(
            courtId, slotDate.Date);
    }

    // 5. View only available slots
    public async Task<List<Slot>> GetAvailableAsync(
        int courtId, DateTime slotDate)
    {
        var slots = await GetByCourtAndDateAsync(courtId, slotDate);

        return slots
            .Where(slot => slot.SlotStatus == "Available")
            .OrderBy(slot => slot.StartTime)
            .ToList();
    }

    // 6. Block a slot
    public async Task<bool> BlockAsync(int slotId)
    {
        var slot = await GetRequiredSlotAsync(slotId);

        if (slot.SlotStatus == "Reserved")
            throw new InvalidOperationException(
                "A reserved slot cannot be blocked.");

        if (slot.SlotStatus == "Blocked")
            return true;

        return await _repository.UpdateStatusAsync(slotId, "Blocked");
    }

    // 7. Unblock a slot
    public async Task<bool> UnblockAsync(int slotId)
    {
        var slot = await GetRequiredSlotAsync(slotId);

        if (slot.SlotStatus == "Reserved")
            throw new InvalidOperationException(
                "A reserved slot cannot be unblocked.");

        if (slot.SlotStatus == "Available")
            return true;

        return await _repository.UpdateStatusAsync(slotId, "Available");
    }

    private async Task<Slot> GetRequiredSlotAsync(int slotId)
    {
        if (slotId <= 0)
            throw new ArgumentOutOfRangeException(nameof(slotId));

        var slot = await _repository.GetByIdAsync(slotId);

        return slot ?? throw new InvalidOperationException(
            "Slot was not found.");
    }

    private static void ValidateSlot(Slot slot)
    {
        if (slot.CourtId <= 0)
            throw new ArgumentException("A valid court ID is required.");

        if (slot.SlotDate.Date < DateTime.Today)
            throw new ArgumentException(
                "Cannot create a slot for a past date.");

        if (slot.EndTime <= slot.StartTime)
            throw new ArgumentException(
                "End time must be later than start time.");

        if (slot.StartTime.Minutes != 0 ||
            slot.StartTime.Seconds != 0 ||
            slot.EndTime.Minutes != 0 ||
            slot.EndTime.Seconds != 0)
        {
            throw new ArgumentException(
                "Slots must start and end on whole-hour boundaries.");
        }
    }
}
