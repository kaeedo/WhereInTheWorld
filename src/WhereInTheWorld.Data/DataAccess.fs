namespace WhereInTheWorld.Data

open System
open System.IO
open System.Globalization
open Microsoft.Data.Sqlite

open Dapper
open Hopac
open Hopac.Infixes

open WhereInTheWorld.Utilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities

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

    let upper: Func<string, string> =
         Func<string, string>(fun (args: string) ->
                        args.ToUpper(CultureInfo.InvariantCulture))

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
        job {
            let sanitizedInput = cityName.Replace(" ", String.Empty).ToUpper()

            let query (input: string) =
                let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.queryCityName.sql"

                let connection = Database.safeSqlConnection Database.connectionString
                connection.Open()
            
                connection.CreateFunction<string, string>("UPPER", Database.upper)

                job {
                    let! results = 
                        connection.QueryAsync<PostalCodeInformation>(sql, dict ["Input", box input])
                        |> Job.awaitTask
                    connection.Close()

                    return results |> List.ofSeq
                } |> run

            try
                return Result.Ok (query sanitizedInput)
            with
            | _ as e -> return Result.Error e
        }

    let getPostalCodeInformation (postalCodeInput: string) =
        job {
            let sanitizedInput = postalCodeInput.Replace(" ", String.Empty).ToUpper()

            let query (input: string) =
                let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.queryPostalCode.sql"

                let connection = Database.safeSqlConnection Database.connectionString
                connection.Open()
                job {
                    let! results = 
                        connection.QueryAsync<PostalCodeInformation>(sql, dict ["Input", box input])
                        |> Job.awaitTask
                    connection.Close()

                    return results |> List.ofSeq
                }

            let rec queryUntilMatch (input: string) =
                match input with
                | _ when input.Length <= 3 -> query input
                | _ ->
                    let results =
                        job {
                            return! query input
                        } |> run

                    if results |> List.isEmpty
                    then
                        let newInput = input.Substring(0, int (Math.Ceiling(float input.Length / 2.0)))
                        queryUntilMatch newInput
                    else job { return results }

            try
                let! result = (queryUntilMatch sanitizedInput)
                return Result.Ok result
            with
            | _ as e -> return Result.Error e
        }

    let getSearchResult input =
        let postalCodeJob = getPostalCodeInformation input
        let cityNameJob = getCityNameInformation input

        let postalResult, cityResult = (postalCodeJob <*> cityNameJob) |> run
        if cityResult.IsOk && cityResult.OkValue.Length > 0 then
            cityResult
        else 
            postalResult


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
