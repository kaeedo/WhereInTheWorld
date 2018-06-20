open Argu
open System.IO
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Update

let ensureDirectory () =
    if not (Directory.Exists(Models.baseDirectory))
    then Directory.CreateDirectory(Models.baseDirectory) |> ignore

let printSupportedCountries () =
    let longestCountryLength =
        DataDownload.supportedCountries
        |> Seq.maxBy (fun (_, countryName, _) -> countryName.Length)
        |> fun (_, countryName, _) -> countryName.Length

    let longestLocalizedCountryLength =
        DataDownload.supportedCountries
        |> Seq.maxBy (fun (_, _, localizedName) -> localizedName.Length)
        |> fun (_, _, localizedName) -> localizedName.Length

    printfn "%s" <| String.replicate (longestCountryLength + longestLocalizedCountryLength + 12) "-"
    printf "|Code| %-*s" (longestCountryLength + 1) "Country"
    printfn "| %-*s|" (longestLocalizedCountryLength + 1) "Localized Name"
    printfn "%s" <| String.replicate (longestCountryLength + longestLocalizedCountryLength + 12) "-"

    DataDownload.supportedCountries
    |> Seq.iter (fun (countryCode, countryName, localizedName) ->
        printf "|%3s " countryCode
        printf "| %-*s " longestCountryLength countryName
        printfn "| %-*s |" longestLocalizedCountryLength localizedName
    )
    printfn "%s" <| String.replicate (longestCountryLength + longestLocalizedCountryLength + 12) "-"

let getPostalCodeInformation postalCode =
    printfn "Received postal code: %s" postalCode

let updateCountry (countryCode: string) =
    let uppercaseCountryCode = countryCode.ToUpperInvariant()

    if uppercaseCountryCode = "SUPPORTED"
    then
        printSupportedCountries()
    else
        let isValidCountryCode =
            DataDownload.supportedCountries
            |> Seq.exists (fun (code, _, _) ->
                code = uppercaseCountryCode
            )

        if not isValidCountryCode
        then printfn "%s is not a valid country code. \"witw --update supported\" to see a list of supported countries" countryCode
        else
            let updateJob = UpdateProcess.updateCountryProcess uppercaseCountryCode StatusPrinter.downloadStatusPrinter StatusPrinter.insertStatusPrinter
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

    let args = [|"--update"; "de"|]

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
            | Some countryCode -> updateCountry countryCode

    0
