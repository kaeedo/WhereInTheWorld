open System
open System.IO
open WhereInTheWorld.Update
open Hopac

let ensureDirectory () =
    if Directory.Exists(DataDownload.baseSaveDirectory)
    then Directory.Delete(DataDownload.baseSaveDirectory, true)
    Directory.CreateDirectory(DataDownload.baseSaveDirectory) |> ignore

[<EntryPoint>]
let main argv =
    ensureDirectory()
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    DataDownload.supportedCountries
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
    stopWatch.Stop()
    printfn "Total time took %fms" stopWatch.Elapsed.TotalMilliseconds
    0
