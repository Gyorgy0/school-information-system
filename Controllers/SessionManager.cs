using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System;
using System.Data.SQLite;
public class SessionManager
{
    public static string CreateSession(Int64 UserID)
    {
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string sessionCookie;

            do
            {
                sessionCookie = Guid.NewGuid().ToString();
            } while (SessionCookieExists(sessionCookie, connection));

            Int64 validUntil = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10;

            string insertSql = "INSERT INTO Session (SessionCookie, UserID, ValidUntil, LoginTime) VALUES (@SessionCookie, @UserID, @ValidUntil, @LoginTime)";
            using (SQLiteCommand cmd = new SQLiteCommand(insertSql, connection))
            {
                cmd.Parameters.AddWithValue("@SessionCookie", sessionCookie);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@ValidUntil", validUntil);
                cmd.Parameters.AddWithValue("@LoginTime", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                cmd.ExecuteNonQuery();
            }
            return sessionCookie;
        }
    }

    private static bool SessionCookieExists(string sessionCookie, SQLiteConnection connection)
    {
        string selectSql = "SELECT COUNT(*) FROM Session WHERE SessionCookie = @SessionCookie";

        using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
        {
            cmd.Parameters.AddWithValue("@SessionCookie", sessionCookie);

            try
            {
                object result = cmd.ExecuteScalar();
                if (result != null && Convert.ToInt64(result) > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex.Message);
                // Log the exception for further analysis
            }
        }
        return false;
    }

    public static string InvalidateAllSessions(Int64 UserID)
    {
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string deleteSql = "DELETE FROM Session WHERE UserID = @UserID";
            using (SQLiteCommand cmd = new SQLiteCommand(deleteSql, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.ExecuteNonQuery();
            }
        }
        return "All sessions invalidated for UserID: " + UserID;
    }

    public static string InvalidateSession(string SessionCookie)
    {
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string deleteSql = "DELETE FROM Session WHERE SessionCookie = @SessionCookie";
            using (SQLiteCommand cmd = new SQLiteCommand(deleteSql, connection))
            {
                cmd.Parameters.AddWithValue("@SessionCookie", SessionCookie);
                cmd.ExecuteNonQuery();
            }
        }
        return "Session invalidated for SessionCookie: " + SessionCookie;
    }

    public static Int64 GetUserID(string? SessionCookie)
    {
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string selectSql = "SELECT UserID FROM Session WHERE SessionCookie = @SessionCookie AND ValidUntil > @CurrentTime";

            using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
            {
                cmd.Parameters.AddWithValue("@SessionCookie", SessionCookie);
                cmd.Parameters.AddWithValue("@CurrentTime", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                try
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        return Convert.ToInt64(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occurred: " + ex.Message);
                    // Log the exception for further analysis
                }
            }
        }
        return -1;
    }

    public static Int64 ValidateSession(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            throw new UnauthorizedAccessException("No session cookie");

        Int64 userID = SessionManager.GetUserID(sessionId);
        if (userID == -1)
            throw new UnauthorizedAccessException("Session invalid or expired");

        return userID;
    }
}
/*public class SessionValidation(ILogger<PeriodicBackgroundTask> logger, TimeProvider timeProvider) 
    : BackgroundService
{
    private readonly ILogger<PeriodicBackgroundTask> _logger = logger;
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken))
        {
            
        }
    }
}
*/