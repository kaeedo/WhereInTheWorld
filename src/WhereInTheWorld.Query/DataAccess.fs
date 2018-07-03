namespace WhereInTheWorld.Query

open WhereInTheWorld.Utilities.Models
open FSharp.Data.Sql

type private Sql = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite,
                    ConnectionString = "Data Source=../world.db;Version=3;",
                    UseOptionTypes = true>

module DataAccess =
    let private ctx = Sql.GetDataContext(sprintf "Data Source=%s;Version=3" databaseFile)
