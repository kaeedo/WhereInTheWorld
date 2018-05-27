CREATE TABLE `Country` ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    `Code` TEXT NOT NULL UNIQUE,
    `Name` TEXT NOT NULL,
    `LocalizedName` TEXT NOT NULL );

CREATE TABLE `Subdivision` ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    `CountryId` INTEGER NOT NULL,
    `Code` TEXT NOT NULL UNIQUE,
    `Name` TEXT NOT NULL,
     FOREIGN KEY(`CountryId`) REFERENCES `Country`(`Id`));

CREATE TABLE `PostalCode` ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
     `PostalCode` TEXT NOT NULL,
     `PlaceName` TEXT NOT NULL,
     `SubdivisionId` INTEGER NOT NULL,
     `CountyName` TEXT,
     `CountyCode` TEXT,
     `CommunityName` TEXT,
     `CommunityCode` TEXT,
     `Latitude` REAL,
     `Longitude` REAL,
     `Accuracy` INTEGER,
     UNIQUE(`PostalCode`, `PlaceName`) ON CONFLICT IGNORE,
     FOREIGN KEY(`SubdivisionId`) REFERENCES `Subdivision`(`Id`));
