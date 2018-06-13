namespace WhereInTheWorld.Update

open Dapper
open Models
open Hopac
open System.IO
open System.Data.SQLite
open System.Reflection
open System.Text

module DataAccess =
    type Codes = { Codes: seq<string> }
    let private databaseFilename = sprintf "%s/world.db" baseDirectory
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = Utilities.safeSqlConnection connectionString

    let private getSqlScript scriptName =
        let assembly = Assembly.GetExecutingAssembly()
        let resourceStream = assembly.GetManifestResourceStream(scriptName)
        use reader = new StreamReader(resourceStream, Encoding.UTF8)
        reader.ReadToEnd()

    let openConnection () = connection.Open()
    let closeConnection () = connection.Close()

    let ensureDatabase () =
        if not (File.Exists(databaseFilename))
        then SQLiteConnection.CreateFile(databaseFilename)

        let sql = getSqlScript "WhereInTheWorld.Update.sqlScripts.createTables.sql"
        connection.Execute(sql) |> ignore

    let insertPostalCodes (postalCodes: seq<PostalCodeInformation>) =
        job {
            let transaction = connection.BeginTransaction()

            let sql = getSqlScript "WhereInTheWorld.Update.sqlScripts.insertPostalCode.sql"

            try
                let! _ = connection.ExecuteAsync(sql, postalCodes, transaction)
                transaction.Commit()
            with
            | Failure (message) -> failwith message
        }
