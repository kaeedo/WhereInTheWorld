namespace WhereInTheWorld.Update

open System.IO
open System.IO.Compression
open Hopac
open HttpFs.Client
open Utilities

module DataDownload =
    let private baseUrl = "http://download.geonames.org/export/zip/"
    let private baseSaveDirectory = "./temp"

    let private downloadZip countryCode =
        job {
            try
                let fileBytes =
                    Request.createUrl Get <| sprintf "%s%s.zip" baseUrl countryCode
                    |> Request.responseAsBytes
                    |> run
                    |> Result.Ok
                return fileBytes
            with
            | e -> return Result.Error e
        }

    let private saveZip countryCode file =
        job {
            try
                let filePath = sprintf "./%s/%s.zip" baseSaveDirectory countryCode

                File.WriteAllBytes(filePath, file)
                return filePath |> Result.Ok
            with
            | e -> return Result.Error e
        }

    let private saveCountryFile filePath =
        job {
            try
                if Directory.Exists(baseSaveDirectory)
                then Directory.Delete(baseSaveDirectory, true)

                Directory.CreateDirectory(baseSaveDirectory) |> ignore
                ZipFile.ExtractToDirectory(filePath, baseSaveDirectory)

                return Result.Ok filePath
            with
            | e -> return Result.Error e
        }

    let supportedCountries =
        [ "CA", "Canada", "Canada"
          "DE", "Germany", "Deutschland"
          "US", "United States of America", "United States of America" ]

    let downloadPostalCodesForCountry countryCode =
        let workflow = downloadZip >=> (saveZip countryCode) >=> saveCountryFile
        workflow countryCode
