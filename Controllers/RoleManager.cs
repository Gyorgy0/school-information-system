using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolAPI.Models;
using System.Data.SQLite;

public class RoleManager
{
    // Role lookup sessionid alapján
    public static string CheckRole(string sessionId)
    {
        int UserID = -1;
        string role = "";
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = $"SELECT UserID From Session WHERE SessionCookie = '{sessionId}'";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UserID = reader.GetInt32(0);
                }
            }
            if (UserID == -1)
            {
                return "";
            }
            sql = $"SELECT Role From User WHERE UserID = '{Convert.ToString(UserID)}'";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    role = reader.GetString(0);
                }
            }
        }
        return role;
    }
}