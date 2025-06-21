using System.Data.SQLite;
using Microsoft.AspNetCore.Http.HttpResults;
using SchoolAPI.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));
SQLiteConnection connection = DatabaseConnector.Db();
SQLiteCommand command = connection.CreateCommand();
command.CommandText =
    "PRAGMA foreign_keys = ON;"
    //#################################################################################
    // User table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `User` ("
    + "`UserID` INTEGER NOT NULL PRIMARY KEY, "
    + "`Username` TEXT NOT NULL, "
    + "`PasswordHash` TEXT NOT NULL, "
    + "`PasswordSalt` TEXT NOT NULL, "
    + "`Firstname` TEXT, "
    + "`Middlename` TEXT, "
    + "`Surname` TEXT, "
    + "`Role` TEXT NOT NULL,"
    + "`Class` TEXT);"
    //#################################################################################
    // Session table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Session` ("
    + "`Sessioncookie` TEXT NOT NULL PRIMARY KEY,"
    + "`UserID` INTEGER NOT NULL,"
    + "`LoginTime` DATETIME NOT NULL,"
    + "`ValidUntil` DATETIME NOT NULL);"
    //#################################################################################
    // Subjects table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Subjects` ("
    + "`SubjectID` INTEGER PRIMARY KEY AUTOINCREMENT,"
    + "`Name` TEXT NOT NULL);"
    //#################################################################################
    // Classrooms table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Classrooms` ("
    + "`ClassroomID` INTEGER PRIMARY KEY AUTOINCREMENT,"
    + "`Name` TEXT NOT NULL);"
    //#################################################################################
    // Classes table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Classes` ("
    + "`Year` INTEGER NOT NULL,"
    + "`Group` INTEGER PRIMARY KEY AUTOINCREMENT,"
    + "`Class` TEXT NOT NULL);"
    //#################################################################################
    // SchoolEvent table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `SchoolEvent` ("
    + "EventID INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "TimetableID INTEGER NOT NULL, "
    + "EventType TEXT NOT NULL, "
    + "EventDate DATETIME NOT NULL, "
    + "Description TEXT"
    + ");"
    //#################################################################################
    // Grade table
    //#################################################################################
    //+ "CREATE TABLE IF NOT EXISTS `Grade` ("
    //+ "`GradeID` INTEGER NOT NULL PRIMARY KEY, "
    //+ "`UserID` INTEGER NOT NULL, "
    //+ "`Subject` TEXT NOT NULL, "
    //+ "`GradeValue` INTEGER NOT NULL, "
    //+ "`Date` TEXT NOT NULL, "
    //+ "FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`));"
    //#################################################################################
    // Homework table
    //#################################################################################
    //+ "CREATE TABLE IF NOT EXISTS `Homework` ("
    //+ "`HomeworkID` INTEGER NOT NULL PRIMARY KEY, "
    //+ "`UserID` INTEGER NOT NULL, "
    //+ "`Subject` TEXT NOT NULL, "
    //+ "`Description` TEXT NOT NULL, "
    //+ "`DueDate` TEXT NOT NULL, "
    //+ "FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`));"
    //#################################################################################
    // Message table
    //#################################################################################
    //+ "CREATE TABLE IF NOT EXISTS `Message` ("
    //+ "`MessageID` INTEGER NOT NULL PRIMARY KEY, "
    //+ "`SenderID` INTEGER NOT NULL, "
    //+ "`ReceiverID` INTEGER NOT NULL, "
    //+ "`MessageText` TEXT NOT NULL, "
    //+ "`Timestamp` TEXT NOT NULL, "
    //+ "FOREIGN KEY (`SenderID`) REFERENCES `Users` (`UserID`), "
    //+ "FOREIGN KEY (`ReceiverID`) REFERENCES `Users` (`UserID`));"
    //#################################################################################
    // Timetable table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Timetable` ("
    + "`TimetableID` TEXT NOT NULL, "
    + "`Day` TEXT NOT NULL, "
    + "`Hour` TEXT NOT NULL, "
    + "`Subject` TEXT NOT NULL, "
    + "`Classroom` TEXT NOT NULL, "
    + "`TeacherID` INTEGER NOT NULL, "
    + "FOREIGN KEY (`TeacherID`) REFERENCES `Users` (`UserID`),"
    + "FOREIGN KEY (`TimetableID`) REFERENCES `Classes` (`Class`)); "
    //#################################################################################
    // Course table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Course` ("
    + "CourseID INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "Name TEXT NOT NULL, "
    + "TeacherID INTEGER NOT NULL, "
    + "Visible INTEGER NOT NULL DEFAULT 1, "
    + "FOREIGN KEY(TeacherID) REFERENCES `User` (`UserID`));"
    //#################################################################################
    // CourseStudent table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `CourseStudent` ("
    + "CourseID INTEGER NOT NULL, "
    + "StudentID INTEGER NOT NULL, "
    + "EnrolledAt DATETIME NOT NULL DEFAULT (datetime('now')), "
    + "PRIMARY KEY(CourseID,StudentID), "
    + "FOREIGN KEY(CourseID) REFERENCES Course(CourseID), "
    + "FOREIGN KEY(StudentID) REFERENCES `User` (`UserID`));"
    //#################################################################################
    // CourseMaterial table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `CourseMaterial` ("
    + "MaterialID INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "CourseID INTEGER NOT NULL, "
    + "Title TEXT NOT NULL, "
    + "Url TEXT, "
    + "UploadedAt DATETIME NOT NULL DEFAULT (datetime('now')), "
    + "FOREIGN KEY(CourseID) REFERENCES `Course`(`CourseID`));"
    //#################################################################################
    // CourseEntries table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `CourseEntries` ("
    + "`EntryID`    INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "`CourseID`   INTEGER NOT NULL, "
    + "`Content`    TEXT    NOT NULL, "
    + "`CreatedAt`  DATETIME NOT NULL DEFAULT (datetime('now')), "
    + "FOREIGN KEY(`CourseID`) REFERENCES `Course`(`CourseID`));"
    //#################################################################################
    // CourseTest table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `CourseTest` ("
    + "TestID INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "CourseID INTEGER NOT NULL, "
    + "Title TEXT NOT NULL, "
    + "Description TEXT, "
    + "DueDate DATETIME, "
    + "FOREIGN KEY(CourseID) REFERENCES `Course`(`CourseID`));"
    //#################################################################################
    // Soup table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Soup` ("
    + "`SoupID` INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "`Name` TEXT NOT NULL UNIQUE);"
    //#################################################################################
    // MainDish table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `MainDish` ("
    + "`MainDishID` INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "`Name` TEXT NOT NULL UNIQUE);"
    //#################################################################################
    // Dessert table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Dessert` ("
    + "`DessertID` INTEGER PRIMARY KEY AUTOINCREMENT, "
    + "`Name` TEXT NOT NULL UNIQUE);"
    //#################################################################################
    // Subject table
    //#################################################################################
    //+ "CREATE TABLE IF NOT EXISTS `Subject` ("
    //+ "`SubjectID` INTEGER NOT NULL PRIMARY KEY, "
    //+ "`UserID` INTEGER NOT NULL, "
    //+ "`Subject` TEXT NOT NULL, "
    //+ "`IsClosed`	INTEGER NOT NULL, "
    //+ "FOREIGN KEY(`UserID`) REFERENCES `User`(`UserID`));"
    //#################################################################################
    // Absence table
    //#################################################################################
    //+ "CREATE TABLE IF NOT EXISTS `Absence` ("
    //+ "`AbsenceID` INTEGER NOT NULL PRIMARY KEY, "
    //+ "`UserID` INTEGER NOT NULL, "
    //+ "`Subject` TEXT NOT NULL, "
    //+ "`Date` DATETIME NOT NULL, "
    //+ "`IsExcused` INTEGER NOT NULL, "
    //+ "FOREIGN KEY(`UserID`) REFERENCES `User`(`UserID`));"
    //#################################################################################
    // Lunch table
    //#################################################################################
    + "CREATE TABLE IF NOT EXISTS `Lunch` ("
    + "`LunchID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "
    + "`Day` TEXT NOT NULL, "
    + "`SoupID` INTEGER NOT NULL, "
    + "`MainDishID` INTEGER NOT NULL, "
    + "`DessertID` INTEGER NOT NULL, "
    + "`Date` DATETIME, "
    + "FOREIGN KEY (`SoupID`) REFERENCES `Soup`(`SoupID`), "
    + "FOREIGN KEY (`MainDishID`) REFERENCES `MainDish`(`MainDishID`), "
    + "FOREIGN KEY (`DessertID`) REFERENCES `Dessert`(`DessertID`));";

command.ExecuteNonQuery();

LunchGenerator.InsertSampleData(connection);
LunchGenerator.GenerateFullYearLunch(connection, 2025);

command.Dispose();

app.Run();
