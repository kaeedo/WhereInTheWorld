open Argu
open System.IO
open System.Reflection
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Update
open WhereInTheWorld.Query
open WhereInTheWorld.Utilities
open WhereInTheWorld.Utilities.IoUtilities

let ensureDirectory () =
    if not (Directory.Exists(Models.baseDirectory))
    then Directory.CreateDirectory(Models.baseDirectory) |> ignore

let ensureDatabase () =
    let currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
    if not (File.Exists(Models.databaseFile))
    then File.Copy(currentDirectory @@ "world.db", Models.baseDirectory @@ "world.db")

let printSupportedCountries () =
    let longestCountryLength =
        DataDownload.supportedCountries
        |> Seq.maxBy (fun (_, countryName) -> countryName.Length)
        |> fun (_, countryName) -> countryName.Length

    printfn "%s" <| String.replicate (longestCountryLength + 9) "-"
    printfn "|Code| %-*s|" (longestCountryLength + 1) "Country"
    printfn "%s" <| String.replicate (longestCountryLength + 9) "-"

    DataDownload.supportedCountries
    |> Seq.iter (fun (countryCode, countryName) ->
        printf "|%3s " countryCode
        printfn "| %-*s |" longestCountryLength countryName
    )
    printfn "%s" <| String.replicate (longestCountryLength + 9) "-"

let getPostalCodeInformation postalCode =
    let postalCodeInformation = Query.getInformation postalCode
    let numberOfResults = Seq.length postalCodeInformation

    if numberOfResults = 0
    then
        printfn "No information found for postal code: \"%s\"." postalCode
    else
        printfn "Information about \"%s\" (found %i %s):" postalCode numberOfResults (if numberOfResults = 1 then "result" else "results")
        printfn "%s" <| String.replicate 50 "-"
        postalCodeInformation
        |> Seq.iter (fun pci ->
            printfn "Place name: %s" pci.PlaceName

            if pci.CommunityName.IsSome
            then
                printf "%4sIn Community: %s" "" pci.CommunityName.Value
                if pci.CommunityCode.IsSome
                then printf " (%s)" pci.CommunityCode.Value
                printfn ""

            if pci.CountyName.IsSome
            then
                printf "%4sIn County: %s" "" pci.CountyName.Value
                if pci.CountyCode.IsSome
                then printf " (%s)" pci.CountyCode.Value
                printfn ""

            printfn "%4sWithin Subdivision: %s (%s)" "" pci.Subdivision.Name pci.Subdivision.Code
            printfn "%4sIn Country: %s (%s)" "" pci.Subdivision.Country.Name pci.Subdivision.Country.Code
            printfn "%s" <| String.replicate 25 "-"
        )

let updateCountry (countryCode: string) =
    let uppercaseCountryCode = countryCode.ToUpperInvariant()

    if uppercaseCountryCode = "SUPPORTED"
    then
        printSupportedCountries()
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
                UpdateProcess.updateCountryProcess uppercaseCountryCode StatusPrinter.downloadStatusPrinter StatusPrinter.insertStatusPrinter
            match updateJob with
            | Ok _ -> printfn "Successfully update country: %s" countryCode
            | Error (countryCode, e) -> printfn "%s failed with message %s" countryCode e.Message

let updateAll () =
    let successfulUpdates, failedUpdates =
            UpdateProcess.updateAll StatusPrinter.downloadStatusPrinter StatusPrinter.insertStatusPrinter

    printfn "Succesfully updated %i countries" (successfulUpdates |> Seq.length)

    if failedUpdates |> Seq.isEmpty
    then ()
    else
        printfn "Problem updating the following"
        failedUpdates
        |> Seq.iter (function
            | Ok _ -> ()
            | Error (countryCode, e) -> printfn "%s failed with message %A" countryCode e
        )

let parser = ArgumentParser.Create<Arguments>(programName = "witw")

[<EntryPoint>]
let main argv =
    ensureDirectory()
    ensureDatabase()

    let args = [|"--update"; "de"|]
    //let args = [|"01983"|]

    let arguments = parser.Parse args

    if args |> Seq.isEmpty || arguments.IsUsageRequested
    then printfn "%s" <| parser.PrintUsage()
    else
        let hasPostalCode = arguments.Contains PostalCode
        let hasUpdate = arguments.Contains Update

        if hasPostalCode && hasUpdate
        then printfn "%s" <| parser.PrintUsage()
        elif hasPostalCode && not hasUpdate
        then getPostalCodeInformation <| arguments.GetResult PostalCode
        elif not hasPostalCode && hasUpdate
        then
            match arguments.GetResult Update with
            | None -> updateAll()
            | Some countryCode ->
                updateCountry countryCode

    0
