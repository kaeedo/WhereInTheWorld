namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper

[<CLIMutable>]
type Test =
    { Id: int
      Name: string }


module Update =
    let private databaseFilename = ".\world.db"
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = new SQLiteConnection(connectionString)

    connection.Open()

    let get =
        connection.Query<Test>("SELECT * FROM Test")
        |> Seq.tryHead
