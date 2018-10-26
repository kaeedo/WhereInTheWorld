namespace WhereInTheWorld

open Argu
open System.IO
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Data
open WhereInTheWorld.Utilities

module Main =
    let ensureCleanDirectory () =
        if not (Directory.Exists(Models.baseDirectory)) 
        then Directory.CreateDirectory(Models.baseDirectory) |> ignore

        Directory.EnumerateFiles(Models.baseDirectory)
        |> Seq.filter (fun f -> f.EndsWith("zip") || f.EndsWith("txt"))
        |> Seq.iter File.Delete

    let queryDatabase (input: string) =
        Database.ensureDatabase()

        Query.getSearchResult input

    let updateCountry (countryCode: string) =
        Database.ensureDatabase()
        let uppercaseCountryCode = countryCode.ToUpperInvariant()

        let isValidCountryCode =
            DataDownload.supportedCountries
            |> Map.exists (fun key _ -> key = uppercaseCountryCode)

        if not isValidCountryCode
        then printfn "%s is not a valid country code. \"witw --supported\" to see a list of supported countries" countryCode
        else
            let updateJob: Result<_, exn> =
                UpdateProcess.updateCountryProcess uppercaseCountryCode ConsolePrinter.downloadStatusPrinter ConsolePrinter.insertStatusPrinter
            match updateJob with
            | Ok _ -> printfn "Successfully updated country: %s" uppercaseCountryCode
            | Error e ->
                ConsolePrinter.printErrorMessage e ErrorLog.writeException

    [<EntryPoint>]
    let main argv =
        let parser = ArgumentParser.Create<Arguments>(programName = "witw")
        ensureCleanDirectory()

        if argv |> Array.contains("--help")
        then printfn "%s" <| parser.PrintUsage()
        else
            let arguments = parser.Parse argv

            if argv |> Seq.isEmpty || arguments.IsUsageRequested
            then printfn "%s" <| parser.PrintUsage()
            else
                match arguments with
                | HasQuery -> 
                    let input = arguments.GetResult SearchQuery
                    let results = queryDatabase input
                    match results with
                    | Error e -> ConsolePrinter.printErrorMessage e ErrorLog.writeException
                    | Ok information ->
                        if Seq.isEmpty information
                        then
                            printfn "No information found for: \"%s\"" input
                        else
                            ConsolePrinter.printQueryResults input information
                | UpdateCountry ->
                    match arguments.GetResult Update with
                    | None -> printfn "%s" <| parser.PrintUsage()
                    | Some countryCode ->
                        updateCountry countryCode
                | ListAvailable ->
                    Database.ensureDatabase()
                    match Query.getAvailableCountries () with
                    | Ok countries ->
                        match countries with
                        | None -> printfn "No countries have been updated yet"
                        | Some c -> ConsolePrinter.printCountries c
                    | Error e ->
                        ConsolePrinter.printErrorMessage e ErrorLog.writeException
                | ListSupported -> ConsolePrinter.printCountries DataDownload.supportedCountries
                | HasClearDatabase -> Database.clearDatabase()
                | _ -> printfn "%s" <| parser.PrintUsage()

        0
