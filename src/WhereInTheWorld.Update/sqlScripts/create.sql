CREATE TABLE "Country" ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `Code` TEXT NOT NULL UNIQUE, `Name` TEXT NOT NULL, `LocalizedName` TEXT NOT NULL );
CREATE TABLE `Subdivision` ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `Code` TEXT NOT NULL UNIQUE, `Name` TEXT NOT NULL );
CREATE TABLE `PostalCode` ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
     `PostalCode` TEXT NOT NULL,
     `PlaceName` INTEGER NOT NULL,
     `CountryId` INTEGER NOT NULL,
     `SubdivisionId` INTEGER NOT NULL,
     `CommunityName` TEXT,
     `CommunityCode` INTEGER,
     `Latitude` REAL,
     `Longitude` REAL,
     `Accuracy` INTEGER,
     FOREIGN KEY(`SubdivisionId`) REFERENCES `Subdivision`(`Id`),
     FOREIGN KEY(`CountryId`) REFERENCES `Country`(`Id`) );
