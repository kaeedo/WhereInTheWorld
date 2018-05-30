namespace WhereInTheWorld.Update

open Models
open Hopac
open Utilities
open System.IO

module UpdateProcess =
    let private getCountryInformation countryCode =
        DataDownload.supportedCountries
        |> Seq.find (fun sc ->
            let code, _, _ = sc
            code = countryCode
        )

    let updateCountry statusChannel countryCode =
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

                    do! Ch.give statusChannel (Inserted countryCode)

                    return Result.Ok countryCode
                with
                | e -> return Result.Error (countryCode, e)
        }

    let updateAll downloadStatusPrinter insertStatusPrinter =
        let downloadStatusChannel = Ch<DownloadStatus>()
        let insertStatusChannel = Ch<InsertStatus>()

        let countryDownloads =
            DataDownload.supportedCountries
            |> Seq.map (fun sc ->
                let code, _, _ = sc
                job {
                    let downloadStatusPrinterChannel = downloadStatusPrinter downloadStatusChannel
                    do! Job.foreverServer downloadStatusPrinterChannel

                    return! DataDownload.downloadPostalCodesForCountry downloadStatusChannel code
                }
            )
            |> Job.conCollect
            |> run

        let successfulDownloads =
            countryDownloads
            |> Seq.filter isOkResult

        let failedDownloads =
            countryDownloads
            |> Seq.filter isErrorResult

        successfulDownloads
        |> Seq.iter (function
            | Ok filePath ->
                let code = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last
                job {
                    let insertStatusPrinterChannel = insertStatusPrinter insertStatusChannel
                    do! Job.foreverServer insertStatusPrinterChannel

                    return! updateCountry insertStatusChannel code
                }
                |> run
                |> ignore
            | _ -> ()
        )

        // TODO: Properly report failed country downloads or inserts
        failedDownloads
        |> Seq.iter (fun error ->
            printfn "Downloading failed with message %A" error
        )
