using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;
using SportsCourtBooking.Helpers;
using SportsCourtBooking.Models;

namespace SportsCourtBooking.Repositories;

public class CourtRepository
{
    public async Task<int> AddAsync(Court court)
    {
        const string sql = """
            INSERT INTO Courts (CourtName, SportType, Location, HourlyRate)
            VALUES (@CourtName, @SportType, @Location, @HourlyRate);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@CourtName", SqlDbType.NVarChar, 100).Value =
            court.CourtName;
        command.Parameters.Add("@SportType", SqlDbType.NVarChar, 20).Value =
            court.SportType;
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 150).Value =
            (object?)court.Location ?? DBNull.Value;
        command.Parameters.Add("@HourlyRate", SqlDbType.Decimal).Value =
            court.HourlyRate;
        command.Parameters["@HourlyRate"].Precision = 10;
        command.Parameters["@HourlyRate"].Scale = 2;

        await connection.OpenAsync();
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}