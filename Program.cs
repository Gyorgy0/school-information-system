using System.Data.SQLite;

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
command.CommandText = "PRAGMA foreign_keys = ON;" +
    "CREATE TABLE IF NOT EXISTS `User` (" +
    "`UserID` INTEGER NOT NULL PRIMARY KEY, " +
    "`Username` TEXT NOT NULL, " +
    "`PasswordHash` TEXT NOT NULL, " +
    "`PasswordSalt` TEXT NOT NULL, " +
    "`Role` TEXT NOT NULL);" +

    "CREATE TABLE IF NOT EXISTS `Roles` (" +
    "`Role` TEXT PRIMARY KEY," +
    "`ModifyUser` INTEGER NOT NULL," +
    "`ModifyEvent` INTEGER NOT NULL," +
    "`AddSubject` INTEGER NOT NULL," +
    "`RemoveSubject` INTEGER NOT NULL," +
    "`ModifySubject` INTEGER NOT NULL," +
    "`SeeSchedule` INTEGER NOT NULL," +
    "`SeeAllSchedule` INTEGER NOT NULL," +
    "`ModifySchedule` INTEGER NOT NULL," +
    "`SeeOwnGrades` INTEGER NOT NULL," +
    "`SeeGrades` INTEGER NOT NULL," +
    "`ModifyGrades` INTEGER NOT NULL," +
    "`SeeOwnStatistics` INTEGER NOT NULL," +
    "`SeeStatistics` INTEGER NOT NULL," +
    "`ModifyStatistics` INTEGER NOT NULL," +
    "`SeeCourses` INTEGER NOT NULL," +
    "`ModifyCourses` INTEGER NOT NULL," +
    "`ModifyLunchTable` INTEGER NOT NULL);" +

    "CREATE TABLE IF NOT EXISTS `Subjects` (" +
    "`SubjectID` INTEGER NOT NULL PRIMARY KEY, " +
    "`Name` TEXT NOT NULL);" +

    "CREATE TABLE IF NOT EXISTS `Classes` (" +
    "`ClassID` INTEGER NOT NULL PRIMARY KEY, " +
    "`Name` TEXT NOT NULL, " +
    "`StudentID` INTEGER NOT NULL, " +
    "`StudentName` TEXT NOT NULL);" +

    "CREATE TABLE IF NOT EXISTS `Grade` (" +
    "`GradeID` INTEGER NOT NULL PRIMARY KEY, " +
    "`UserID` INTEGER NOT NULL, " +
    "`Subject` TEXT NOT NULL, " +
    "`GradeValue` INTEGER NOT NULL, " +
    "`Date` TEXT NOT NULL, " +
    "FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`));" +

    "CREATE TABLE IF NOT EXISTS `Homework` (" +
    "`HomeworkID` INTEGER NOT NULL PRIMARY KEY, " +
    "`UserID` INTEGER NOT NULL, " +
    "`Subject` TEXT NOT NULL, " +
    "`Description` TEXT NOT NULL, " +
    "`DueDate` TEXT NOT NULL, " +
    "FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`));" +

    "CREATE TABLE IF NOT EXISTS `Message` (" +
    "`MessageID` INTEGER NOT NULL PRIMARY KEY, " +
    "`SenderID` INTEGER NOT NULL, " +
    "`ReceiverID` INTEGER NOT NULL, " +
    "`MessageText` TEXT NOT NULL, " +
    "`Timestamp` TEXT NOT NULL, " +
    "FOREIGN KEY (`SenderID`) REFERENCES `Users` (`UserID`), " +
    "FOREIGN KEY (`ReceiverID`) REFERENCES `Users` (`UserID`));" +

    "CREATE TABLE IF NOT EXISTS `Timetable` (" +
    "`TimetableID` INTEGER NOT NULL PRIMARY KEY, " +
    "`Day` TEXT NOT NULL, " +
    "`Hour` TEXT NOT NULL, " +
    "`Subject` TEXT NOT NULL, " +
    "`Room` TEXT NOT NULL, " +
    "`TeacherID` INTEGER NOT NULL, " +
    "FOREIGN KEY (`TeacherID`) REFERENCES `Users` (`UserID`));" +

    "CREATE TABLE IF NOT EXISTS `Lunch` (" +
    "`LunchID` INTEGER NOT NULL PRIMARY KEY, " +
    "`Day` TEXT NOT NULL, " +
    "`Meal` TEXT NOT NULL);";

command.ExecuteNonQuery();
command.Dispose();

app.Run();