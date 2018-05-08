namespace WhereInTheWorld.Update

open System.IO
open System.IO.Compression
open Hopac
open HttpFs.Client

module DataDownload =
    let private baseUrl = "http://download.geonames.org/export/zip/"
    let private baseSaveDirectory = "./temp"

    let private downloadZip countryCode =
        Request.createUrl Get <| sprintf "%s%s.zip" baseUrl countryCode
        |> Request.responseAsBytes

    let private saveZip countryCode (response: Job<byte[]>) =
        job {
            let! contents = response

            File.WriteAllBytes(sprintf "./%s/%s.zip" baseSaveDirectory countryCode, contents)
        }

    let supportedCountries =
        [ "CA", "Canada", "Canada"
          "DE", "Germany", "Deutschland"
          "US", "United States of America", "United States of America" ]

    let saveCountryFile countryCode =
        Directory.CreateDirectory(baseSaveDirectory) |> ignore

        job {
            let zipJob = downloadZip countryCode
            do! saveZip countryCode zipJob
            ZipFile.ExtractToDirectory(sprintf "./%s/%s.zip" baseSaveDirectory countryCode, baseSaveDirectory)
        }
