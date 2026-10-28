using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using SportsCourtBooking.Helpers;
using SportsCourtBooking.Models;

namespace SportsCourtBooking.Repositories;

public class BookingRepository
{

    // 1. Get one booking by its ID
    public async Task<Booking?> GetByIdAsync(int bookingId)
    {
        const string sql = """
            SELECT BookingId, CourtId, CustomerName, CustomerPhone,
                   BookingDate, StartTime, EndTime, SlotCount,
                   TotalAmount, BookingStatus, CreatedAt
            FROM Bookings
            WHERE BookingId = @BookingId;
            """;

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@BookingId", bookingId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapBooking(reader);
    }

    // 2. Get all bookings
    public async Task<List<Booking>> GetAllAsync()
    {
        const string sql = """
            SELECT BookingId, CourtId, CustomerName, CustomerPhone,
                   BookingDate, StartTime, EndTime, SlotCount,
                   TotalAmount, BookingStatus, CreatedAt
            FROM Bookings
            ORDER BY BookingDate DESC, StartTime;
            """;

        var bookings = new List<Booking>();

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            bookings.Add(MapBooking(reader));
        }

        return bookings;
    }

    // 3. Convert a database row into a Booking object
    private static Booking MapBooking(SqlDataReader reader)
    {
        return new Booking
        {
            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
            CourtId = reader.GetInt32(reader.GetOrdinal("CourtId")),
            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
            CustomerPhone = reader.GetString(reader.GetOrdinal("CustomerPhone")),
            BookingDate = reader.GetDateTime(reader.GetOrdinal("BookingDate")),
            StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
            EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
            SlotCount = reader.GetInt32(reader.GetOrdinal("SlotCount")),
            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
            BookingStatus = reader.GetString(reader.GetOrdinal("BookingStatus")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }


    public async Task<int> CreateBookingAsync(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.CourtId <= 0)
            throw new ArgumentException("Invalid court ID.");

        if (string.IsNullOrWhiteSpace(booking.CustomerName))
            throw new ArgumentException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(booking.CustomerPhone))
            throw new ArgumentException("Customer phone is required.");

        if (booking.BookingDate.Date < DateTime.Today)
            throw new ArgumentException("Booking date cannot be in the past.");

        if (booking.StartTime < TimeSpan.Zero ||
            booking.StartTime >= TimeSpan.FromHours(24) ||
            booking.EndTime > TimeSpan.FromHours(24) ||
            booking.EndTime <= booking.StartTime)
        {
            throw new ArgumentException("Invalid booking time.");
        }

        if (booking.StartTime.Ticks % TimeSpan.TicksPerHour != 0 ||
            booking.EndTime.Ticks % TimeSpan.TicksPerHour != 0)
        {
            throw new ArgumentException(
                "Bookings must start and end on whole-hour boundaries.");
        }

        int expectedSlotCount =
            (int)((booking.EndTime - booking.StartTime).TotalHours);

        if (booking.SlotCount != expectedSlotCount)
            throw new ArgumentException(
                "Slot count must match the booking duration.");

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            // 1. Verify that the court exists and is active.
            const string courtSql = """
            SELECT HourlyRate
            FROM Courts WITH (UPDLOCK, HOLDLOCK)
            WHERE CourtId = @CourtId AND IsActive = 1;
            """;

            decimal hourlyRate;

            await using (var courtCommand =
                new SqlCommand(courtSql, connection, transaction))
            {
                courtCommand.Parameters.Add(
                    "@CourtId", SqlDbType.Int).Value = booking.CourtId;

                object? result = await courtCommand.ExecuteScalarAsync();

                if (result is null || result == DBNull.Value)
                    throw new InvalidOperationException(
                        "Court does not exist or is inactive.");

                hourlyRate = Convert.ToDecimal(result);
            }

            // 2. Lock and read the requested slots.
            const string slotsSql = """
            SELECT SlotId, StartTime, EndTime, SlotStatus, BookingId
            FROM Slots WITH (UPDLOCK, HOLDLOCK)
            WHERE CourtId = @CourtId
              AND SlotDate = @BookingDate
              AND StartTime >= @StartTime
              AND StartTime < @EndTime
            ORDER BY StartTime;
            """;

            var slotIds = new List<int>();
            TimeSpan expectedStart = booking.StartTime;

            await using (var slotsCommand =
                new SqlCommand(slotsSql, connection, transaction))
            {
                slotsCommand.Parameters.Add(
                    "@CourtId", SqlDbType.Int).Value = booking.CourtId;

                slotsCommand.Parameters.Add(
                    "@BookingDate", SqlDbType.Date).Value =
                    booking.BookingDate.Date;

                slotsCommand.Parameters.Add(
                    "@StartTime", SqlDbType.Time).Value = booking.StartTime;

                slotsCommand.Parameters.Add(
                    "@EndTime", SqlDbType.Time).Value = booking.EndTime;

                await using var reader =
                    await slotsCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    int slotId = reader.GetInt32(0);
                    TimeSpan slotStart = reader.GetTimeSpan(1);
                    TimeSpan slotEnd = reader.GetTimeSpan(2);
                    string status = reader.GetString(3);

                    if (status != "Available" ||
                        !reader.IsDBNull(4))
                    {
                        throw new InvalidOperationException(
                            "One or more requested slots are unavailable.");
                    }

                    if (slotStart != expectedStart ||
                        slotEnd - slotStart != TimeSpan.FromHours(1))
                    {
                        throw new InvalidOperationException(
                            "Requested slots must be consecutive one-hour slots.");
                    }

                    slotIds.Add(slotId);
                    expectedStart = slotEnd;
                }
            }

            if (slotIds.Count != booking.SlotCount ||
                expectedStart != booking.EndTime)
            {
                throw new InvalidOperationException(
                    "The requested slots are missing or not consecutive.");
            }

            // 3. Calculate the amount using the database court rate.
            decimal totalAmount = hourlyRate * booking.SlotCount;

            // 4. Insert the booking and retrieve its generated ID.
            const string bookingSql = """
            INSERT INTO Bookings
                (CourtId, CustomerName, CustomerPhone, BookingDate,
                 StartTime, EndTime, SlotCount, TotalAmount, BookingStatus)
            OUTPUT INSERTED.BookingId
            VALUES
                (@CourtId, @CustomerName, @CustomerPhone, @BookingDate,
                 @StartTime, @EndTime, @SlotCount, @TotalAmount, N'Pending');
            """;

            int bookingId;

            await using (var bookingCommand =
                new SqlCommand(bookingSql, connection, transaction))
            {
                bookingCommand.Parameters.Add(
                    "@CourtId", SqlDbType.Int).Value = booking.CourtId;

                bookingCommand.Parameters.Add(
                    "@CustomerName", SqlDbType.NVarChar, 100).Value =
                    booking.CustomerName.Trim();

                bookingCommand.Parameters.Add(
                    "@CustomerPhone", SqlDbType.NVarChar, 25).Value =
                    booking.CustomerPhone.Trim();

                bookingCommand.Parameters.Add(
                    "@BookingDate", SqlDbType.Date).Value =
                    booking.BookingDate.Date;

                bookingCommand.Parameters.Add(
                    "@StartTime", SqlDbType.Time).Value = booking.StartTime;

                bookingCommand.Parameters.Add(
                    "@EndTime", SqlDbType.Time).Value = booking.EndTime;

                bookingCommand.Parameters.Add(
                    "@SlotCount", SqlDbType.Int).Value = booking.SlotCount;

                var amountParameter = bookingCommand.Parameters.Add(
                    "@TotalAmount", SqlDbType.Decimal);

                amountParameter.Precision = 10;
                amountParameter.Scale = 2;
                amountParameter.Value = totalAmount;

                bookingId = Convert.ToInt32(
                    await bookingCommand.ExecuteScalarAsync());
            }

            // 5. Reserve exactly the slots we checked.
            const string reserveSql = """
            UPDATE Slots
            SET SlotStatus = N'Reserved', BookingId = @BookingId
            WHERE SlotId = @SlotId
              AND SlotStatus = N'Available'
              AND BookingId IS NULL;
            """;

            foreach (int slotId in slotIds)
            {
                await using var reserveCommand =
                    new SqlCommand(reserveSql, connection, transaction);

                reserveCommand.Parameters.Add(
                    "@BookingId", SqlDbType.Int).Value = bookingId;

                reserveCommand.Parameters.Add(
                    "@SlotId", SqlDbType.Int).Value = slotId;

                int rowsAffected = await reserveCommand.ExecuteNonQueryAsync();

                if (rowsAffected != 1)
                    throw new InvalidOperationException(
                        "A slot could not be reserved. Booking cancelled.");
            }

            // 6. Create the pending cash-payment record.
            const string paymentSql = """
            INSERT INTO Payments
                (BookingId, Amount, PaymentMethod, PaymentStatus)
            VALUES
                (@BookingId, @Amount, N'Cash', N'Pending');
            """;

            await using (var paymentCommand =
                new SqlCommand(paymentSql, connection, transaction))
            {
                paymentCommand.Parameters.Add(
                    "@BookingId", SqlDbType.Int).Value = bookingId;

                var paymentAmount = paymentCommand.Parameters.Add(
                    "@Amount", SqlDbType.Decimal);

                paymentAmount.Precision = 10;
                paymentAmount.Scale = 2;
                paymentAmount.Value = totalAmount;

                await paymentCommand.ExecuteNonQueryAsync();
            }

            // 7. Commit only after every operation succeeds.
            await transaction.CommitAsync();

            booking.BookingId = bookingId;
            booking.TotalAmount = totalAmount;
            booking.BookingStatus = "Pending";

            return bookingId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        using SqlTransaction transaction =
            connection.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            const string checkQuery = @"
            SELECT b.BookingStatus, p.PaymentStatus
            FROM Bookings b WITH (UPDLOCK, HOLDLOCK)
            INNER JOIN Payments p WITH (UPDLOCK, HOLDLOCK)
                ON b.BookingId = p.BookingId
            WHERE b.BookingId = @BookingId;";

            string? bookingStatus = null;
            string? paymentStatus = null;

            using (SqlCommand command =
                   new SqlCommand(checkQuery, connection, transaction))
            {
                command.Parameters.Add("@BookingId", SqlDbType.Int).Value =
                    bookingId;

                using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    bookingStatus = reader.GetString(0);
                    paymentStatus = reader.GetString(1);
                }
            }

            // Booking or its payment record does not exist.
            if (bookingStatus is null || paymentStatus is null)
            {
                transaction.Rollback();
                return false;
            }

            // Do not cancel an already cancelled booking.
            if (bookingStatus == "Cancelled")
            {
                transaction.Rollback();
                return false;
            }

            // Never cancel a booking after payment has been collected.
            if (paymentStatus == "Paid")
            {
                transaction.Rollback();
                return false;
            }

            const string cancelQuery = @"
            UPDATE Bookings
            SET BookingStatus = 'Cancelled'
            WHERE BookingId = @BookingId
              AND BookingStatus IN ('Pending', 'Confirmed');";

            using (SqlCommand command =
                   new SqlCommand(cancelQuery, connection, transaction))
            {
                command.Parameters.Add("@BookingId", SqlDbType.Int).Value =
                    bookingId;

                if (await command.ExecuteNonQueryAsync() != 1)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            const string releaseSlotsQuery = @"
            UPDATE Slots
            SET SlotStatus = 'Available',
                BookingId = NULL
            WHERE BookingId = @BookingId
              AND SlotStatus = 'Reserved';";

            using (SqlCommand command =
                   new SqlCommand(releaseSlotsQuery, connection, transaction))
            {
                command.Parameters.Add("@BookingId", SqlDbType.Int).Value =
                    bookingId;

                await command.ExecuteNonQueryAsync();
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
                // Preserve the original exception.
            }

            throw;
        }
    }


}
