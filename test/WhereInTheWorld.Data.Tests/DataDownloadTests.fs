namespace WhereInTheWorld.Data.Tests

open System.IO
open Xunit
open Swensen.Unquote
open Hopac
open WhereInTheWorld.Data
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities
open System

type DataDownloadTests() =
    interface IDisposable with
        member __.Dispose() = ()
            //File.Delete("")

    [<Fact>]
    member __.``When given valid country code should save txt file`` () =
        let workflowResult =
            job {
                do! Job.foreverServer <| Job.lift id ()
                
                return! DataDownload.downloadPostalCodesForCountry (Ch<DownloadStatus>()) "AD"
            } |> run

        test <@ workflowResult.IsOk @>