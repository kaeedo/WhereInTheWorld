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
            let countryCode = (fileImports |> Seq.head).CountryCode
            let countryName = DataDownload.supportedCountries.[countryCode]

            let! _ =
                fileImports
                |> List.map (fun fi ->
                    { fi with
                        PostalCodeInformation.CountryName = countryName
                        CountryCode = countryCode
                        SubdivisionCode = defaultSubdivisionCode fi.SubdivisionCode countryCode
                        SubdivisionName = defaultSubdivisionName fi.SubdivisionName countryName }
                )
                |> DataAccess.insertPostalCodes

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
