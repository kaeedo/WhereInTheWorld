namespace WhereInTheWorld.Utilities.Tests

open Xunit
open Hopac
open Swensen.Unquote
open WhereInTheWorld.Utilities.ResultUtilities
open System
open Xunit.Sdk

module ResultUtilitiesTests =
    [<Fact>]
    let ``Result utilities should work`` () =
        let OkResult: Result<string, int> = Result.Ok "testValue"
        let ErrorResult: Result<int, string> = Result.Error "Some problem"

        test <@ OkResult.IsOk @>
        test <@ ErrorResult.IsError @>

        test <@ OkResult |> isOkResult @>
        test <@ ErrorResult |> isErrorResult @>

    [<Fact>]
    let ``Result value should throw on error`` () =
        let errorResult: Result<int, string> = Result.Error "Some problem"

        raisesWith<ArgumentException> <@ errorResult.OkValue @>

    [<Fact>]
    let ``Result value should throw on OK`` () =
        let okResult: Result<string, int> = Result.Ok "Good!"

        raisesWith<ArgumentException> <@ okResult.ErrorValue @>

    [<Fact>]
    let ``Two succesful switches should bind and return ok`` () =
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
    let ``A failed switch should bind and return error`` () =
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