
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using SportsCourtBooking.Helpers;
using SportsCourtBooking.Models;

namespace SportsCourtBooking.Repositories;

public class PaymentRepository
{
    public async Task<Payment?> GetByBookingIdAsync(int bookingId)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT PaymentId, BookingId, Amount, PaymentMethod,
                   PaymentStatus, CollectedAt, Notes
            FROM Payments
            WHERE BookingId = @BookingId;";

        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.Add("@BookingId", SqlDbType.Int).Value = bookingId;

        using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return MapPayment(reader);

        return null;
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        List<Payment> payments = new();

        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT PaymentId, BookingId, Amount, PaymentMethod,
                   PaymentStatus, CollectedAt, Notes
            FROM Payments
            ORDER BY PaymentId DESC;";

        using SqlCommand command = new SqlCommand(query, connection);
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            payments.Add(MapPayment(reader));

        return payments;
    }

    public async Task<bool> CollectCashAsync(int bookingId, string? notes = null)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        using SqlTransaction transaction =
            connection.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            const string checkQuery = @"
                SELECT p.PaymentStatus, b.BookingStatus
                FROM Payments p WITH (UPDLOCK, HOLDLOCK)
                INNER JOIN Bookings b WITH (UPDLOCK, HOLDLOCK)
                    ON p.BookingId = b.BookingId
                WHERE p.BookingId = @BookingId;";

            string? paymentStatus = null;
            string? bookingStatus = null;

            using (SqlCommand checkCommand =
                   new SqlCommand(checkQuery, connection, transaction))
            {
                checkCommand.Parameters.Add("@BookingId", SqlDbType.Int)
                    .Value = bookingId;

                using SqlDataReader reader =
                    await checkCommand.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    paymentStatus = reader.GetString(0);
                    bookingStatus = reader.GetString(1);
                }
            }

            // Booking or payment does not exist.
            if (paymentStatus is null || bookingStatus is null)
            {
                transaction.Rollback();
                return false;
            }

            // Do not collect payment for a cancelled booking.
            if (bookingStatus == "Cancelled")
            {
                transaction.Rollback();
                return false;
            }

            // Do not collect the same payment twice.
            if (paymentStatus == "Paid")
            {
                transaction.Rollback();
                return false;
            }

            const string updateQuery = @"
                UPDATE Payments
                SET PaymentStatus = 'Paid',
                    PaymentMethod = 'Cash',
                    CollectedAt = SYSDATETIME(),
                    Notes = COALESCE(@Notes, Notes)
                WHERE BookingId = @BookingId
                  AND PaymentStatus = 'Pending';";

            using SqlCommand updateCommand =
                new SqlCommand(updateQuery, connection, transaction);

            updateCommand.Parameters.Add("@BookingId", SqlDbType.Int)
                .Value = bookingId;

            updateCommand.Parameters.Add("@Notes", SqlDbType.NVarChar, -1)
                .Value = string.IsNullOrWhiteSpace(notes)
                    ? DBNull.Value
                    : notes.Trim();

            int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Preserve the original exception if rollback also fails.
            }

            throw;
        }
    }

    private static Payment MapPayment(SqlDataReader reader)
    {
        return new Payment
        {
            PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            PaymentMethod = reader.GetString(
                reader.GetOrdinal("PaymentMethod")),
            PaymentStatus = reader.GetString(
                reader.GetOrdinal("PaymentStatus")),
            CollectedAt = reader.IsDBNull(
                reader.GetOrdinal("CollectedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("CollectedAt")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                ? null
                : reader.GetString(reader.GetOrdinal("Notes"))
        };
    }
}
