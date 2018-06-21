namespace WhereInTheWorld.Update

open Models
open Utilities
open System
open System.IO
open FSharp.Data.Sql

type private sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite,
                    ConnectionString = "Data Source=./world.db;Version=3;",
                    UseOptionTypes = true>

module DataAccess =
    let private ctx = sql.GetDataContext(sprintf "Data Source=%s;Version=3" Models.databaseFile)

    let insertCountry (country: CountryDao): int64 =
        let insertedCountry =
            ctx.Main.Country.``Create(Code, LocalizedName, Name)``
                                (country.Code,
                                 country.LocalizedName,
                                 country.Name)

        ctx.SubmitUpdates()
        insertedCountry.Id

    let upsertCountry (country: CountryDao) =
        let foundCountryMaybe =
            query {
                for c in ctx.Main.Country do
                where (c.Code = country.Code)
                select (Some c)
                exactlyOneOrDefault
            }

        match foundCountryMaybe with
        | None -> insertCountry country
        | Some foundCountry ->
            foundCountry.Name <- country.Name
            foundCountry.LocalizedName <- country.LocalizedName
            ctx.SubmitUpdates()
            foundCountry.Id

    let insertSubdivisions (subdivisions: SubdivisionDao list) =
        let insertedSubdivisions =
            subdivisions
            |> List.map (fun s ->
                let insertedSubdvision =
                    ctx.Main.Subdivision.``Create(Code, CountryId, Name)``
                                            (s.Code,
                                             s.CountryId,
                                             s.Name)

                insertedSubdvision
            )

        ctx.SubmitUpdates()
        insertedSubdivisions

    let insertPostalCodes (postalCodes: PostalCodeDao list)  =
        let insertedPostalCodes =
            postalCodes
            |> List.map (fun pc ->
                let insertedPostalCode =
                    ctx.Main.PostalCode.``Create(PlaceName, PostalCode, SubdivisionId)``
                                            (pc.PlaceName,
                                             pc.PostalCode,
                                             pc.SubdivisionId)
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
