namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper
open Hopac

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
    let bind (fn: 'a -> Job<Result<'b, 'c>>) (jobValue: Job<Result<'a, 'c>>) =
        job {
            let! r = jobValue
            match r with
            | Ok value ->
                let next = fn value
                return! next
            | Error err -> return (Error err)
        }

    let compose oneTrack twoTrack value =
        bind twoTrack (oneTrack value)

    let (>>=) twoTrackInput switchFunction = bind switchFunction twoTrackInput
    let (>=>) firstSwitch secondSwitch = compose firstSwitch secondSwitch

    let safeSqlConnection (connectionString: string) =
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        new SQLiteConnection(connectionString)
