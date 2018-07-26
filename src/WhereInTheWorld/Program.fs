﻿open Argu
open System.IO
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Update
open WhereInTheWorld.Query
open WhereInTheWorld.Utilities

let ensureDirectory () =
    if not (Directory.Exists(Models.baseDirectory))
    then Directory.CreateDirectory(Models.baseDirectory) |> ignore

let getPostalCodeInformation postalCode =
    let postalCodeInformation = Query.getInformation postalCode
    let numberOfResults = Seq.length postalCodeInformation

    if numberOfResults = 0
    then
        printfn "No information found for postal code: \"%s\"." postalCode
    else
        ConsolePrinter.printQueryResults postalCode postalCodeInformation numberOfResults

let updateCountry (countryCode: string) =
    let uppercaseCountryCode = countryCode.ToUpperInvariant()

    if uppercaseCountryCode = "SUPPORTED"
    then
        ConsolePrinter.printSupportedCountries()
    else
        let isValidCountryCode =
            DataDownload.supportedCountries
            |> Seq.exists (fun (code, _) ->
                code = uppercaseCountryCode
            )

        if not isValidCountryCode
        then printfn "%s is not a valid country code. \"witw --update supported\" to see a list of supported countries" countryCode
        else
            let updateJob =
                UpdateProcess.updateCountryProcess uppercaseCountryCode ConsolePrinter.downloadStatusPrinter ConsolePrinter.insertStatusPrinter
            match updateJob with
            | Ok _ -> printfn "Successfully update country: %s" countryCode
            | Error e -> printfn "%s failed with message %s" countryCode e.StackTrace

let updateAll () =
    let successfulUpdates, failedUpdates =
            UpdateProcess.updateAll ConsolePrinter.downloadStatusPrinter ConsolePrinter.insertStatusPrinter

    printfn "Succesfully updated %i countries" (successfulUpdates |> Seq.length)

    if failedUpdates |> Seq.isEmpty
    then ()
    else
        printfn "Problem updating the following"
        failedUpdates
        |> Seq.iter (function
            | Ok _ -> ()
            | Error e -> printfn "Update failed with message %A" e
        )

let parser = ArgumentParser.Create<Arguments>(programName = "witw")

[<EntryPoint>]
let main argv =
    ensureDirectory()
    DataAccess.ensureDatabase()

    if argv |> Array.contains("--help")
    then printfn "%s" <| parser.PrintUsage()
    else
        let arguments = parser.Parse argv

        if argv |> Seq.isEmpty || arguments.IsUsageRequested
        then printfn "%s" <| parser.PrintUsage()
        else
            let hasPostalCode = arguments.Contains PostalCode
            let hasUpdate = arguments.Contains Update
            let hasSupported = arguments.Contains Supported

            if hasPostalCode && hasUpdate
            then printfn "%s" <| parser.PrintUsage()
            elif hasPostalCode && not hasUpdate
            then getPostalCodeInformation <| arguments.GetResult PostalCode
            elif hasSupported
            then ConsolePrinter.printSupportedCountries()
            elif not hasPostalCode && hasUpdate
            then
                match arguments.GetResult Update with
                | None -> updateAll()
                | Some countryCode ->
                    updateCountry countryCode

    0
