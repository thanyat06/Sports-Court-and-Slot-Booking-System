
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using SportsCourtBooking.Helpers;
using SportsCourtBooking.Models;

namespace SportsCourtBooking.Repositories;

public class CourtRepository
{
    // 1. Add a court.
    public async Task<int> AddAsync(Court court)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO Courts
                (CourtName, SportType, Location, HourlyRate, IsActive)
            OUTPUT INSERTED.CourtId
            VALUES
                (@CourtName, @SportType, @Location, @HourlyRate, @IsActive);";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add("@CourtName", SqlDbType.NVarChar, 100)
            .Value = court.CourtName;

        command.Parameters.Add("@SportType", SqlDbType.NVarChar, 20)
            .Value = court.SportType;

        command.Parameters.Add("@Location", SqlDbType.NVarChar, 150)
            .Value = string.IsNullOrWhiteSpace(court.Location)
                ? DBNull.Value
                : court.Location.Trim();

        SqlParameter rateParameter =
            command.Parameters.Add("@HourlyRate", SqlDbType.Decimal);
        rateParameter.Precision = 10;
        rateParameter.Scale = 2;
        rateParameter.Value = court.HourlyRate;

        command.Parameters.Add("@IsActive", SqlDbType.Bit)
            .Value = court.IsActive;

        object? result = await command.ExecuteScalarAsync();

        if (result is null || result == DBNull.Value)
        {
            throw new InvalidOperationException(
                "The court could not be added.");
        }

        court.CourtId = Convert.ToInt32(result);
        return court.CourtId;
    }

    // 2. Get all courts.
    public async Task<List<Court>> GetAllAsync()
    {
        List<Court> courts = new();

        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT CourtId, CourtName, SportType, Location,
                   HourlyRate, IsActive
            FROM Courts
            ORDER BY CourtId;";

        using SqlCommand command = new SqlCommand(query, connection);
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            courts.Add(MapCourt(reader));
        }

        return courts;
    }

    // 3. Get a court by ID.
    public async Task<Court?> GetByIdAsync(int courtId)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT CourtId, CourtName, SportType, Location,
                   HourlyRate, IsActive
            FROM Courts
            WHERE CourtId = @CourtId;";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add("@CourtId", SqlDbType.Int).Value = courtId;

        using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapCourt(reader);
        }

        return null;
    }

    // 4. Update court details.
    public async Task<bool> UpdateAsync(Court court)
    {
        using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            UPDATE Courts
            SET CourtName = @CourtName,
                SportType = @SportType,
                Location = @Location,
                HourlyRate = @HourlyRate,
                IsActive = @IsActive
            WHERE CourtId = @CourtId;";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add("@CourtId", SqlDbType.Int).Value =
            court.CourtId;

        command.Parameters.Add("@CourtName", SqlDbType.NVarChar, 100)
            .Value = court.CourtName;

        command.Parameters.Add("@SportType", SqlDbType.NVarChar, 20)
            .Value = court.SportType;

        command.Parameters.Add("@Location", SqlDbType.NVarChar, 150)
            .Value = string.IsNullOrWhiteSpace(court.Location)
                ? DBNull.Value
                : court.Location.Trim();

        SqlParameter rateParameter =
            command.Parameters.Add("@HourlyRate", SqlDbType.Decimal);
        rateParameter.Precision = 10;
        rateParameter.Scale = 2;
        rateParameter.Value = court.HourlyRate;

        command.Parameters.Add("@IsActive", SqlDbType.Bit)
            .Value = court.IsActive;

        return await command.ExecuteNonQueryAsync() == 1;
    }

    // Private helper: convert a database row into a Court object.
    private static Court MapCourt(SqlDataReader reader)
    {
        return new Court
        {
            CourtId = reader.GetInt32(reader.GetOrdinal("CourtId")),
            CourtName = reader.GetString(reader.GetOrdinal("CourtName")),
            SportType = reader.GetString(reader.GetOrdinal("SportType")),
            Location = reader.IsDBNull(reader.GetOrdinal("Location"))
                ? null
                : reader.GetString(reader.GetOrdinal("Location")),
            HourlyRate = reader.GetDecimal(
                reader.GetOrdinal("HourlyRate")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }
}
