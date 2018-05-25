open System
open System.IO
open WhereInTheWorld.Update
open Hopac

let ensureDirectory () =
    if Directory.Exists(Models.baseDirectory)
    then Directory.Delete(Models.baseDirectory, true)
    Directory.CreateDirectory(Models.baseDirectory) |> ignore

let batchesOf n =
    Seq.mapi (fun i v -> i / n, v) >>
    Seq.groupBy fst >>
    Seq.map snd >>
    Seq.map (Seq.map snd)

[<EntryPoint>]
let main argv =
    ensureDirectory()
    DataAccess.ensureDatabase()
    DataAccess.openConnection()

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()

    DataDownload.supportedCountries
    |> Seq.map (fun sc ->
        let code, _, _ = sc
        DataDownload.downloadPostalCodesForCountry code
    )
    |> Job.conCollect
    |> run
    |> Seq.iter (fun cd ->
        match cd with
        | Error error ->
            printfn "Country download for %s failed with message: %A" "" error
        | Ok countryCode -> printfn "Country download for %s succeeded" countryCode
    )

    DataDownload.supportedCountries
    |> Seq.iter (fun sc ->
        let code, _, _ = sc
        UpdateProcess.updateCountry code
        |> run
        |> ignore
    )

    stopWatch.Stop()
    printfn "Total time took %fms" stopWatch.Elapsed.TotalMilliseconds
    DataAccess.closeConnection()
    0
