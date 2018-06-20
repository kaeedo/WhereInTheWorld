namespace WhereInTheWorld.Update

open Models
open System
open FSharp.Data.Sql

type private sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite,
                    ConnectionString = "Data Source=./seed.db;Version=3;",
                    UseOptionTypes = true>

module DataAccess =
    let private ctx = sql.GetDataContext()

    let insertCountry (country: CountryDao): int64 =
        let insertedCountry = ctx.Main.Country.Create()
        insertedCountry.Code <- country.Code
        insertedCountry.LocalizedName <- country.LocalizedName
        insertedCountry.Name <- country.Name

        ctx.SubmitUpdates()
        insertedCountry.Id

    let insertSubdivisions (subdivisions: SubdivisionDao list) =
        let insertedSubdivisions =
            subdivisions
            |> List.map (fun s ->
                let insertedSubdvision = ctx.Main.Subdivision.Create()
                insertedSubdvision.CountryId <- s.CountryId
                insertedSubdvision.Code <- s.Code
                insertedSubdvision.Name <- s.Name

                insertedSubdvision
            )

        ctx.SubmitUpdates()
        insertedSubdivisions


    let insertPostalCodes (postalCodes: PostalCodeDao list)  =
        let insertedPostalCodes =
            postalCodes
            |> List.map (fun pc ->
                let insertedPostalCode = ctx.Main.PostalCode.Create()
                insertedPostalCode.SubdivisionId <- pc.SubdivisionId
                insertedPostalCode.PostalCode <- pc.PostalCode
                insertedPostalCode.PlaceName <- pc.PlaceName
                insertedPostalCode.CountyName <- pc.CountyName
                insertedPostalCode.CountyCode <- pc.CountyCode
                insertedPostalCode.CommunityName <- pc.CommunityName
                insertedPostalCode.CommunityCode <- pc.CommunityCode
                insertedPostalCode.Latitude <- pc.Latitude
                insertedPostalCode.Longitude <- pc.Longitude
                insertedPostalCode.Accuracy <- pc.Accuracy

                insertedPostalCode
            )

        ctx.SubmitUpdates()
        insertedPostalCodes
