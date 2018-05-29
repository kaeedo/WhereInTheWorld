open System.IO
open WhereInTheWorld.Update
open WhereInTheWorld.Update.Models
open ShellProgressBar
open Hopac

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

    let options = new ProgressBarOptions(
                    ProgressCharacter = '-',
                    ProgressBarOnBottom = true
                )
    use progressBar =
        new ProgressBar(countryLength, "Downloading", options)

    let jobStatusPrinterJob jobStatusChannel =
        job {
            let! jobStatus = Ch.take jobStatusChannel

            match jobStatus with
            | Completed _ ->
                progressBar.Tick(sprintf "%i of %i" (progressBar.CurrentTick + 1) countryLength)
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
