namespace WhereInTheWorld

open System
open System.Data.SQLite

open Hopac

open WhereInTheWorld.Utilities.Models
open System.Text

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

    let printQueryResults postalCode (postalCodeInformation: PostalCodeInformation seq) =
        let places =
            postalCodeInformation
            |> Seq.groupBy (fun information ->
                (information.PlaceName, information.SubdivisionCode, information.CountryCode)
            )

        let numberOfResults = places |> Seq.length
        printfn "Information about \"%s\" (found %i %s):" postalCode numberOfResults (if numberOfResults = 1 then "result" else "results")
        printfn "%s" <| String.replicate 50 "-"

        places
        |> Seq.iter (fun placeInformation ->
            let (placeName, _, _) = fst placeInformation
            let placeInformation = snd placeInformation |> List.ofSeq

            if placeInformation |> Seq.length = 1
            then
                printfn "Place name: %s with postal code: %s" placeName placeInformation.[0].PostalCode
            else
                printfn "Place name: %s has following postal codes:" placeName
                placeInformation
                |> Seq.map (fun pc ->
                    pc.PostalCode
                )
                |> String.concat  (", ")
                |> printfn "%s"

            let allInformation = placeInformation.[0]
            if allInformation.CommunityName.IsSome
            then
                printf "%4sIn Community: %s" "" allInformation.CommunityName.Value
                if allInformation.CommunityCode.IsSome
                then printf " (%s)" allInformation.CommunityCode.Value
                printfn ""

            if allInformation.CountyName.IsSome
            then
                printf "%4sIn County: %s" "" allInformation.CountyName.Value
                if allInformation.CountyCode.IsSome
                then printf " (%s)" allInformation.CountyCode.Value
                printfn ""

            printfn "%4sWithin Subdivision: %s (%s)" String.Empty allInformation.SubdivisionName allInformation.SubdivisionCode
            printfn "%4sIn Country: %s (%s)" String.Empty allInformation.CountryName allInformation.CountryCode
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
