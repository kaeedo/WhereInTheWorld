open System.IO
open WhereInTheWorld.Update
open WhereInTheWorld.Update.Models
open Hopac
open System

let ensureDirectory () =
    if Directory.Exists(Models.baseDirectory)
    then Directory.Delete(Models.baseDirectory, true)
    Directory.CreateDirectory(Models.baseDirectory) |> ignore

[<EntryPoint>]
let main argv =
    ensureDirectory()
    DataAccess.ensureDatabase()
    DataAccess.openConnection()

    let countryLength = DataDownload.supportedCountries |> Seq.length

    let jobStatusPrinterJob jobStatusChannel =
        job {
            let! jobStatus = Ch.take jobStatusChannel

            match jobStatus with
            | Completed cc ->
                printfn "%s downloaded" cc
        }

    let jobStatusChannel = Ch<DownloadStatus>()

    let download jobStatusChannel =
        job {
            let jobStatusPrinter = jobStatusPrinterJob jobStatusChannel
            do! Job.foreverServer jobStatusPrinter

            return!
                DataDownload.supportedCountries
                |> Seq.map (fun sc ->
                    let code, _, _ = sc
                    DataDownload.downloadPostalCodesForCountry jobStatusChannel code
                )
                |> Job.conCollect
        }

    download jobStatusChannel |> run |> ignore



    // UpdateProcess.updateAll()

    DataAccess.closeConnection()
    0
