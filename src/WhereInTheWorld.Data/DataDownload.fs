namespace WhereInTheWorld.Data

open System.IO
open System.IO.Compression
open System.Net.Http
open Hopac
open Hopac.Infixes
open WhereInTheWorld.Utilities.ResultUtilities
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities
open System


module DataDownload =
    let private baseUrl = downloadUrl
    let downloadZip countryCode =
        job {
            let httpClient = new HttpClient()
            let! response = httpClient.GetByteArrayAsync(sprintf "%s%s.zip" baseUrl countryCode) |> Job.awaitTask
            return Result.Ok response
        }

    let saveZip countryCode file =
        Directory.CreateDirectory(baseDirectory) |> ignore
        let filePath = baseDirectory @@ countryCode
        File.WriteAllBytes(sprintf "%s.zip" filePath, file)
        filePath |> Result.Ok

    let saveCountryFile filePath =
        let zipFileName = sprintf "%s.zip" filePath
        let countryCode = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last

        let archive = ZipFile.OpenRead(zipFileName)

        archive.Entries
        |> Seq.find (fun zae ->
            zae.Name <> "readme.txt"
        )
        |> fun entry -> entry.ExtractToFile(sprintf "%s.txt" (baseDirectory @@ countryCode))
        archive.Dispose()

        File.Delete(zipFileName)

        Result.Ok filePath

    let supportedCountries =
        IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.supportedCountries.tsv"
        |> IoUtilities.parseTsv
        |> Map.ofSeq

    let postalCodeFormats =
        IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.countryInformationPostalCodes.tsv"
        |> IoUtilities.parseTsv
        |> Map.ofSeq


    let downloadPostalCodesForCountry statusChannel countryCode =
        let workflow = downloadZip >=> Job.lift (saveZip countryCode) >=> Job.lift saveCountryFile
        let workflowResult =
            job {
                do! statusChannel *<- (DownloadStatus.Started <| supportedCountries.[countryCode])

                let! result = workflow countryCode

                if result.IsOk
                then do! statusChannel *<- (Completed <| supportedCountries.[countryCode])

                return result
            }
        Job.tryWith workflowResult (Job.lift Result.Error)
