
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using SportsCourtBooking.Helpers;
using SportsCourtBooking.Models;

namespace SportsCourtBooking.Repositories;

public class SlotRepository
{
    // 1. Create a new available slot.
    public async Task<int> CreateAsync(Slot slot)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO Slots
                (CourtId, SlotDate, StartTime, EndTime, SlotStatus, BookingId)
            OUTPUT INSERTED.SlotId
            SELECT
                @CourtId, @SlotDate, @StartTime, @EndTime, 'Available', NULL
            WHERE EXISTS
            (
                SELECT 1
                FROM Courts
                WHERE CourtId = @CourtId
                  AND IsActive = 1
            );";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add("@CourtId", SqlDbType.Int).Value =
            slot.CourtId;

        command.Parameters.Add("@SlotDate", SqlDbType.Date).Value =
            slot.SlotDate.Date;

        command.Parameters.Add("@StartTime", SqlDbType.Time).Value =
            slot.StartTime;

        command.Parameters.Add("@EndTime", SqlDbType.Time).Value =
            slot.EndTime;

        object? result = await command.ExecuteScalarAsync();

        if (result is null || result == DBNull.Value)
        {
            throw new InvalidOperationException(
                "The court does not exist or is inactive.");
        }

        slot.SlotId = Convert.ToInt32(result);
        slot.SlotStatus = "Available";
        slot.BookingId = null;

        return slot.SlotId;
    }

    // 2. Retrieve all slots.
    public async Task<List<Slot>> GetAllAsync()
    {
        List<Slot> slots = new();

        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT SlotId, CourtId, SlotDate, StartTime,
                   EndTime, SlotStatus, BookingId
            FROM Slots
            ORDER BY SlotDate, StartTime, CourtId;";

        using SqlCommand command = new SqlCommand(query, connection);
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            slots.Add(MapSlot(reader));
        }

        return slots;
    }

    // 3. Retrieve a slot by its ID.
    public async Task<Slot?> GetByIdAsync(int slotId)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT SlotId, CourtId, SlotDate, StartTime,
                   EndTime, SlotStatus, BookingId
            FROM Slots
            WHERE SlotId = @SlotId;";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add("@SlotId", SqlDbType.Int).Value = slotId;

        using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapSlot(reader);
        }

        return null;
    }

    // 4. Retrieve available slots for a court on a date.
    public async Task<List<Slot>> GetAvailableAsync(
        int courtId,
        DateTime slotDate)
    {
        List<Slot> slots = new();

        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT SlotId, CourtId, SlotDate, StartTime,
                   EndTime, SlotStatus, BookingId
            FROM Slots
            WHERE CourtId = @CourtId
              AND SlotDate = @SlotDate
              AND SlotStatus = 'Available'
              AND BookingId IS NULL
            ORDER BY StartTime;";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add("@CourtId", SqlDbType.Int).Value = courtId;

        command.Parameters.Add("@SlotDate", SqlDbType.Date).Value =
            slotDate.Date;

        using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            slots.Add(MapSlot(reader));
        }

        return slots;
    }

    // Private helper: convert a database row into a Slot object.
    private static Slot MapSlot(SqlDataReader reader)
    {
        return new Slot
        {
            SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
            CourtId = reader.GetInt32(reader.GetOrdinal("CourtId")),
            SlotDate = reader.GetDateTime(reader.GetOrdinal("SlotDate")),
            StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
            EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
            SlotStatus = reader.GetString(reader.GetOrdinal("SlotStatus")),
            BookingId = reader.IsDBNull(reader.GetOrdinal("BookingId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("BookingId"))
        };
    }
}
