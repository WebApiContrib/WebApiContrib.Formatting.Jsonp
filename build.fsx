#I "packages/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper

type Project = {
    ReleaseNotes: ReleaseNotes
    SolutionFile: String
    TestAssemblies: String
}

let project = {
    ReleaseNotes = LoadReleaseNotes "RELEASE_NOTES.md"
    SolutionFile = "WebApiContrib.Formatting.Jsonp.sln"
    TestAssemblies = "test/**/bin/Release/*Tests*.dll"
}

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

Target "Build" (fun _ ->
    !! project.SolutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "RunTests" (fun _ ->
    !! project.TestAssemblies
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)

Target "PackageNuget" (fun _ ->
    Paket.Pack(fun p -> 
        { p with
            OutputPath = "bin"
            Version = project.ReleaseNotes.NugetVersion
            ReleaseNotes = toLines project.ReleaseNotes.Notes})
)

Target "PublishNuget" (fun _ ->
    Paket.Push(fun p -> 
        { p with
            WorkingDir = "bin" })
)

Target "All" DoNothing

"Clean"
  ==> "Build"
  ==> "RunTests"
  ==> "All"

"All"
  ==> "PackageNuget"
  ==> "PublishNuget"

RunTargetOrDefault "All"