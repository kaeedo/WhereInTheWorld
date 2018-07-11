namespace WhereInTheWorld.Update

open WhereInTheWorld.Utilities.Models
open FSharp.Data.Sql

type private Sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite,
                    ConnectionString = "Data Source=./world.db;Version=3;",
                    UseOptionTypes = true>

module DataAccess =
    let private ctx = Sql.GetDataContext(sprintf "Data Source=%s;Version=3" databaseFile)

    let insertCountry (country: Country): int64 =
        let insertedCountry =
            ctx.Main.Country.``Create(Code, Name)``
                                (country.Code,
                                 country.Name)

        ctx.SubmitUpdates()
        insertedCountry.Id

    let insertSubdivisions (subdivisions: Subdivision list) =
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

    let insertPostalCodes (postalCodes: PostalCode list)  =
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
