namespace WhereInTheWorld.Data

open System.IO
open WhereInTheWorld.Utilities.Models
open FSharp.Data.Sql
open Hopac
open System
open Microsoft.Data.Sqlite
open WhereInTheWorld.Utilities

type private Sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.MicrosoftDataSqlite,
                    ResolutionPath = "temp",
                    ConnectionString = "Filename=./../world.db",
                    UseOptionTypes = true>

module Database =
    let runtimeConnectionString = sprintf "Filename=%s" databaseFile

    let clearDatabase () =
        if File.Exists(databaseFile)
        then File.Delete(databaseFile)

    let ensureDatabase () =
        if not (File.Exists(databaseFile))
        then
            let connection = new SqliteConnection(runtimeConnectionString)
            connection.Open()
            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.createTables.sql"
            let command = new SqliteCommand(sql, connection)
            command.ExecuteNonQuery() |> ignore
            connection.Close()

module Query =
    let private ctx = Sql.GetDataContext(Database.runtimeConnectionString)

    let getAvailableCountries () =
            let results =
                query {
                    for country in ctx.Main.Country do
                        select (country.Code, country.Name)
                }

            match results |> Seq.isEmpty with
            | true -> None
            | false -> Some (results |> Map.ofSeq)

    let getPostalCodeInformation (postalCodeInput: string) =
        let sanitizedInput = postalCodeInput.Replace(" ", String.Empty).ToUpper()

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


module DataAccess =
    let private ctx = Sql.GetDataContext(Database.runtimeConnectionString)

    let insertCountry (country: Country): Job<int64> =
        job {
            let insertedCountry =
                ctx.Main.Country.``Create(Code, Name)``
                                    (country.Code,
                                        country.Name)

            do! ctx.SubmitUpdatesAsync()
            return insertedCountry.Id
        }

    let insertSubdivisions (subdivisions: Subdivision list) =
        job {
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

            do! ctx.SubmitUpdatesAsync()
            return insertedSubdivisions
        }

    let insertPostalCodes (postalCodes: PostalCode list) =
        job {
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

            do! ctx.SubmitUpdatesAsync()
            return insertedPostalCodes
        }
