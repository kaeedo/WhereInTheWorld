namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper

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

module Utilities =
    let (>>=) twoTrackInput switchFunction =
        Result.bind switchFunction twoTrackInput

    let (>=>) firstSwitch secondSwitch =
        match firstSwitch with
        | Result.Ok output -> secondSwitch output
        | Result.Error error -> Result.Error error

    let switch fn value =
        fn value |> Result.Ok

    let map oneTrack twoTrack =
        match twoTrack with
        | Ok x -> Ok (oneTrack x)
        | Error e -> Error e

    let safeSqlConnection (connectionString: string) =
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        new SQLiteConnection(connectionString)
