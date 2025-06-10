using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class CourseController : Controller
{
    string _sid => Request.Cookies["id"];

    [HttpGet]
    public IActionResult GetCourses()
    {
        var list = new List<CourseModel>();
        using var conn = DatabaseConnector.CreateNewConnection();
        using var cmd = new SQLiteCommand(
            "SELECT CourseID, Name, TeacherID, Visible FROM Course", conn);
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
            list.Add(new CourseModel
            {
                CourseID = rdr.GetInt32(0),
                Name = rdr.GetString(1),
                TeacherID = rdr.GetInt32(2),
                Visible = rdr.GetBoolean(3)
            });
        return Json(list);
    }

    [HttpPost]
    public IActionResult CreateCourse([FromForm] string name,
                                      [FromForm] int teacherID,
                                      [FromForm] bool visible)
    {
        using var conn = DatabaseConnector.CreateNewConnection();
        using var cmd = new SQLiteCommand(
            "INSERT INTO Course (Name, TeacherID, Visible) VALUES (@Name,@TeacherID,@Visible)", conn);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@TeacherID", teacherID);
        cmd.Parameters.AddWithValue("@Visible", visible);
        cmd.ExecuteNonQuery();
        return Ok();
    }
    
[HttpPost]
public IActionResult EditCourse(
    [FromForm] int courseID,
    [FromForm] string name,
    [FromForm] int teacherID,
    [FromForm] bool visible)
{
    using var conn = DatabaseConnector.CreateNewConnection();
    using var cmd = new SQLiteCommand(
        "UPDATE Course SET Name=@Name, TeacherID=@TeacherID, Visible=@Visible WHERE CourseID=@CourseID",
        conn);
    cmd.Parameters.AddWithValue("@Name", name);
    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
    cmd.Parameters.AddWithValue("@Visible", visible);
    cmd.Parameters.AddWithValue("@CourseID", courseID);
    cmd.ExecuteNonQuery();
    return Ok();
}

[HttpPost]
public IActionResult DeleteCourse([FromForm] int id)
{
    using var conn = DatabaseConnector.CreateNewConnection();
    using var cmd = new SQLiteCommand(
        "DELETE FROM Course WHERE CourseID=@ID", conn);
    cmd.Parameters.AddWithValue("@ID", id);
    cmd.ExecuteNonQuery();
    return Ok();
}

[HttpGet]
public IActionResult GetCourseEntries(int courseId)
{
    var list = new List<string>();
    using var conn = DatabaseConnector.CreateNewConnection();
    using var cmd = new SQLiteCommand(
        "SELECT Content FROM CourseEntries WHERE CourseID=@ID", conn);
    cmd.Parameters.AddWithValue("@ID", courseId);
    using var rdr = cmd.ExecuteReader();
    while(rdr.Read()) list.Add(rdr.GetString(0));
    return Json(list);
}

[HttpPost]
public IActionResult AddCourseEntry([FromForm] int courseId,
                                    [FromForm] string content)
{
    using var conn = DatabaseConnector.CreateNewConnection();
    using var cmd = new SQLiteCommand(
        "INSERT INTO CourseEntries (CourseID,Content) VALUES(@ID,@Content)", conn);
    cmd.Parameters.AddWithValue("@ID", courseId);
    cmd.Parameters.AddWithValue("@Content", content);
    cmd.ExecuteNonQuery();
    return Ok();
}
}