PRAGMA foreign_keys = ON;

-- Create Users Table
CREATE TABLE IF NOT EXISTS `User` (
    `UserID` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Username` TEXT NOT NULL,
    `PasswordHash` TEXT NOT NULL,
    `PasswordSalt` TEXT NOT NULL,
    `Role` TEXT NOT NULL  -- 'student', 'teacher', 'admin'
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