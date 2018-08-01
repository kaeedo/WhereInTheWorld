namespace WhereInTheWorld.Utilities.Tests

open Xunit
open Hopac
open Swensen.Unquote
open WhereInTheWorld.Utilities.ResultUtilities
open System

type ResultUtilitiesTests() =
    [<Fact>]
    member __.``Result utilities should be true for appropriate values`` () =
        let OkResult: Result<string, int> = Result.Ok "testValue"
        let ErrorResult: Result<int, string> = Result.Error "Some problem"

        test <@ OkResult.IsOk @>
        test <@ ErrorResult.IsError @>

        test <@ OkResult |> isOkResult @>
        test <@ ErrorResult |> isErrorResult @>

    [<Fact>]
    member __.``Result value should throw on error`` () =
        let errorResult: Result<int, string> = Result.Error "Some problem"

        raisesWith<ArgumentException> <@ errorResult.OkValue @>

    [<Fact>]
    member __.``Result value should throw on OK`` () =
        let okResult: Result<string, int> = Result.Ok "Good!"

        raisesWith<ArgumentException> <@ okResult.ErrorValue @>

    [<Fact>]
    member __.``Two succesful switches should bind and return ok`` () =
        let inputValue = 1

        let firstOkJob input =
            job {
                return Result.Ok input
            }
        let secondOkJob input =
            job {
                return Result.Ok input
            }

        let workflow = firstOkJob >=> secondOkJob
        let actual = workflow inputValue |> run

        test <@ actual.IsOk @>
        test <@ actual.OkValue = inputValue @>

    [<Fact>]
    member __.``A failed switch should bind and return error`` () =
        let errorMessage = Exception("Failure")
        let inputValue = 1

        let okJob input =
            job {
                return Result.Ok input
            }
        let errorJob _ =
            job {
                return Result.Error errorMessage
            }

        let workflow = okJob >=> errorJob

        let actual = workflow inputValue |> run

        test <@ actual.IsError @>
        test <@ actual.ErrorValue = errorMessage @>