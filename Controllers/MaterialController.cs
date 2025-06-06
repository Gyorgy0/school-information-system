using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using SchoolAPI.Models.Materials;
using SchoolAPI.Models;

[ApiController]
[Route("[controller]/[action]")]
public class MaterialController : Controller
{
    [HttpGet]
    public IActionResult GetMaterials()
    {
        List<Material> materials = new List<Material>();
        string sql = "SELECT * FROM Material";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    materials.Add(new Material
                    {
                        MaterialID = reader.GetInt32(0),
                        FileName = reader.GetString(1),
                        FileData = reader.GetString(2),
                        Subject = reader.GetString(3),
                        UploadDate = reader.GetString(4),
                        SubjectID = reader.GetInt32(5),
                        TeacherID = reader.GetInt32(6)
                    });
                }
            }
        }

        return Json(materials);
    }

    [HttpPost]
    public IActionResult UploadMaterial([FromBody] Material request)
    {
        // Feltételezzük, hogy a request.Subject már tartalmazza a tantárgy nevét, 
        // nem próbáljuk meg lekérdezni külön Subject táblából

        try
        {
            byte[] fileBytes = Convert.FromBase64String(request.FileData);

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, request.FileName);
            System.IO.File.WriteAllBytes(filePath, fileBytes);

            string sql = "INSERT INTO Material (FileName, FileData, Subject, UploadDate, SubjectID, TeacherID) VALUES (@FileName, @FileData, @Subject, @UploadDate, @SubjectID, @TeacherID)";
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@FileName", request.FileName);
                    cmd.Parameters.AddWithValue("@FileData", request.FileData);
                    cmd.Parameters.AddWithValue("@Subject", request.Subject);
                    cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@SubjectID", request.SubjectID);
                    cmd.Parameters.AddWithValue("@TeacherID", request.TeacherID);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Anyag sikeresen feltöltve");
        }
        catch (Exception ex)
        {
            return BadRequest($"Hiba történt a feltöltés során: {ex.Message}");
        }
    }
}