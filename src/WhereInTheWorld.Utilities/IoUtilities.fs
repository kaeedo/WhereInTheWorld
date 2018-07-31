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