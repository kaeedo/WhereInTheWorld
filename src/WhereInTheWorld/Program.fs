﻿open Argu
open System.IO
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Update
open WhereInTheWorld.Query
open WhereInTheWorld.Utilities
open System.Data.SQLite

let ensureCleanDirectory () =
    if not (Directory.Exists(Models.baseDirectory))
    then Directory.CreateDirectory(Models.baseDirectory) |> ignore
    Directory.EnumerateFiles(Models.baseDirectory)

    |> Seq.filter (fun f -> f.EndsWith("zip") || f.EndsWith("txt"))
    |> Seq.iter File.Delete


let getPostalCodeInformation postalCode =
    let postalCodeInformation = Query.getPostalCodeInformation postalCode
    let numberOfResults = Seq.length postalCodeInformation

    if numberOfResults = 0
    then
        printfn "No information found for postal code: \"%s\"" postalCode
    else
        ConsolePrinter.printQueryResults postalCode postalCodeInformation numberOfResults

let updateCountry (countryCode: string) =
    let uppercaseCountryCode = countryCode.ToUpperInvariant()

    let isValidCountryCode =
        DataDownload.supportedCountries
        |> Seq.exists (fun (code, _) ->
            code = uppercaseCountryCode
        )

    if not isValidCountryCode
    then printfn "%s is not a valid country code. \"witw --supported\" to see a list of supported countries" countryCode
    else
        let updateJob: Result<_, exn> =
            UpdateProcess.updateCountryProcess uppercaseCountryCode ConsolePrinter.downloadStatusPrinter ConsolePrinter.insertStatusPrinter
        match updateJob with
        | Ok _ -> printfn "Successfully updated country: %s" uppercaseCountryCode
        | Error e ->
            let innermost = e.GetBaseException()
            match innermost with
            | :? IOException ->
                printfn "Text file already exists"
            | :? SQLiteException ->
                printfn "Problem with the database"
            | _ ->
                printfn "%s failed with message %s" uppercaseCountryCode e.StackTrace

let parser = ArgumentParser.Create<Arguments>(programName = "witw")

[<EntryPoint>]
let main argv =
    ensureCleanDirectory()
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
            let hasList = arguments.Contains List

            if hasPostalCode && hasUpdate
            then printfn "%s" <| parser.PrintUsage()
            elif hasPostalCode && not hasUpdate
            then getPostalCodeInformation <| arguments.GetResult PostalCode
            elif hasList
            then
                match arguments.GetResult List with
                | None -> printfn "%s" <| parser.PrintUsage()
                | Some list ->
                    match list with
                    | Supported -> ConsolePrinter.printCountries DataDownload.supportedCountries
                    | Available -> ConsolePrinter.printCountries (Query.getAvailableCountries())
            elif not hasPostalCode && hasUpdate
            then
                match arguments.GetResult Update with
                | None -> printfn "%s" <| parser.PrintUsage()
                | Some countryCode ->
                    updateCountry countryCode

    0
