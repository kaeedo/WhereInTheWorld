namespace WhereInTheWorld.Data.Tests

open System.IO
open Xunit
open Swensen.Unquote
open Hopac
open WhereInTheWorld.Data
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities
open System
open Foq
open Xunit.Abstractions

type IWork =
    abstract member DoWork: 'a -> unit

type DataDownloadTests() =
    let verifier (printer: IWork) message =
        job {
            let! status = Ch.take message

            match status with
            | _ ->
                printer.DoWork("")
        }

    interface IDisposable with
        member __.Dispose() = 
            if Directory.Exists(baseDirectory)
            then Directory.Delete(baseDirectory, true)

    [<Fact>]
    member __.``When given valid country code should save txt file`` () =
        let channel = Ch<DownloadStatus>()
        let printer = Mock<IWork>().Create()

        let workflowResult =
            job {
                let insertStatusPrinterChannel = verifier printer channel
                do! Job.foreverServer insertStatusPrinterChannel
                
                return! DataDownload.downloadPostalCodesForCountry channel "AD"
            } |> run

        test <@ workflowResult.IsOk @>

    [<Fact>]
    member __.``When given invalid country code should not save txt file`` () =
        let channel = Ch<DownloadStatus>()
        let printer = Mock<IWork>().Create()

        let workflowResult =
            job {
                let insertStatusPrinterChannel = verifier printer channel
                do! Job.foreverServer insertStatusPrinterChannel
                
                return! DataDownload.downloadPostalCodesForCountry channel "ADb"
            } |> run

        test <@ workflowResult.IsError @>

    [<Fact>]
    member __.``When given valid country code should send message to channel`` () =
        let channel = Ch<DownloadStatus>()
        let printer = Mock<IWork>().Create()

        job {
            let insertStatusPrinterChannel = verifier printer channel
            do! Job.foreverServer insertStatusPrinterChannel
                
            return! DataDownload.downloadPostalCodesForCountry channel "AD"
        }
        |> run
        |> ignore

        Mock.Verify(<@ printer.DoWork(any()) @>, Times.exactly(2))
