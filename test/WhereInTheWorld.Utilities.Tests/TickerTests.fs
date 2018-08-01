namespace WhereInTheWorld.Utilities.Tests

open Xunit
open Hopac
open WhereInTheWorld.Utilities.Models
open Foq

type IWork =
    abstract member DoWork: 'a -> unit

type TickerTests() =
    let verifier (printer: IWork) message =
        job {
            let! status = Ch.take message

            match status with
            | _ ->
                printer.DoWork("")
        }
    
    [<Fact>]
    member __.``New ticker should not have loop started`` () =
        let printer = Mock<IWork>().Create()
        job {
            let ticker = WhereInTheWorld.Utilities.Ticker(50)

            let insertStatusPrinterChannel = verifier printer ticker.Channel
            do! Job.foreverServer insertStatusPrinterChannel

            Mock.Verify(<@ printer.DoWork(any()) @>, never)
        } |> run

    [<Fact>]
    member __.``Ticker should tick twice`` () =
        let printer = Mock<IWork>().Create()
        job {
            let ticker = WhereInTheWorld.Utilities.Ticker(50)

            let insertStatusPrinterChannel = verifier printer ticker.Channel
            do! Job.foreverServer insertStatusPrinterChannel

            ticker.Start()

            do! timeOutMillis 140

            ticker.Stop()

            Mock.Verify(<@ printer.DoWork(any()) @>, Times.AtLeast(2))
        } |> run

    [<Fact>]
    member __.``Ticker should tick twice then stop`` () =
        let printer = Mock<IWork>().Create()
        job {
            let ticker = WhereInTheWorld.Utilities.Ticker(50)

            let insertStatusPrinterChannel = verifier printer ticker.Channel
            do! Job.foreverServer insertStatusPrinterChannel

            ticker.Start()

            do! timeOutMillis 140

            ticker.Stop()

            do! timeOutMillis 200

            Mock.Verify(<@ printer.DoWork(any()) @>, Times.exactly(2))
        } |> run
