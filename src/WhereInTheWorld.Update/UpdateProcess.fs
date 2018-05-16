namespace WhereInTheWorld.Update

open Utilities
open Models
open Hopac
open System.IO

module UpdateProcess =
    let private createSubdivions fileImport =
        fileImport
        |> Seq.map (fun i ->
            { Id = 1; CountryId = 1; Code = i.SubdivisionCode; Name = i.SubdivisionName; }
        )
        |> Seq.distinctBy (fun sd -> sd.Code)

    let private getCountryInformation countryCode =
        DataDownload.supportedCountries
        |> Seq.find (fun sc ->
            let code, _, _ = sc
            code = countryCode
        )

    let private getUniqueSubdivisions import =
        import
        |> Seq.map (fun i ->
            { Id = 1; CountryId = 1; Code = i.SubdivisionCode; Name = i.SubdivisionName; }
        )
        |> Seq.distinctBy (fun sd -> sd.Code)

    let updateCountry countryCode =
        job {
            let! importedPostalCodes =
                (DataDownload.downloadPostalCodesForCountry
                 >=> DataImport.readPostalCodesFile) countryCode

            match importedPostalCodes with
            | Error e -> return Result.Error (countryCode, e)
            | Ok import ->
                try
                    let countryCode, countryName, countryLocalizedName = getCountryInformation countryCode

                    let uniqueSubdivisions = getUniqueSubdivisions import

                    let! countryId =
                        DataAccess.insertCountryGetId { Id = 1; Code = countryCode; Name = countryName; LocalizedName = countryLocalizedName }

                    do! DataAccess.insertSubdivisions countryId uniqueSubdivisions

                    let! subdivisions = DataAccess.getSubdivisions uniqueSubdivisions

                    let getSubdivisionId fileImport subdivisions =
                        let subdivision =
                            subdivisions
                            |> Seq.find (fun sd -> sd.Code = fileImport.SubdivisionCode)

                        subdivision.Id

                    do! import
                        |> Seq.map (fun i ->
                            { Id = 1
                              PostalCode = i.PostalCode
                              PlaceName = i.PlaceName
                              SubdivisionId = subdivisions |> getSubdivisionId i
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
