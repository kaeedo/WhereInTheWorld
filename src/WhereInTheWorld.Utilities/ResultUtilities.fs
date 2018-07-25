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

    let bind (fn: 'a -> Job<Result<'b, 'c>>) (jobValue: Job<Result<'a, 'c>>) =
        Job.tryWith (job {
            let! r = jobValue
            match r with
            | Ok value ->
                let next = fn value
                return! next
            | Error err -> return (Error err)
        }) (Job.lift Error)

    let compose oneTrack twoTrack value =
        bind twoTrack (oneTrack value)

    let (>>=) twoTrackInput switchFunction = bind switchFunction twoTrackInput
    let (>=>) firstSwitch secondSwitch = compose firstSwitch secondSwitch

    let isOkResult (result: Result<_,_>) = result.IsOk

    let isErrorResult (result: Result<_,_>) = result.IsError
