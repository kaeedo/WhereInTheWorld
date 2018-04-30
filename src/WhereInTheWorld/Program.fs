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

        let countryId =
            DataAccess.insertCountry { Id = 1; Code = countryCode; Name = countryName; LocalizedName = countryLocalizedName }

        printfn "%i" countryId

        import
        |> Seq.map (fun i ->
            { Id = 1; Code = i.SubdivisionCode; Name = i.SubdivisionName; }
        )
        |> Seq.iter DataAccess.insertSubdivision

        import
        |> Seq.map (fun i ->
            { Id = 1
              PostalCode = i.PostalCode
              PlaceName = i.PlaceName
              CountryId = 1
              SubdivisionId = 1
              CountyName = i.CountyName
              CountyCode = i.CountyCode
              CommunityName = i.CommunityName
              CommunityCode = i.CommunityCode
              Latitude = i.Latitude
              Longitude = i.Longitude
              Accuracy = i.Accuracy }
        )
        |> ignore
        // |> Seq.iter DataAccess


    System.Console.ReadLine() |> ignore
    0
