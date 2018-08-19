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

    [<CLIMutable>]
    type Country =
        { Code: string
          Name: string }

    [<CLIMutable>]
    type PostalCodeInformation =
        { CountryCode: string
          CountryName: string
          PostalCode: string
          PlaceName: string
          SubdivisionCode: string
          SubdivisionName: string
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: string option }

    type DownloadStatus =
    | Started of countryCode: string
    | Completed of countryCode: string

    type InsertStatus =
    | Progress of symbol: string
    | Started
    | Inserted
