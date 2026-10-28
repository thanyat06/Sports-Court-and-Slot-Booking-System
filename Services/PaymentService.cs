using System;
using System.Collections.Generic;
using System.Text;

using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;

namespace SportsCourtBooking.Services;

public class PaymentService
{
    private readonly PaymentRepository _repository;

    public PaymentService(PaymentRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    // 1. Get payment details for a booking.
    public async Task<Payment?> GetByBookingIdAsync(int bookingId)
    {
        ValidateBookingId(bookingId);

        return await _repository.GetByBookingIdAsync(bookingId);
    }

    // 2. Get all payments.
    public async Task<List<Payment>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // 3. Collect cash for a booking.
    public async Task<bool> CollectCashAsync(
        int bookingId,
        string? notes = null)
    {
        ValidateBookingId(bookingId);

        if (notes is not null)
            notes = notes.Trim();

        return await _repository.CollectCashAsync(bookingId, notes);
    }

    // 4. Get the current payment status.
    public async Task<string?> GetPaymentStatusAsync(int bookingId)
    {
        ValidateBookingId(bookingId);

        Payment? payment =
            await _repository.GetByBookingIdAsync(bookingId);

        return payment?.PaymentStatus;
    }

    // Private validation helper; does not count as a public method.
    private static void ValidateBookingId(int bookingId)
    {
        if (bookingId <= 0)
        {
            throw new ArgumentException(
                "Booking ID must be greater than zero.",
                nameof(bookingId));
        }
    }
}
