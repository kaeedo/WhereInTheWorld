open Fake.DotNet

#r "paket:
nuget FSharp.Core 4.5.2
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.MSBuild
nuget Fake.Core.Target
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Xml //"
#load "./.fake/build.fsx/intellisense.fsx"

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

let entryProject = "./src/WhereInTheWorld/WhereInTheWorld.fsproj"

Target.create "Clean" (fun _ ->
    DotNet.exec id "clean" ""
    |> ignore

    !!("**/*.fsproj")
    |> Seq.iter (fun project ->
        let projectDirectory = Path.GetDirectoryName(project)
        let binDirectory = projectDirectory @@ "bin"
        let objDirectory = projectDirectory @@ "obj"

        if Directory.Exists(binDirectory)
        then Directory.Delete(binDirectory, true)

        if Directory.Exists(objDirectory)
        then Directory.Delete(objDirectory, true)
    )

    if Directory.Exists("output")
    then Directory.Delete("output", true)
)

Target.create "SetVersion" (fun _ ->
    let release = ReleaseNotes.load "RELEASE_NOTES.md"
    let releaseVersion = release.SemVer.ToString()
    Xml.pokeInnerText entryProject "/Project/PropertyGroup[1]/Version[1]" releaseVersion
    Xml.saveDoc entryProject (Xml.loadDoc entryProject)
)

Target.create "Build" (fun _ ->
    DotNet.build (fun buildOptions ->
        { buildOptions with
            Configuration = DotNet.BuildConfiguration.Release }
    ) ""
)

Target.create "Test" (fun _ ->
    let setDotNetOptions projectDirectory =
        fun (dotNetTestOptions: DotNet.TestOptions) ->
            { dotNetTestOptions with
                Common = { dotNetTestOptions.Common with WorkingDirectory = projectDirectory} }

    !!("test/**/*.Tests.fsproj")
    |> Seq.iter (
        fun projectName ->
            let projectDirectory = Path.GetDirectoryName(projectName)
            DotNet.test (setDotNetOptions projectDirectory) ""
    )
)

Target.create "Publish" (fun _ ->
    let setPublishParams (defaultPublishParams: DotNet.PublishOptions) =
        { defaultPublishParams with
            Configuration = DotNet.BuildConfiguration.Release }

    DotNet.publish setPublishParams entryProject
)

Target.create "Pack" (fun _ ->
    DotNet.pack (fun packOptions ->
        { packOptions with
            Configuration = DotNet.BuildConfiguration.Release
            NoBuild = true
            OutputPath = Some "../../output" }
    ) entryProject
)

Target.createFinal "Done" (fun _ ->
  Trace.log " --- Fake script is done --- "
)

"Clean"
    ==> "SetVersion"
    ==> "Build"
    ==> "Test"
    ==> "Publish"
    ==> "Pack"
    ==> "Done"

Target.runOrDefault "Done"
