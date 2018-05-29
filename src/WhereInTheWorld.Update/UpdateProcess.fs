namespace WhereInTheWorld.Update

open Models
open Hopac
open Utilities
open System.IO
open System.Globalization

module UpdateProcess =
    let private getCountryInformation countryCode =
        DataDownload.supportedCountries
        |> Seq.find (fun sc ->
            let code, _, _ = sc
            code = countryCode
        )

    let updateCountry countryCode =
        job {
            let! importedPostalCodes = DataImport.readPostalCodesFile countryCode

            match importedPostalCodes with
            | Error e -> return Result.Error (countryCode, e)
            | Ok import ->
                try
                    let countryCode, countryName, countryLocalizedName = getCountryInformation countryCode

                    do! import
                        |> Seq.map (fun i ->
                            { Id = Unchecked.defaultof<int>
                              CountryCode = countryCode
                              CountryName = countryName
                              CountryLocalizedName = countryLocalizedName
                              PostalCode = i.PostalCode
                              PlaceName = i.PlaceName
                              SubdivisionCode = i.SubdivisionCode
                              SubdivisionName = i.SubdivisionName
                              CountyName = i.CountyName
                              CountyCode = i.CountyCode
                              CommunityName = i.CommunityName
                              CommunityCode = i.CommunityCode
                              Latitude = i.Latitude
                              Longitude = i.Longitude
                              Accuracy = i.Accuracy }
                        )
                        |> DataAccess.insertPostalCodes

                    return Result.Ok countryCode
                with
                | e -> return Result.Error (countryCode, e)
        }

    let updateAll () =
        let stopWatch = System.Diagnostics.Stopwatch.StartNew()

        printfn "Downloading all country postal codes"
        // let countryDownloads =
        //     DataDownload.supportedCountries
        //     |> Seq.map (fun sc ->
        //         let code, _, _ = sc
        //         DataDownload.downloadPostalCodesForCountry code
        //     )
        //     |> Job.conCollect
        //     |> run

        // let successfulDownloads =
        //     countryDownloads
        //     |> Seq.filter isOkResult

        // let failedDownloads =
        //     countryDownloads
        //     |> Seq.filter isErrorResult

        // printfn "Finished downloading all country postal codes. Took %sms" (stopWatch.ElapsedMilliseconds.ToString("n2"))
        // printfn "%i/%i succeeded" (successfulDownloads |> Seq.length) (DataDownload.supportedCountries |> Seq.length)

        // stopWatch.Restart()
        // printfn "Inserting postal codes into database"

        // successfulDownloads
        // |> Seq.map (function
        //     | Ok filePath ->
        //         let code = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last
        //         updateCountry code
        //         |> run
        //     | Error e ->
        //          Error ("Downloading failed with message", e)
        // )
        // |> Seq.iter (fun cd ->
        //     match cd with
        //     | Error e ->
        //         let countryCode, error = e
        //         printfn "%s: Inserting into database failed with message: %A" countryCode error
        //     | Ok countryCode -> printfn "%s: Inserting succeeded" countryCode
        // )

        // // TODO: Properly report failed country downloads or inserts
        // failedDownloads
        // |> Seq.iter (fun error ->
        //     printfn "Downloading failed with message %A" error
        // )

        stopWatch.Stop()
        printfn "Total time inserting took %sms" (stopWatch.ElapsedMilliseconds.ToString("n2", CultureInfo.InvariantCulture))
