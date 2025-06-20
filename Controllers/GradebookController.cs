using System.Data.SQLite;
using Microsoft.AspNetCore.Mvc;

namespace SchoolAPI.Controllers
{
    public class GradeEntry
    {
        public int GradeID { get; set; }
        public int UserID { get; set; }
        public string? Subject { get; set; }
        public int GradeValue { get; set; }
        public string? Date { get; set; }
    }

    public class AbsenceEntry
    {
        public int AbsenceID { get; set; }
        public int UserID { get; set; }
        public string? Subject { get; set; }
        public string? Date { get; set; }
        public bool IsExcused { get; set; }
    }

    public class AddGradeDto
    {
        public int UserID { get; set; }
        public string? Subject { get; set; }
        public int GradeValue { get; set; }
        public string? Date { get; set; }
    }

    public class DeleteGradeDto
    {
        public int GradeID { get; set; }
    }

    public class CloseSubjectDto
    {
        public int UserID { get; set; }
        public string? Subject { get; set; }
    }

    public class AddAbsenceDto
    {
        public int UserID { get; set; }
        public string? Subject { get; set; }
        public string? Date { get; set; }
    }

    public class ExcuseAbsenceDto
    {
        public int UserID { get; set; }
        public string? Date { get; set; }
    }

    public class DeleteAbsenceDto
    {
        public int UserID { get; set; }
        public string? Date { get; set; }
    }

    [ApiController]
    [Route("[controller]/[action]")]
    public class GradeController : Controller
    {
        // GET /Grade/GetGrades?userId=xx
        [HttpGet]
        public IActionResult GetGrades(int userId)
        {
            try
            {
                var grades = new List<GradeEntry>();
                string sql =
                    "SELECT GradeID, UserID, Subject, GradeValue, Date FROM Grade WHERE UserID = @UserID";

                using (var connection = DatabaseConnector.CreateNewConnection())
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            grades.Add(
                                new GradeEntry
                                {
                                    GradeID = reader.GetInt32(0),
                                    UserID = reader.GetInt32(1),
                                    Subject = reader.GetString(2),
                                    GradeValue = reader.GetInt32(3),
                                    Date = reader.GetString(4),
                                }
                            );
                        }
                    }
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hiba történt: " + ex.Message);
            }
        }

        // POST /Grade/AddGrade (JSON body)
        [HttpPost]
        public IActionResult AddGrade([FromBody] AddGradeDto data)
        {
            if (
                data == null
                || data.UserID <= 0
                || string.IsNullOrWhiteSpace(data.Subject)
                || data.GradeValue < 1
                || data.GradeValue > 5
                || string.IsNullOrWhiteSpace(data.Date)
            )
            {
                return BadRequest("Hiányzó vagy hibás adatok.");
            }

            try
            {
                if (!DateTime.TryParse(data.Date, out DateTime date))
                {
                    return BadRequest("Érvénytelen dátum formátum.");
                }

                using (var connection = DatabaseConnector.CreateNewConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        // 1. Ellenőrizzük, hogy a tantárgy már szerepel-e a Subject táblában adott UserID-hez
                        string checkSubjectSql =
                            "SELECT COUNT(*) FROM Subject WHERE UserID = @UserID AND Subject = @Subject";
                        using (
                            var checkCmd = new SQLiteCommand(
                                checkSubjectSql,
                                connection,
                                transaction
                            )
                        )
                        {
                            checkCmd.Parameters.AddWithValue("@UserID", data.UserID);
                            checkCmd.Parameters.AddWithValue("@Subject", data.Subject);
                            long count = (long)checkCmd.ExecuteScalar();

                            // 2. Ha nem szerepel, beszúrjuk
                            if (count == 0)
                            {
                                string insertSubjectSql =
                                    "INSERT INTO Subject (UserID, Subject, IsClosed) VALUES (@UserID, @Subject, 0)";
                                using (
                                    var insertCmd = new SQLiteCommand(
                                        insertSubjectSql,
                                        connection,
                                        transaction
                                    )
                                )
                                {
                                    insertCmd.Parameters.AddWithValue("@UserID", data.UserID);
                                    insertCmd.Parameters.AddWithValue("@Subject", data.Subject);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // 3. Érdemjegy beszúrása
                        string insertGradeSql =
                            "INSERT INTO Grade (UserID, Subject, GradeValue, Date) VALUES (@UserID, @Subject, @GradeValue, @Date)";
                        using (
                            var gradeCmd = new SQLiteCommand(
                                insertGradeSql,
                                connection,
                                transaction
                            )
                        )
                        {
                            gradeCmd.Parameters.AddWithValue("@UserID", data.UserID);
                            gradeCmd.Parameters.AddWithValue("@Subject", data.Subject);
                            gradeCmd.Parameters.AddWithValue("@GradeValue", data.GradeValue);
                            gradeCmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                            gradeCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                return Ok("Érdemjegy és szükség esetén tantárgy hozzáadva.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hiba: " + ex.Message);
            }
        }

        // POST /Grade/DeleteGrade (JSON body)
        [HttpPost]
        public IActionResult DeleteGrade([FromBody] DeleteGradeDto data)
        {
            if (data == null || data.GradeID <= 0)
                return BadRequest("Hiányzó vagy hibás adatok.");

            string sql = "DELETE FROM Grade WHERE GradeID = @GradeID";
            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GradeID", data.GradeID);
                int affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                    return NotFound("Érdemjegy nem található.");
            }

            return Ok("Érdemjegy törölve.");
        }

        // POST /Grade/CloseSubject (JSON body)
        [HttpPost]
        public IActionResult CloseSubject([FromBody] CloseSubjectDto data)
        {
            if (data == null || data.UserID <= 0 || string.IsNullOrEmpty(data.Subject))
                return BadRequest("Hiányzó vagy hibás adatok.");

            // Feltételezem, hogy a Subject táblában van UserID és Subject mező is
            string sql =
                "UPDATE Subject SET IsClosed = 1 WHERE UserID = @UserID AND Subject = @Subject";
            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", data.UserID);
                cmd.Parameters.AddWithValue("@Subject", data.Subject);
                int affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                    return NotFound("Tantárgy nem található.");
            }

            return Ok("Tantárgy lezárva.");
        }

        // GET /Grade/GetAbsences?userId=xx
        [HttpGet]
        public IActionResult GetAbsences(int userId)
        {
            try
            {
                var absences = new List<AbsenceEntry>();
                string sql =
                    "SELECT AbsenceID, UserID, Subject, Date, IsExcused FROM Absence WHERE UserID = @UserID";

                using (var connection = DatabaseConnector.CreateNewConnection())
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            absences.Add(
                                new AbsenceEntry
                                {
                                    AbsenceID = reader.GetInt32(0),
                                    UserID = reader.GetInt32(1),
                                    Subject = reader.GetString(2),
                                    Date = reader.GetString(3),
                                    IsExcused = reader.GetBoolean(4),
                                }
                            );
                        }
                    }
                }

                return Ok(absences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hiba történt: " + ex.Message);
            }
        }

        // POST /Grade/AddAbsence (JSON body)
        [HttpPost]
        public IActionResult AddAbsence([FromBody] AddAbsenceDto data)
        {
            if (
                data == null
                || data.UserID <= 0
                || string.IsNullOrEmpty(data.Subject)
                || string.IsNullOrEmpty(data.Date)
            )
                return BadRequest("Hiányzó vagy hibás adatok.");

            try
            {
                DateTime date = DateTime.Parse(data.Date);
                string sql =
                    "INSERT INTO Absence (UserID, Subject, Date, IsExcused) VALUES (@UserID, @Subject, @Date, 0)";
                using (var connection = DatabaseConnector.CreateNewConnection())
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", data.UserID);
                    cmd.Parameters.AddWithValue("@Subject", data.Subject);
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    cmd.ExecuteNonQuery();
                }

                return Ok("Hiányzás hozzáadva.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hiba: " + ex.Message);
            }
        }

        // POST /Grade/ExcuseAbsence (JSON body)
        [HttpPost]
        public IActionResult ExcuseAbsence([FromBody] ExcuseAbsenceDto data)
        {
            if (data == null || data.UserID <= 0 || string.IsNullOrEmpty(data.Date))
                return BadRequest("Hiányzó vagy hibás adatok.");

            try
            {
                DateTime date = DateTime.Parse(data.Date);

                string sql =
                    "UPDATE Absence SET IsExcused = 1 WHERE UserID = @UserID AND Date = @Date";
                using (var connection = DatabaseConnector.CreateNewConnection())
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", data.UserID);
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                        return NotFound("Hiányzás nem található.");
                }

                return Ok("Hiányzás igazolva.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hiba: " + ex.Message);
            }
        }

        // POST /Grade/DeleteAbsence (JSON body)
        [HttpPost]
        public IActionResult DeleteAbsence([FromBody] DeleteAbsenceDto data)
        {
            if (data == null || data.UserID <= 0 || string.IsNullOrEmpty(data.Date))
                return BadRequest("Hiányzó vagy hibás adatok.");

            try
            {
                DateTime date = DateTime.Parse(data.Date);

                string sql = "DELETE FROM Absence WHERE UserID = @UserID AND Date = @Date";
                using (var connection = DatabaseConnector.CreateNewConnection())
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", data.UserID);
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                        return NotFound("Hiányzás nem található.");
                }

                return Ok("Hiányzás törölve.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hiba: " + ex.Message);
            }
        }
    }
}
