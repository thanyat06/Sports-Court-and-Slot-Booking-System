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

    // Create a booking after validating customer details and duration.
    public async Task<int> CreateAsync(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        ValidateBooking(booking);

        booking.CustomerName = booking.CustomerName.Trim();
        booking.CustomerPhone = booking.CustomerPhone.Trim();

        return await _repository.CreateBookingAsync(booking);
    }

    // Retrieve one booking.
    public async Task<Booking?> GetByIdAsync(int bookingId)
    {
        if (bookingId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(bookingId), "Booking ID must be greater than zero.");

        return await _repository.GetByIdAsync(bookingId);
    }

    // Retrieve all bookings.
    public async Task<List<Booking>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    private static void ValidateBooking(Booking booking)
    {
        if (booking.CourtId <= 0)
            throw new ArgumentException("Select a valid court.");

        if (string.IsNullOrWhiteSpace(booking.CustomerName))
            throw new ArgumentException("Customer name is required.");

        if (booking.CustomerName.Trim().Length > 100)
            throw new ArgumentException(
                "Customer name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(booking.CustomerPhone))
            throw new ArgumentException("Customer phone is required.");

        if (booking.CustomerPhone.Trim().Length > 25)
            throw new ArgumentException(
                "Customer phone cannot exceed 25 characters.");

        if (booking.BookingDate.Date < DateTime.Today)
            throw new ArgumentException(
                "Booking date cannot be in the past.");

        if (booking.StartTime < TimeSpan.Zero ||
            booking.StartTime >= TimeSpan.FromHours(24) ||
            booking.EndTime > TimeSpan.FromHours(24) ||
            booking.EndTime <= booking.StartTime)
        {
            throw new ArgumentException(
                "Enter a valid booking time.");
        }

        if (booking.StartTime.Ticks % TimeSpan.TicksPerHour != 0 ||
            booking.EndTime.Ticks % TimeSpan.TicksPerHour != 0)
        {
            throw new ArgumentException(
                "Booking times must be on whole-hour boundaries.");
        }

        int expectedSlotCount =
            (int)((booking.EndTime - booking.StartTime).TotalHours);

        if (booking.SlotCount != expectedSlotCount)
            throw new ArgumentException(
                "Slot count does not match the booking duration.");
    }


    public async Task<bool> ConfirmAsync(int bookingId)
    {
        ValidateBookingId(bookingId);
        return await _repository.ConfirmBookingAsync(bookingId);
    }

    public async Task<bool> CancelAsync(int bookingId)
    {
        ValidateBookingId(bookingId);
        return await _repository.CancelBookingAsync(bookingId);
    }

    private static void ValidateBookingId(int bookingId)
    {
        if (bookingId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(bookingId), "Booking ID must be greater than zero.");
    }

}
