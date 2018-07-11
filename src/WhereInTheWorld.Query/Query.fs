namespace WhereInTheWorld.Query

open WhereInTheWorld.Utilities.Models
open FSharp.Data.Sql
open FSharp.Data.Sql.Common

type private Sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite,
                    ConnectionString = "Data Source=./world.db;Version=3;",
                    UseOptionTypes = true>

module Query =
    let private ctx = Sql.GetDataContext(sprintf "Data Source=%s;Version=3" databaseFile)

    let getInformation postalCodeInput =
        let result =
            query {
                for postalCode in ctx.Main.PostalCode do
                    for subdivision in postalCode.``main.Subdivision by Id`` do
                        for country in subdivision.``main.Country by Id`` do
                            where (postalCode.PostalCode = postalCodeInput)
                            select (postalCode, subdivision, country)
            }
            |> Seq.map (fun (postalCode, subdivision, country) ->
                let country =
                    { Code = country.Code; Name = country.Name }

                let subdivision =
                    { Country = country; Code = subdivision.Code; Name = subdivision.Name }

                let postalCode =
                    { Subdivision = subdivision
                      PostalCode = postalCode.PostalCode
                      PlaceName = postalCode.PlaceName
                      CountyName = postalCode.CountyName
                      CountyCode = postalCode.CountyCode
                      CommunityName = postalCode.CommunityName
                      CommunityCode = postalCode.CommunityCode
                      Latitude = postalCode.Latitude
                      Longitude = postalCode.Longitude
                      Accuracy = postalCode.Accuracy }

                postalCode
            )

        result
