namespace WhereInTheWorld

open System
open System.IO
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.IoUtilities

module ErrorLog =
    let private logFile = baseDirectory @@ sprintf "error_%s.log" (DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss"))

    let writeException (ex: exn) =
        use streamWriter = File.CreateText(logFile)
        streamWriter.WriteLine(ex.Message)
        streamWriter.Write(ex.StackTrace)
