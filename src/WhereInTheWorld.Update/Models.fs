namespace WhereInTheWorld.Update

open System
open Utilities
open Hopac

module Models =
    let baseDirectory =
        match Environment.OSVersion.Platform with
        | PlatformID.Unix -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
        | PlatformID.MacOSX -> Environment.GetEnvironmentVariable("HOME") @@ ".WhereInTheWorld"
        | _ -> Environment.GetFolderPath(Environment.SpecialFolder.Personal) @@ ".WhereInTheWorld"

    [<CLIMutable>]
    type CountryDao =
        { Id: int64
          Code: string
          Name: string
          LocalizedName: string }

    [<CLIMutable>]
    type SubdivisionDao =
        { Id: int64
          CountryId: int64
          Code: string
          Name: string }

    [<CLIMutable>]
    type PostalCodeDao =
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
    | Completed of countryCode: string

    type InsertStatus =
    | Inserted of countryCode: string
