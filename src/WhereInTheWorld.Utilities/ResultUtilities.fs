namespace WhereInTheWorld.Utilities

open Hopac

module ResultUtilities =
    type Result<'a, 'b> with
        member result.IsOk =
            match result with
            | Ok _ -> true
            | Error _ -> false

        member result.IsError =
            match result with
            | Ok _ -> false
            | Error _ -> true

        member result.OkValue =
            match result with
            | Ok value -> value
            | Error _ -> invalidArg "Value" "Result is not Ok"

        member result.ErrorValue =
            match result with
            | Ok _ -> invalidArg "Value" "Result is not an Error"
            | Error value -> value

    let bind (fn: 'a -> Job<Result<'b, exn>>) (jobValue: Job<Result<'a, exn>>) = 
        job {
            try
                let! result = jobValue
                match result with
                | Error e -> return Result.Error e
                | Ok q -> return! fn q
            with
            | ex -> return Result.Error ex
        }

    let compose (first : 'a -> Job<Result<'b, exn>>) (second : 'b -> Job<Result<'c, exn>>) : 'a -> Job<Result<'c, exn>> =
        fun x ->
            bind second (first x)

    let (>>=) twoTrackInput switchFunction = bind switchFunction twoTrackInput
    let (>=>) firstSwitch secondSwitch = compose firstSwitch secondSwitch

    let isOkResult (result: Result<_,_>) = result.IsOk

    let isErrorResult (result: Result<_,_>) = result.IsError
