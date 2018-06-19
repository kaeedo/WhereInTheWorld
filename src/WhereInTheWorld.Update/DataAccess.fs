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
        let insertedCountry =
            ctx.Main.Country.``Create(Code, LocalizedName, Name)``
                (country.Code, country.Name, country.LocalizedName)

        ctx.SubmitUpdates()
        insertedCountry.Id

    let insertSubdivisions (subdivisions: seq<SubdivisionDao>) countryId =
        let insertedSubdivisions =
            subdivisions
            |> Seq.map (fun s ->
                let insertedSubdvision = ctx.Main.Subdivision.Create()
                insertedSubdvision.CountryId <- countryId
                insertedSubdvision.Code <- s.Code
                insertedSubdvision.Name <- s.Name

                insertedSubdvision
            )

        ctx.SubmitUpdates()
        insertedSubdivisions


    // let insertPostalCodes (postalCodes: seq<PostalCodeDao>) subdivisionId =
    //     postalCodes
    //     |> Seq.map (fun pc ->
    //         let insertedPostalCode = ctx.Main.PostalCode.Create()
    //         insertedPostalCode.SubdivisionId <- subdivisionId
    //         insertedPostalCode.PostalCode <- pc.PostalCode
    //         insertedPostalCode.PlaceName <- pc.PlaceName
    //         insertedPostalCode.CountyName <- pc.CountyName
    //         insertedPostalCode.CountyCode <- pc.CountyCode
    //         insertedPostalCode.CommunityName <- pc.CommunityName
    //         insertedPostalCode.CommunityCode <- pc.CommunityCode
    //         insertedPostalCode.Latitude <- pc.Latitude
    //         insertedPostalCode.Longitude <- pc.Longitude
    //         insertedPostalCode.Accuracy <- pc.Accuracy
    //     )
