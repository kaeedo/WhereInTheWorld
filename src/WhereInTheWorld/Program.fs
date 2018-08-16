open Argu
open System.IO
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Data
open WhereInTheWorld.Utilities

let ensureCleanDirectory () =
    if not (Directory.Exists(Models.baseDirectory))
    then Directory.CreateDirectory(Models.baseDirectory) |> ignore

    Directory.EnumerateFiles(Models.baseDirectory)
    |> Seq.filter (fun f -> f.EndsWith("zip") || f.EndsWith("txt"))
    |> Seq.iter File.Delete
let getPostalCodeInformation postalCode =
    let da = new DataAccess()
    da.EnsureDatabase()
    let postalCodeInformation = []//Query.getPostalCodeInformation postalCode
    let numberOfResults = Seq.length postalCodeInformation

    if numberOfResults = 0
    then
        printfn "No information found for postal code: \"%s\"" postalCode
    else
        ConsolePrinter.printQueryResults postalCode postalCodeInformation numberOfResults

let updateCountry (countryCode: string) =
    let da = new DataAccess()
    da.EnsureDatabase()
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
            let innermost = e.GetBaseException()
            do ErrorLog.writeException innermost
            match innermost with
            (* | :? SQLiteException ->
                printfn "Problem with the database. Please try again. If the problem persists, try running \"witw --cleardatabase\" to start fresh." *)
            | _ ->
                printfn "Following error occured: %s Please try again. If the problem persists, please report the error along with the latest error log from %s" e.Message Models.baseDirectory

let parser = ArgumentParser.Create<Arguments>(programName = "witw")

[<EntryPoint>]
let main argv =
    ensureCleanDirectory()

    if argv |> Array.contains("--help")
    then printfn "%s" <| parser.PrintUsage()
    else
        let arguments = parser.Parse argv

        if argv |> Seq.isEmpty || arguments.IsUsageRequested
        then printfn "%s" <| parser.PrintUsage()
        else
            let hasInfo = arguments.Contains Info
            let hasPostalCode = arguments.Contains PostalCode
            let hasUpdate = arguments.Contains Update
            let hasList = arguments.Contains List
            let hasClearDatabase = arguments.Contains ClearDatabase

            if hasInfo
            then printfn "info"
            elif hasPostalCode && hasUpdate
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
                    | Available ->
                        printfn "available"
                        // Query.getAvailableCountries()
                        // |> Option.iter ConsolePrinter.printCountries
            elif hasClearDatabase
            then
                let da = new DataAccess()
                da.ClearDatabase()
            elif not hasPostalCode && hasUpdate
            then
                match arguments.GetResult Update with
                | None -> printfn "%s" <| parser.PrintUsage()
                | Some countryCode ->
                    updateCountry countryCode

    0
