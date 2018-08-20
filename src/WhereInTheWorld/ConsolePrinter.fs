namespace WhereInTheWorld

open System
open System.Data.SQLite

open Hopac

open WhereInTheWorld.Utilities.Models

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

    let printCountries (countryList: Map<string, string>) =
        let longestCountryLength =
            countryList
            |> Map.toSeq
            |> Seq.maxBy (fun (_, countryName) -> countryName.Length)
            |> fun (_, countryName) -> countryName.Length

        printfn "%s" <| String.replicate (longestCountryLength + 9) "-"
        printfn "|Code| %-*s|" (longestCountryLength + 1) "Country"
        printfn "%s" <| String.replicate (longestCountryLength + 9) "-"

        countryList
        |> Map.iter (fun countryCode countryName ->
            printf "|%3s " countryCode
            printfn "| %-*s |" longestCountryLength countryName
        )
        printfn "%s" <| String.replicate (longestCountryLength + 9) "-"

    let printQueryResults postalCode (postalCodeInformation: PostalCodeInformation seq) numberOfResults =
        printfn "Information about \"%s\" (found %i %s):" postalCode numberOfResults (if numberOfResults = 1 then "result" else "results")
        printfn "%s" <| String.replicate 50 "-"
        postalCodeInformation
        |> Seq.iter (fun pci ->
            let foundPostalCode = if pci.PostalCode = postalCode then String.Empty else sprintf " with postal code: %s" pci.PostalCode
            printfn "Place name: %s%s" pci.PlaceName foundPostalCode

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

            printfn "%4sWithin Subdivision: %s (%s)" String.Empty pci.SubdivisionName pci.SubdivisionCode
            printfn "%4sIn Country: %s (%s)" String.Empty pci.CountryName pci.CountryCode
            printfn "%s" <| String.replicate 25 "-"
        )

    let printErrorMessage (e: exn) errorWriter =
        let innermost = e.GetBaseException()
        do errorWriter innermost
        match innermost with
        | :? SQLiteException ->
            printfn "Problem with the database. Please try again. If the problem persists, try running \"witw --cleardatabase\" to start fresh."
        | _ ->
            printfn "Following error occured: %s Please try again. If the problem persists, please report the error along with the latest error log from %s" e.Message baseDirectory
