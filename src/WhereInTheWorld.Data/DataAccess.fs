namespace WhereInTheWorld.Data

open Dapper
open System
open System.IO
open Hopac
open System.Data.SQLite
open WhereInTheWorld.Utilities
open WhereInTheWorld.Utilities.Models

type OptionHandler<'T>() =
    inherit SqlMapper.TypeHandler<option<'T>>()
    override __.SetValue(param, value) =
        let valueOrNull =
            match value with
            | Some x -> box x
            | None -> null
        param.Value <- valueOrNull
    override __.Parse value =
        if isNull value || value = box DBNull.Value
        then None
        else Some (value :?> 'T)

module Database =
    let connectionString = sprintf "Data Source=%s;Version=3" databaseFile

    let safeSqlConnection (connectionString: string) =
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        new SQLiteConnection(connectionString)

module Query =
    type foo = {Input: string}
    let getAvailableCountries () =
        ["DE", "Germany"]
        |> Map.ofSeq
        |> Some

    let getPostalCodeInformation (postalCodeInput: string) =
        let sanitizedInput = postalCodeInput.Replace(" ", String.Empty).ToUpper()

        let query (input: string) =
            let sql = """
                SELECT
                    c.Code AS 'CountryCode',
                    c.Name AS 'CountryName',
                    pc.PostalCode AS 'PostalCode',
                    pc.PlaceName AS 'PlaceName',
                    s.Code AS 'SubdivisionCode',
                    s.Name AS 'SubdivisionName',
                    pc.CountyName AS 'CountyName',
                    pc.CountyCode AS 'CountyCode',
                    pc.CommunityName AS 'CommunityName',
                    pc.CommunityCode AS 'CommunityCode',
                    pc.Latitude AS 'Latitude',
                    pc.Longitude AS 'Longitude',
                    pc.Accuracy AS 'Accuracy'
                FROM PostalCode pc
                JOIN Subdivision s on pc.SubdivisionId = s.Id
                JOIN Country c on s.CountryId = c.Id
                WHERE UPPER(REPLACE(pc.PostalCode, ' ', '')) like @input || '%';
                """

            let connection = Database.safeSqlConnection Database.connectionString
            connection.Open()
            connection.Query<PostalCodeInformation>(sql, { Input = input})

        query sanitizedInput



module DataAccess =
    let clearDatabase () =
        if File.Exists(databaseFile)
        then File.Delete(databaseFile)

    let ensureDatabase () =
        if not (File.Exists(databaseFile))
        then
            SQLiteConnection.CreateFile(databaseFile)
            let connection = Database.safeSqlConnection Database.connectionString
            connection.Open()
            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.createTables.sql"
            connection.Execute(sql) |> ignore
            connection.Close()

    let insertPostalCodes (postalCodes: PostalCodeInformation list) =
        job {
            let connection = Database.safeSqlConnection Database.connectionString
            connection.Open()
            let transaction = connection.BeginTransaction()

            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.insertPostalCodes.sql"
            connection.ExecuteAsync(sql, postalCodes, transaction) |> ignore

            transaction.Commit()
            connection.Close()
        }
