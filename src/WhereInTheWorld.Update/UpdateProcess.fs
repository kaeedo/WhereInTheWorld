namespace WhereInTheWorld.Update

open Utilities
open Models
open Hopac

module UpdateProcess =
    let private getCountryInformation countryCode =
        DataDownload.supportedCountries
        |> Seq.find (fun sc ->
            let code, _, _ = sc
            code = countryCode
        )

    let updateAll postalCodes =
        try
            //let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            printfn "%i" (postalCodes |> Seq.length)
            //let countryCode, countryName, countryLocalizedName = getCountryInformation countryCode

            do postalCodes
                |> Seq.map (fun i ->
                    { Id = Unchecked.defaultof<int>
                      CountryCode = (getCountryInformation i.CountryCode).Item1
                      CountryName = (getCountryInformation i.CountryCode).Item2
                      CountryLocalizedName = (getCountryInformation i.CountryCode).Item3
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
                |> run

            //stopWatch.Stop()

            Result.Ok "Done"
        with
        | e -> Result.Error ("", e)


    (* let updateCountry countryCode =
        job {
            let! importedPostalCodes =
                (DataDownload.downloadPostalCodesForCountry
                >=> DataImport.readPostalCodesFile) countryCode

            match importedPostalCodes with
            | Error e -> return Result.Error (countryCode, e)
            | Ok import ->
                try
                    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
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

                    stopWatch.Stop()
                    printfn "%s: Inserting postal codes took %fms" countryCode stopWatch.Elapsed.TotalMilliseconds

                    return Result.Ok countryCode
                with
                | e -> return Result.Error (countryCode, e)
        } *)
