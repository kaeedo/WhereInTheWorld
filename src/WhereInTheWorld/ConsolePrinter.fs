namespace WhereInTheWorld

open System
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Update
open Hopac

module ConsolePrinter =
    let downloadStatusPrinter channel =
        job {
            let! status = Ch.take channel

            match status with
            | DownloadStatus.Started country ->
                printfn "Downloading data for country: %s" country
            | Completed country ->
                printfn "Finished downloading data for country: %s" country
        }

    let insertStatusPrinter message =
        job {
            let! status = Ch.take message

            match status with
            | Progress symbol ->
                Console.SetCursorPosition(0, Console.CursorTop)
                printf "%s" symbol
            | Started -> printfn "Inserting postal code data into local database"
            | Inserted -> printfn "\nFinished inserting postal code data into local database"
        }

    let printCountries (countryList: seq<string * string>) =
        let longestCountryLength =
            countryList
            |> Seq.maxBy (fun (_, countryName) -> countryName.Length)
            |> fun (_, countryName) -> countryName.Length

        printfn "%s" <| String.replicate (longestCountryLength + 9) "-"
        printfn "|Code| %-*s|" (longestCountryLength + 1) "Country"
        printfn "%s" <| String.replicate (longestCountryLength + 9) "-"

        countryList
        |> Seq.iter (fun (countryCode, countryName) ->
            printf "|%3s " countryCode
            printfn "| %-*s |" longestCountryLength countryName
        )
        printfn "%s" <| String.replicate (longestCountryLength + 9) "-"

    let printQueryResults postalCode (postalCodeInformation: PostalCodeDao seq) numberOfResults =
        printfn "Information about \"%s\" (found %i %s):" postalCode numberOfResults (if numberOfResults = 1 then "result" else "results")
        printfn "%s" <| String.replicate 50 "-"
        postalCodeInformation
        |> Seq.iter (fun pci ->
            printfn "Place name: %s with postal code: %s" pci.PlaceName pci.PostalCode

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
