open System
open WhereInTheWorld.Update
open WhereInTheWorld.Update.Models
open Hopac
open System.IO

[<EntryPoint>]
let main argv =
    job {
        if Directory.Exists("./temp")
        then Directory.Delete("./temp", true)
        Directory.CreateDirectory("./temp") |> ignore

        let! filePath = DataDownload.downloadPostalCodesForCountry "DE"
        match filePath with
        | Error e -> printfn "Something went wrong when downloading for country code %s: %A" "DE" e
        | Ok fp ->
            let! import = DataImport.fileImport fp

            match import with
            | Error e -> printfn "Something went from importing from file: %A" e
            | Ok imp ->
                let countryCode, countryName, countryLocalizedName =
                    DataDownload.supportedCountries
                    |> Seq.find (fun sc ->
                        let code, _, _ = sc
                        code = "DE"
                    )

                let uniqueSubdivisions =
                    imp
                    |> Seq.map (fun i ->
                        { Id = 1; CountryId = 1; Code = i.SubdivisionCode; Name = i.SubdivisionName; }
                    )
                    |> Seq.distinctBy (fun sd -> sd.Code)

                let! countryId =
                    DataAccess.insertCountryGetId { Id = 1; Code = countryCode; Name = countryName; LocalizedName = countryLocalizedName }

                do! DataAccess.insertSubdivisions countryId uniqueSubdivisions

                let! subdivisions =
                    uniqueSubdivisions
                    |> DataAccess.getSubdivisions

                let getSubdivisionId fileImport subdivisions =
                    let subdivision =
                        subdivisions
                        |> Seq.find (fun sd -> sd.Code = fileImport.SubdivisionCode)

                    subdivision.Id

                imp
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
                |> run
    }
    |> run

    0
