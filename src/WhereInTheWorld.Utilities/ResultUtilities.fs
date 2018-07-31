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

    let bind (fn: 'a -> Job<Result<'b, 'c>>) (jobValue: Job<Result<'a, 'c>>) =
        Job.tryWith (job {
            let! r = jobValue
            match r with
            | Ok value ->
                let next = fn value
                return! next
            | Error err -> return (Error err)
        }) (Job.lift Error)

    let compose firstSwitch secondSwitch value =
        bind secondSwitch (firstSwitch value)

    let (>>=) twoTrackInput switchFunction = bind switchFunction twoTrackInput
    let (>=>) firstSwitch secondSwitch = compose firstSwitch secondSwitch

    let isOkResult (result: Result<_,_>) = result.IsOk

    let isErrorResult (result: Result<_,_>) = result.IsError
