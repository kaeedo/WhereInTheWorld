namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper
open Hopac
open System.IO

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
    type Result<'a, 'b> with
        member result.IsOk =
            match result with
            | Ok _ -> true
            | Error _ -> false

        member result.IsError =
            match result with
            | Ok _ -> false
            | Error _ -> true

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

    let (@@) a b = Path.Combine(a, b)

    let isOkResult (result: Result<_,_>) = result.IsOk

    let isErrorResult (result: Result<_,_>) = result.IsError

    let safeSqlConnection (connectionString: string) =
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        new SQLiteConnection(connectionString)
