open System
open WhereInTheWorld.Update
open Hopac

[<EntryPoint>]
let main argv =
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
    0
