PRAGMA foreign_keys = ON;

-- Create Users Table
CREATE TABLE IF NOT EXISTS `User` (
    `UserID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Username` TEXT NOT NULL,
    `PasswordHash` TEXT NOT NULL,
    `PasswordSalt` TEXT NOT NULL,
    FOREIGN KEY (`Role`) REFERENCES `Roles` (`Role`)  -- 'student', 'teacher', 'admin'
);

-- Create Roles Table
CREATE TABLE IF NOT EXISTS `Roles` (
    `Role` TEXT PRIMARY KEY, -- 'student', 'teacher', 'admin'
    `ModifyUser` INTEGER NOT NULL,
    `ModifyEvent` INTEGER NOT NULL,
    `AddSubject` INTEGER NOT NULL,
    `RemoveSubject` INTEGER NOT NULL,
    `ModifySubject` INTEGER NOT NULL,
    `SeeSchedule` INTEGER NOT NULL,
    `SeeAllSchedule` INTEGER NOT NULL,
    `ModifySchedule` INTEGER NOT NULL,
    `SeeOwnGrades` INTEGER NOT NULL,
    `SeeGrades` INTEGER NOT NULL,
    `ModifyGrades` INTEGER NOT NULL,
    `SeeOwnStatistics` INTEGER NOT NULL,
    `SeeStatistics` INTEGER NOT NULL,
    `ModifyStatistics` INTEGER NOT NULL,
    `SeeCourses` INTEGER NOT NULL,
    `ModifyCourses` INTEGER NOT NULL,
    `ModifyLunchTable` INTEGER NOT NULL,
);

-- Create Grades Table
CREATE TABLE IF NOT EXISTS `Grades` (
    `GradeID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `UserID` INTEGER NOT NULL,  -- Link to Users table
    `Subject` TEXT NOT NULL,
    `Grade` INTEGER NOT NULL,
    `Date` DATETIME NOT NULL,
    FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`)
);

-- Create Homework Table
CREATE TABLE IF NOT EXISTS `Homework` (
    `HomeworkID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `UserID` INTEGER NOT NULL,  -- Link to Users table
    `Subject` TEXT NOT NULL,
    `Description` TEXT NOT NULL,
    `DueDate` DATETIME NOT NULL,
    FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`)
);

-- Create Messages Table
CREATE TABLE IF NOT EXISTS `Messages` (
    `MessageID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `SenderID` INTEGER NOT NULL,  -- Link to Users table
    `ReceiverID` INTEGER NOT NULL,  -- Link to Users table
    `Message` TEXT NOT NULL,
    `Timestamp` DATETIME NOT NULL,
    FOREIGN KEY (`SenderID`) REFERENCES `Users` (`UserID`),
    FOREIGN KEY (`ReceiverID`) REFERENCES `Users` (`UserID`)
);

-- Create Timetable Table
CREATE TABLE IF NOT EXISTS `Timetable` (
    `TimetableID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Day` TEXT NOT NULL,
    `Hour` TEXT NOT NULL,
    `Subject` TEXT NOT NULL,
    `Room` TEXT NOT NULL,
    `TeacherID` INTEGER NOT NULL,  -- Link to Users table (teacher)
    FOREIGN KEY (`TeacherID`) REFERENCES `Users` (`UserID`)
);

-- Create Lunch Table
CREATE TABLE IF NOT EXISTS `Lunch` (
    `LunchID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Day` TEXT NOT NULL,
    `Meal` TEXT NOT NULL
);

-- Courses
CREATE TABLE IF NOT EXISTS `Course` (
    `CourseID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Name` TEXT NOT NULL,
    `TeacherID` INTEGER NOT NULL,
    `Visible` INTEGER NOT NULL DEFAULT 1, -- 1=public, 0=private
    FOREIGN KEY (`TeacherID`) REFERENCES `User`(`UserID`)
);

-- Enrollments
CREATE TABLE IF NOT EXISTS `CourseStudent` (
    `CourseID` INTEGER NOT NULL,
    `StudentID` INTEGER NOT NULL,
    `EnrolledAt` DATETIME NOT NULL DEFAULT (datetime('now')),
    PRIMARY KEY (`CourseID`,`StudentID`),
    FOREIGN KEY (`CourseID`) REFERENCES `Course`(`CourseID`),
    FOREIGN KEY (`StudentID`) REFERENCES `User`(`UserID`)
);

-- Materials
CREATE TABLE IF NOT EXISTS `CourseMaterial` (
    `MaterialID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `CourseID` INTEGER NOT NULL,
    `Title` TEXT NOT NULL,
    `Url` TEXT,
    `UploadedAt` DATETIME NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (`CourseID`) REFERENCES `Course`(`CourseID`)
);

CREATE TABLE IF NOT EXISTS `CourseEntries` (
  `EntryID`    INTEGER PRIMARY KEY AUTOINCREMENT,
  `CourseID`   INTEGER NOT NULL,
  `Content`    TEXT    NOT NULL,
  `CreatedAt`  DATETIME NOT NULL DEFAULT (datetime('now')),
  FOREIGN KEY (`CourseID`) REFERENCES `Course`(`CourseID`)
);

-- Tests
CREATE TABLE IF NOT EXISTS `CourseTest` (
    `TestID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `CourseID` INTEGER NOT NULL,
    `Title` TEXT NOT NULL,
    `Description` TEXT,
    `DueDate` DATETIME,
    FOREIGN KEY (`CourseID`) REFERENCES `Course`(`CourseID`)
);

-- Assignments / Projects
CREATE TABLE IF NOT EXISTS `Assignment` (
    `AssignmentID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `CourseID` INTEGER NOT NULL,
    `Title` TEXT NOT NULL,
    `Description` TEXT,
    `DueDate` DATETIME,
    FOREIGN KEY (`CourseID`) REFERENCES `Course`(`CourseID`)
);

-- Create Events table
CREATE TABLE IF NOT EXISTS `SchoolEvent` (
    `EventID`    INTEGER PRIMARY KEY AUTOINCREMENT,
    `TimetableID` INTEGER NOT NULL,
    `EventType`  TEXT NOT NULL,
    `EventDate`  DATETIME NOT NULL,
    `Description` TEXT
);