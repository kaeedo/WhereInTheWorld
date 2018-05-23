﻿open System
open System.IO
open WhereInTheWorld.Update
open Hopac
open System.Net

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

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()

    DataDownload.supportedCountries
    |> Seq.rev
    |> Seq.iter (fun sc ->
        let code, _, _ = sc
        let result = DataDownload.downloadPostalCodesForCountry code |> run
        match result with
        | Error (error: Exception) ->
            printfn "Country download for %s failed with message: %A" code error
        | Ok countryCode -> printfn "Country download for %s succeeded" countryCode
    )

    (*let a =
        DataDownload.supportedCountries
        |> batchesOf 5
        |> Seq.toList

    DataAccess.openConnection()

    DataDownload.supportedCountries
    |> Seq.map (fun sc ->
        let code, _, _ = sc
        UpdateProcess.updateCountry code
    )
    |> Seq.iter (fun cd ->
        match cd |> run with
        | Error (error: string * Exception) ->
            let countryCode, e = error
            printfn "Country download for %s failed with message: %A" countryCode e
        | Ok countryCode -> printfn "Country download for %s succeeded" countryCode
    )

    a.[0..2]
    |> Seq.iter(fun sequence ->
        sequence
        |> Seq.map (fun sc ->
            let code, _, _ = sc
            UpdateProcess.updateCountry code
        )
        |> Job.conCollect
        |> run
        |> Seq.iter (fun cd ->
            match cd with
            | Error (error: string * Exception) ->
                let countryCode, e = error
                printfn "Country download for %s failed with message: %A" countryCode e
            | Ok countryCode -> printfn "Country download for %s succeeded" countryCode
        )
    )

    DataDownload.supportedCountries.[0..7]
    |> Seq.map (fun sc ->
        let code, _, _ = sc
        UpdateProcess.updateCountry code
    )
    |> Job.conCollect
    |> run
    |> Seq.iter (fun cd ->
        match cd with
        | Error (error: string * Exception) ->
            let countryCode, e = error
            printfn "Country download for %s failed with message: %A" countryCode e
        | Ok countryCode -> printfn "Country download for %s succeeded" countryCode
    )*)

    stopWatch.Stop()
    printfn "Total time took %fms" stopWatch.Elapsed.TotalMilliseconds
    DataAccess.closeConnection()
    0
