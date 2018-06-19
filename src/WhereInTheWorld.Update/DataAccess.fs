namespace WhereInTheWorld.Update

open Dapper
open Models
open Hopac
open System.IO
open System.Data.SQLite
open System.Reflection
open System.Text

type DataAccess() =
    let databaseFilename = sprintf "%s/world.db" baseDirectory
    let connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let getSqlScript scriptName =
        let assembly = Assembly.GetExecutingAssembly()
        let resourceStream = assembly.GetManifestResourceStream(scriptName)
        use reader = new StreamReader(resourceStream, Encoding.UTF8)
        reader.ReadToEnd()

    let connection = Utilities.safeSqlConnection connectionString

    do connection.Open()

    member this.CloseConnection () = connection.Close()

    member this.EnsureDatabase () =
        if not (File.Exists(databaseFilename))
        then SQLiteConnection.CreateFile(databaseFilename)

        let sql = getSqlScript "WhereInTheWorld.Update.sqlScripts.createTables.sql"
        connection.Execute(sql) |> ignore

    member this.InsertPostalCodes postalCodes =
        let transaction = connection.BeginTransaction()

        let sql = getSqlScript "WhereInTheWorld.Update.sqlScripts.insertPostalCode.sql"

        try
            connection.Execute(sql, postalCodes, transaction) |> ignore

            transaction.Commit()
        with
        | Failure (message) ->
            failwith message
