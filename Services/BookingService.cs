using System;
using System.Collections.Generic;
using System.Text;

using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;

namespace SportsCourtBooking.Services;

public class BookingService
{
    private readonly BookingRepository _repository;

    public BookingService(BookingRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    // 1. Create a booking
    public async Task<int> CreateAsync(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        ValidateBooking(booking);

        booking.CustomerName = booking.CustomerName.Trim();
        booking.CustomerPhone = booking.CustomerPhone.Trim();

        return await _repository.CreateBookingAsync(booking);
    }

    // 2. Get all bookings
    public async Task<List<Booking>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // 3. Get a booking by ID
    public async Task<Booking?> GetByIdAsync(int bookingId)
    {
        ValidateBookingId(bookingId);
        return await _repository.GetByIdAsync(bookingId);
    }

    // 4. Cancel a booking
    public async Task<bool> CancelAsync(int bookingId)
    {
        ValidateBookingId(bookingId);
        return await _repository.CancelBookingAsync(bookingId);
    }

    private static void ValidateBooking(Booking booking)
    {
        if (booking.CourtId <= 0)
            throw new ArgumentException("Select a valid court.");

        if (string.IsNullOrWhiteSpace(booking.CustomerName) ||
            booking.CustomerName.Trim().Length > 100)
            throw new ArgumentException(
                "Customer name is required and must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(booking.CustomerPhone) ||
            booking.CustomerPhone.Trim().Length > 25)
            throw new ArgumentException(
                "Customer phone is required and must not exceed 25 characters.");

        if (booking.BookingDate.Date < DateTime.Today)
            throw new ArgumentException(
                "Booking date cannot be in the past.");

        if (booking.StartTime < TimeSpan.Zero ||
            booking.StartTime >= TimeSpan.FromHours(24) ||
            booking.EndTime > TimeSpan.FromHours(24) ||
            booking.EndTime <= booking.StartTime)
            throw new ArgumentException("Invalid booking time.");

        if (booking.StartTime.Ticks % TimeSpan.TicksPerHour != 0 ||
            booking.EndTime.Ticks % TimeSpan.TicksPerHour != 0)
            throw new ArgumentException(
                "Booking times must be on whole-hour boundaries.");

        int expectedSlotCount =
            (int)((booking.EndTime - booking.StartTime).TotalHours);

        if (booking.SlotCount != expectedSlotCount)
            throw new ArgumentException(
                "Slot count does not match the booking duration.");
    }

    private static void ValidateBookingId(int bookingId)
    {
        if (bookingId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(bookingId), "Booking ID must be greater than zero.");
    }
}
