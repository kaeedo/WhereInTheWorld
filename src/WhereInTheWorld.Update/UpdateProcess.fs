namespace WhereInTheWorld.Update

open Models
open Hopac
open Utilities
open System
open System.IO

module UpdateProcess =
    let private getCountryInformation countryCode =
        DataDownload.supportedCountries
        |> Seq.find (fun sc ->
            let code, _, _ = sc
            code = countryCode
        )

    let private defaultSubdivisionCode code countryCode =
        if String.IsNullOrWhiteSpace(code)
        then countryCode
        else code

    let private defaultSubdivisionName code countryName =
        if String.IsNullOrWhiteSpace(code)
        then countryName
        else code

    let updateCountry statusChannel countryCode =
        job {
            let! importedPostalCodes = DataImport.readPostalCodesFile countryCode

            match importedPostalCodes with
            | Error e -> return Result.Error (countryCode, e)
            | Ok import ->
                try
                    let countryCode, countryName, countryLocalizedName = getCountryInformation countryCode

                    let countryId =
                        { Id = Unchecked.defaultof<int64>
                          Code = countryCode
                          Name = countryName
                          LocalizedName = countryLocalizedName }
                        |> DataAccess.insertCountry

                    let subdivisions =
                        import
                        |> Seq.distinctBy (fun i ->
                            i.SubdivisionCode
                        )
                        |> Seq.map (fun fi ->
                            { Id = Unchecked.defaultof<int64>
                              CountryId = countryId
                              Code =  defaultSubdivisionCode fi.SubdivisionCode countryCode
                              Name = defaultSubdivisionName fi.SubdivisionName countryName }
                        )
                        |> List.ofSeq
                        |> DataAccess.insertSubdivisions
                        |> List.map (fun s ->
                            s.Code, s.Id
                        )
                        |> dict

                    let postalCodes =
                        import
                        |> Seq.map (fun i ->
                            let subdivisionCode =
                                if String.IsNullOrWhiteSpace(i.SubdivisionCode)
                                then countryCode
                                else i.SubdivisionCode

                            { Id = Unchecked.defaultof<int64>
                              SubdivisionId = subdivisions.[subdivisionCode]
                              PostalCode = i.PostalCode
                              PlaceName = i.PlaceName
                              CountyName = i.CountyName
                              CountyCode = i.CountyCode
                              CommunityName = i.CommunityName
                              CommunityCode = i.CommunityCode
                              Latitude = i.Latitude
                              Longitude = i.Longitude
                              Accuracy = i.Accuracy }
                        )
                        |> List.ofSeq
                        |> DataAccess.insertPostalCodes

                    do! Ch.give statusChannel (Inserted countryCode)

                    return Result.Ok countryCode
                with
                | e ->
                    return Result.Error (countryCode, e)
        }

    let updateCountryProcess countryCode downloadStatusPrinter insertStatusPrinter =
        let downloadStatusChannel = Ch<DownloadStatus>()
        let insertStatusChannel = Ch<InsertStatus>()

        let countryDownload =
            job {
                let downloadStatusPrinterChannel = downloadStatusPrinter downloadStatusChannel
                do! Job.foreverServer downloadStatusPrinterChannel

                return! DataDownload.downloadPostalCodesForCountry downloadStatusChannel countryCode
            }
            |> run

        let updateResult =
            match countryDownload with
            | Error (countryCode, e) -> Result.Error (countryCode, e)
            | Ok filePath ->
                let code = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last
                job {
                    let insertStatusPrinterChannel = insertStatusPrinter insertStatusChannel
                    do! Job.foreverServer insertStatusPrinterChannel

                    return! updateCountry insertStatusChannel code
                }
                |> run

        updateResult

    let updateAll downloadStatusPrinter insertStatusPrinter =
        let downloadStatusChannel = Ch<DownloadStatus>()
        let insertStatusChannel = Ch<InsertStatus>()

        job {
            let! countryDownloads =
                DataDownload.supportedCountries
                |> Seq.map (fun (code, _, _) ->
                    job {
                        let downloadStatusPrinterChannel = downloadStatusPrinter downloadStatusChannel
                        do! Job.foreverServer downloadStatusPrinterChannel

                        return! DataDownload.downloadPostalCodesForCountry downloadStatusChannel code
                    }
                )
                |> Job.conCollect

            let successfulDownloads =
                countryDownloads
                |> Seq.filter isOkResult

            let failedDownloads =
                countryDownloads
                |> Seq.filter isErrorResult

            let countryInsertions =
                successfulDownloads
                |> Seq.map (function
                    | Error (_, e) -> raise e
                    | Ok filePath ->
                        let code = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last
                        job {
                            let insertStatusPrinterChannel = insertStatusPrinter insertStatusChannel
                            do! Job.foreverServer insertStatusPrinterChannel

                            return! updateCountry insertStatusChannel code
                        }
                        |> run
                )

            let successfulInsertions =
                countryInsertions
                |> Seq.filter isOkResult

            let failedInsertions =
                countryInsertions
                |> Seq.filter isErrorResult

            return successfulInsertions, failedDownloads |> Seq.append failedInsertions
        }
        |> run
