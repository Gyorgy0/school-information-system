using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : Controller
{

    [HttpPost]
    public IActionResult Create([FromForm] string username, [FromForm] string password, [FromForm] string role)
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            // Ellen�rizz�k, hogy a felhaszn�l�n�v m�r l�tezik-e
            string checkUserSql = "SELECT COUNT(*) FROM User WHERE Username = @Username";
            using (var checkCmd = new SQLiteCommand(checkUserSql, connection))
            {
                checkCmd.Parameters.AddWithValue("@Username", username);
                long count = (long)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    return Conflict("Username already exists.");
                }
            }

            // Jelsz� hashel�se �s ment�se
            string salt = PasswordManager.GenerateSalt();
            string hashedPassword = PasswordManager.GeneratePasswordHash(password, salt);

            string insertSql = "INSERT INTO User (Username, PasswordHash, PasswordSalt, Role) VALUES (@Username, @PasswordHash, @PasswordSalt, @Role)";
            using (SQLiteCommand cmd = new SQLiteCommand(insertSql, connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("User created successfully");
    }

    [HttpPost]
    public IActionResult Login([FromForm] string username, [FromForm] string password)
    {
        Int64 userID = -1;
        string? role = null;

        // Csak az olvas�sra haszn�ljuk a DB kapcsolatot, majd bez�rjuk
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string selectSql = "SELECT UserID, PasswordHash, PasswordSalt, Role FROM User WHERE Username = @Username";
            using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string? storedPasswordHash = reader["PasswordHash"].ToString();
                        string? storedSalt = reader["PasswordSalt"].ToString();
                        role = reader["Role"].ToString();

                        if (!string.IsNullOrEmpty(storedPasswordHash) && !string.IsNullOrEmpty(storedSalt) &&
                            PasswordManager.Verify(password, storedSalt, storedPasswordHash))
                        {
                            userID = Convert.ToInt64(reader["UserID"]);
                        }
                    }
                }
            }
        }

        if (userID == -1)
            return Unauthorized("Invalid username or password");

        // Itt m�r nincs megnyitva m�sik kapcsolat, mehet az �r�s
        SessionManager.InvalidateAllSessions(userID);
        string sessionCookie = SessionManager.CreateSession(userID);

        Response.Cookies.Append("id", sessionCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return Ok(new { message = "Login successful", role = role });
    }

    [HttpPost]
    public IActionResult Logout()
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId))
        {
            return new UnauthorizedResult();
        }
        SessionManager.InvalidateSession(sessionId);
        return Ok("Logout successful");
    }

    static public bool IsLoggedIn(string SessionCookie)
    {
        Int64 userID = SessionManager.GetUserID(SessionCookie);
        return userID != -1;
    }

    [HttpGet]
    public IActionResult GetUser()
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId))
        {
            return new UnauthorizedResult();
        }
        Int64 userID = SessionManager.GetUserID(sessionId);
        if (userID == -1)
        {
            return new UnauthorizedResult();
        }
        return Json(userID);
    }

    [HttpGet]
    public IActionResult CheckSession()
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId))
        {
            return Json(new { userID = -1 });
        }
        Int64 userID = SessionManager.GetUserID(sessionId);
        return Json(new { userID });
    }

    [HttpGet]
    public IActionResult GetUserList()
    {
        var users = new List<UserDto>();
        try
        {
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                string selectSql = "SELECT UserID, Username FROM User";
                using (var cmd = new SQLiteCommand(selectSql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserDto
                            {
                                UserID = reader.GetInt64(reader.GetOrdinal("UserID")),
                                Username = reader.GetString(reader.GetOrdinal("Username"))
                            });
                        }
                    }
                }
            }
            return Ok(users);
        }
        catch (Exception ex)
        {
            // Log the exception details
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}