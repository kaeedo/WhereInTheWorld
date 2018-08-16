namespace WhereInTheWorld.Utilities

open System
open FSharp.Data
open IoUtilities

type AppSettings = JsonProvider<"../WhereInTheWorld/applicationConfig.json">


module Models =
    let private applicationConfig = AppSettings.Load("./applicationConfig.json")

    let downloadUrl = applicationConfig.ConnectionStrings.CitiesDownloadUrl
    let isTest = applicationConfig.IsTest

    let baseDirectory =
        let homeDirectory =
            match Environment.OSVersion.Platform with
            | PlatformID.Unix -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
            | PlatformID.MacOSX -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
            | _ -> Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) @@ ".WhereInTheWorld"
        if isTest
        then homeDirectory @@ "test"
        else homeDirectory

    let databaseFile = baseDirectory @@ if isTest then "test" else String.Empty @@ "world.db"

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

    type PostalCodeInformation =
        { Id: int
          CountryCode: string
          CountryName: string
          PostalCode: string
          PlaceName: string
          SubdivisionCode: string
          SubdivisionName: string
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: string option
          Latitude: float option
          Longitude: float option
          Accuracy: int option }

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
