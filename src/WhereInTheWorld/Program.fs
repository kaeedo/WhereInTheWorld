open System.IO
open WhereInTheWorld.Update
open WhereInTheWorld.Update.Utilities
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

    let updateProcess =
        DataDownload.supportedCountries
        |> Seq.map (fun sc ->
            let code, _, _ = sc
            (DataDownload.downloadPostalCodesForCountry
            >=> DataImport.readPostalCodesFile) code
        )
        |> Job.conCollect
        |> run
        |> Seq.filter (function
            | Error _ -> false
            |_ -> true
        )
        |> Seq.collect (fun pcs ->
            let (Ok postalCodes) = pcs
            postalCodes
        )
        |> batchesOf 50_000
        |> Seq.iter (fun b ->
            b
            |> UpdateProcess.updateAll
            |> function
            | Error e -> printfn "Didn't work because %A" e
            | Ok o -> printfn "Succeeded"
        )

    (* |> Seq.iter (fun batch ->
        batch
        |> Seq.map (fun sc ->
            let code, _, _ = sc
            UpdateProcess.updateCountry code
        )
        |> Job.conCollect
        |> run
        |> Seq.iter (fun cd ->
            match cd with
            | Error error ->
                printfn "Country download for %s failed with message: %A" "" error
            | Ok countryCode -> printfn "Country download for %s succeeded" countryCode
        )
    ) *)


    stopWatch.Stop()
    printfn "Total time took %fms" stopWatch.Elapsed.TotalMilliseconds

    DataAccess.closeConnection()
    0
