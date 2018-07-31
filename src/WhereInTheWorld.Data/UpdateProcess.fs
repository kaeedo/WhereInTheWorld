namespace WhereInTheWorld.Data

open Hopac
open Hopac.Infixes
open WhereInTheWorld.Utilities
open WhereInTheWorld.Utilities.Models
open System
open System.IO

module UpdateProcess =
    let private defaultSubdivisionCode code countryCode =
        if String.IsNullOrWhiteSpace(code)
        then countryCode
        else code

    let private defaultSubdivisionName code countryName =
        if String.IsNullOrWhiteSpace(code)
        then countryName
        else code

    let updateCountry countryCode =
        job {
            let! importedPostalCodes = DataImport.readPostalCodeFile countryCode

            match importedPostalCodes with
            | Error e -> return Result.Error e
            | Ok import ->
                let countryName = DataDownload.supportedCountries.[countryCode]

                let! countryId =
                    { Id = Unchecked.defaultof<int64>
                      Code = countryCode
                      Name = countryName }
                    |> DataAccess.insertCountry

                let! subdivisions =
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

                let subdivisionsDictionary =
                    subdivisions
                    |> List.map (fun s ->
                        s.Code, s.Id
                    )
                    |> dict

                let postalCodeList =
                   import
                   |> Seq.map (fun i ->
                        let subdivisionCode =
                            if String.IsNullOrWhiteSpace(i.SubdivisionCode)
                            then countryCode
                            else i.SubdivisionCode

                        { Id = Unchecked.defaultof<int64>
                          SubdivisionId = subdivisionsDictionary.[subdivisionCode]
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

                let! _ = DataAccess.insertPostalCodes postalCodeList

                return Result.Ok countryCode
        }

    let countryDownloadJob printer countryCode =
        let downloadStatusChannel = Ch<DownloadStatus>()

        job {
            let downloadStatusPrinterChannel = printer downloadStatusChannel
            do! Job.foreverServer downloadStatusPrinterChannel

            let! _ = DataDownload.downloadPostalCodesForCountry downloadStatusChannel countryCode

            return Result.Ok countryCode
        }

    let updateCountryJob insertStatusPrinter countryCode =
        job {
            let ticker = Ticker(50)

            let insertStatusPrinterChannel = insertStatusPrinter ticker.Channel
            do! Job.foreverServer insertStatusPrinterChannel

            do! ticker.Channel *<- Started

            ticker.Start()
            let! updateCountryResult = updateCountry countryCode
            do! ticker.Channel *<- Inserted
            ticker.Stop()

            return updateCountryResult
        }

    let updateCountryProcess countryCode downloadStatusPrinter insertStatusPrinter =
        try
            let countryDownload =
                countryDownloadJob downloadStatusPrinter countryCode |> run

            let updateResult =
                match countryDownload with
                | Error e -> Result.Error e
                | Ok filePath ->
                    let code = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last
                    updateCountryJob insertStatusPrinter code |> run

            Result.Ok updateResult
        with
        | :? Exception as e -> Result.Error e
