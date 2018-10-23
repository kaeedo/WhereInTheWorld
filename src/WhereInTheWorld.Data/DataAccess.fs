namespace WhereInTheWorld.Data

open System
open System.IO
open Microsoft.Data.Sqlite

open Dapper
open Hopac

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
    let connectionString = sprintf "Data Source=%s" databaseFile

    let safeSqlConnection (connectionString: string) =
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        new SqliteConnection(connectionString)

    let clearDatabase () =
        if File.Exists(databaseFile)
        then File.Delete(databaseFile)

    let ensureDatabase () =
        if not (File.Exists(databaseFile))
        then
            let file = new FileInfo(databaseFile)
            file.Directory.Create()
            let connection = safeSqlConnection connectionString
            connection.Open()
            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.createTables.sql"
            connection.Execute(sql) |> ignore
            connection.Close()

module Query =
    let getAvailableCountries () =
        let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.queryCountry.sql"

        let connection = Database.safeSqlConnection Database.connectionString

        try
            connection.Open()
            let results =
                connection.Query<Country>(sql)
                |> Seq.map (fun c -> c.Code, c.Name)
                |> Map.ofSeq
                |> fun m -> if m |> Map.isEmpty then None else Some m
            connection.Close()
            Result.Ok results
        with
        | _ as e -> Result.Error e

    let getCityNameInformation (cityName: string) =
        let sanitizedInput = cityName.Replace(" ", String.Empty).ToUpper()

        let query (input: string) =
            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.queryCityName.sql"

            let connection = Database.safeSqlConnection Database.connectionString
            connection.Open()
            let results = connection.Query<PostalCodeInformation>(sql, dict ["Input", box input])
            connection.Close()

            results
            |> List.ofSeq

        try
            Result.Ok (query sanitizedInput)
        with
        | _ as e -> Result.Error e

    let getPostalCodeInformation (postalCodeInput: string) =
        let sanitizedInput = postalCodeInput.Replace(" ", String.Empty).ToUpper()

        let query (input: string) =
            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.queryPostalCode.sql"

            let connection = Database.safeSqlConnection Database.connectionString
            connection.Open()
            let results = connection.Query<PostalCodeInformation>(sql, dict ["Input", box input])
            connection.Close()

            results
            |> List.ofSeq

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

        try
            Result.Ok (queryUntilMatch sanitizedInput)
        with
        | _ as e -> Result.Error e


module DataAccess =
    let insertPostalCodes (postalCodes: PostalCodeInformation list) =
        job {
            let connection = Database.safeSqlConnection Database.connectionString
            connection.Open()
            let transaction = connection.BeginTransaction()

            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.insertPostalCodes.sql"
            let! resultSet = connection.ExecuteAsync(sql, postalCodes, transaction)

            transaction.Commit()
            connection.Close()

            return resultSet
        }
