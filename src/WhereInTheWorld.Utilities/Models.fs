namespace WhereInTheWorld.Utilities

open System
open IoUtilities

module Models =
    let baseDirectory =
        match Environment.OSVersion.Platform with
        | PlatformID.Unix -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
        | PlatformID.MacOSX -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
        | _ -> Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) @@ ".WhereInTheWorld"

    let databaseFile = baseDirectory @@ "world.db"

    type Country =
        { Id: int64
          Code: string
          Name: string }

    type Subdivision =
        { Id: int64
          CountryId: int64
          Code: string
          Name: string }

    type PostalCode =
        { Id: int64
          SubdivisionId: int64
          PostalCode: string
          PlaceName: string
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: string option
          Latitude: float option
          Longitude: float option
          Accuracy: int64 option }

    type CountryDao =
        { Code: string
          Name: string }

    type SubdivisionDao =
        { Country: CountryDao
          Code: string
          Name: string }

    type PostalCodeDao =
        { Subdivision: SubdivisionDao
          PostalCode: string
          PlaceName: string
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: string option
          Latitude: float option
          Longitude: float option
          Accuracy: int64 option }

    type FileImport =
        { CountryCode: string
          PostalCode: string
          PlaceName: string
          SubdivisionName: string
          SubdivisionCode: string
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: string option
          Latitude: float option
          Longitude: float option
          Accuracy: int64 option }

    type DownloadStatus =
    | Started of countryCode: string
    | Completed of countryCode: string

    type InsertStatus =
    | Progress of symbol: string
    | Started
    | Inserted
