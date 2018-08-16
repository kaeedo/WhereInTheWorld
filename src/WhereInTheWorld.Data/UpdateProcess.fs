namespace WhereInTheWorld.Data

open Hopac
open Hopac.Infixes
open WhereInTheWorld.Utilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities
open System

module UpdateProcess =
    let private defaultSubdivisionCode code countryCode =
        if String.IsNullOrWhiteSpace(code)
        then countryCode
        else code

    let private defaultSubdivisionName code countryName =
        if String.IsNullOrWhiteSpace(code)
        then countryName
        else code

    let updateCountry fileImports =
        job {
            let da = new DataAccess()
            let countryCode = (fileImports |> Seq.head).CountryCode
            let countryName = DataDownload.supportedCountries.[countryCode]

            let a =
                fileImports
                |> List.map (fun i ->
                    { Id = Unchecked.defaultof<int>
                      CountryCode = countryCode
                      CountryName = countryName
                      PostalCode = i.PostalCode
                      PlaceName = i.PlaceName
                      SubdivisionCode = defaultSubdivisionCode i.SubdivisionCode countryCode
                      SubdivisionName = defaultSubdivisionName i.SubdivisionName countryName
                      CountyName = i.CountyName
                      CountyCode = i.CountyCode
                      CommunityName = i.CommunityName
                      CommunityCode = i.CommunityCode
                      Latitude = i.Latitude
                      Longitude = i.Longitude
                      Accuracy = Some 0 }
                )

            do! da.InsertPostalCodes a

            (* let! countryId =
                { Id = Unchecked.defaultof<int64>
                  Code = countryCode
                  Name = countryName }
                |> DataAccess.insertCountry

            let! subdivisions =
                fileImports
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
               fileImports
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

            let! _ = DataAccess.insertPostalCodes postalCodeList *)

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

    let insertJob insertStatusPrinter imports =
        job {
            let ticker = Ticker(50)

            let insertStatusPrinterChannel = insertStatusPrinter ticker.Channel
            do! Job.foreverServer insertStatusPrinterChannel

            do! ticker.Channel *<- Started

            ticker.Start()

            let! updateCountryResult = updateCountry imports

            if updateCountryResult.IsOk
            then do! ticker.Channel *<- Inserted

            ticker.Stop()

            return updateCountryResult
        }

    let updateCountryProcess countryCode downloadStatusPrinter insertStatusPrinter =
        let workflow = (countryDownloadJob downloadStatusPrinter) >=> DataImport.readPostalCodeFile >=> insertJob insertStatusPrinter

        workflow countryCode |> run
