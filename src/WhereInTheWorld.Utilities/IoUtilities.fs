namespace WhereInTheWorld.Utilities

open System
open System.IO
open System.Reflection
open System.Text

module IoUtilities =
    let (@@) a b = Path.Combine(a, b)

    let getEmbeddedResource resourceName =
        let assembly = Assembly.GetCallingAssembly()
        let resourceStream = assembly.GetManifestResourceStream(resourceName)
        use reader = new StreamReader(resourceStream, Encoding.UTF8)
        reader.ReadToEnd()

    let parseTsv (contents: string) =
        contents
        |> fun t -> t.Split([|Environment.NewLine|], StringSplitOptions.None)
        |> Seq.filter (fun pc -> not (String.IsNullOrWhiteSpace(pc)))
        |> Seq.map (fun pc ->
            let line = pc.Split('\t')
            line.[0], line.[1]
        )
