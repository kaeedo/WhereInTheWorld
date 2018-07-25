namespace WhereInTheWorld.Query

open WhereInTheWorld.Utilities.Models
open System
open FSharp.Data.Sql
open FSharp.Data.Sql.Common

type private Sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite,
                    ConnectionString = "Data Source=./world.db;Version=3;",
                    UseOptionTypes = true>


module Query =
    let private ctx = Sql.GetDataContext(sprintf "Data Source=%s;Version=3" databaseFile)

    let getInformation (postalCodeInput: string) =
        let sanitizedInput = postalCodeInput.Replace(" ", "").ToUpper()

        let query (input: string) =
            query {
                for postalCode in ctx.Main.PostalCode do
                    for subdivision in postalCode.``main.Subdivision by Id`` do
                        for country in subdivision.``main.Country by Id`` do
                            where (postalCode.PostalCode.Replace(" ", "").ToUpper().StartsWith(input))
                            select (postalCode, subdivision, country)
            } |> Seq.toList

        let rec queryUntilMatch (input: string) =
            match input with
            | _ when input.Length = 3 -> query input
            | _ ->
                let results = query input

                if results |> List.isEmpty
                then
                    let newInput = input.Substring(0, int (Math.Ceiling(float input.Length / 2.0)))
                    queryUntilMatch newInput
                else results

        let result =
            queryUntilMatch sanitizedInput
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
