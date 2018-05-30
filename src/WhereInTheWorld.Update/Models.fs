namespace WhereInTheWorld.Update

open System
open Utilities

module Models =
    let baseDirectory =
        match Environment.OSVersion.Platform with
        | PlatformID.Unix -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
        | PlatformID.MacOSX -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
        | _ -> Environment.GetFolderPath(Environment.SpecialFolder.Personal) @@ ".WhereInTheWorld"

    type PostalCodeInformation =
        { Id: int
          CountryCode: string
          CountryName: string
          CountryLocalizedName: string
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
          Accuracy: int option }

    type DownloadStatus =
    | Completed of countryCode: string

    type InsertStatus =
    | Inserted of countryCode: string
