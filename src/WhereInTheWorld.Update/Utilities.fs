namespace WhereInTheWorld.Update

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
