open System
open WhereInTheWorld.Update
open WhereInTheWorld.Update.Models

let mapCountries =
    Seq.map (fun i ->
        let countryCode, countryName, countryLocalizedName =
            DataImport.supportedCountries
            |> Seq.find (fun sc ->
                let code, _, _ = sc
                code = i.CountryCode
            )
        { Id = 1; Code = countryCode; Name = countryName; LocalizedName = countryLocalizedName }
    )

[<EntryPoint>]
let main argv =
    let germanImport = DataImport.fileImport "DE"

    match germanImport with
    | Error e -> printfn "Something went from importing from file: %A" e
    | Ok import ->
        let countries = mapCountries import

        let countryCode, countryName, countryLocalizedName =
            DataImport.supportedCountries
            |> Seq.find (fun sc ->
                let code, _, _ = sc
                code = "DE"
            )

        let uniqueSubdivisions =
            import
            |> Seq.map (fun i ->
                { Id = 1; CountryId = 1; Code = i.SubdivisionCode; Name = i.SubdivisionName; }
            )
            |> Seq.distinctBy (fun sd -> sd.Code)

        let countryId =
            DataAccess.insertCountryGetId { Id = 1; Code = countryCode; Name = countryName; LocalizedName = countryLocalizedName }

        DataAccess.insertSubdivisions countryId uniqueSubdivisions

        let subdivisions =
            uniqueSubdivisions
            |> DataAccess.getSubdivisions

        let getSubdivisionId fileImport subdivisions =
            let subdivision =
                subdivisions
                |> Seq.find (fun sd -> sd.Code = fileImport.SubdivisionCode)

            subdivision.Id


        import
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

    0
