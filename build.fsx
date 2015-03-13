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

let release = LoadReleaseNotes "RELEASE_NOTES.md"

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

Target "All" DoNothing

"Clean"
  ==> "Build"
  ==> "RunTests"
  ==> "All"


RunTargetOrDefault "All"