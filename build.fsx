#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.SemVerHelper
open Fake.NuGetVersion
open XamarinHelper
open System
open System.IO
open System.Security.Cryptography.X509Certificates
open Fake.AndroidPublisher
open System.Xml
open Fake.Testing.XUnit2
open HockeyAppHelper
open ProcessHelper
open System.Diagnostics

let androidBuildDir = __SOURCE_DIRECTORY__ @@ "droidBuild/"
let prodDir = __SOURCE_DIRECTORY__ @@ "pack/"
let nugetsDir = __SOURCE_DIRECTORY__ @@ "NuGet/"

let solutionFile  = "Xamarin.Droid.CrossWalk.sln"

let IncBuild:NuGetVersionIncrement = 
    fun (v:SemVerInfo) ->
        { v with Build=(System.Int32.Parse(v.Build)+1).ToString() }

let nugetKey = getBuildParamOrDefault "nugetKey" ""
let nugetPublishUrl = "http://nuget.org"
let nugetsVersions name dv = 
    NuGetVersion.nextVersion <|
        fun arg -> 
            { arg 
                with 
                    PackageName=name
                    DefaultVersion=dv
                    Increment=IncBuild
                    Server=(nugetPublishUrl + "api/v2")
            }

let publishNuget nupkg key url =
    let nugetExe = __SOURCE_DIRECTORY__ @@ ".nuget/nuget.exe"
    let args = sprintf "push %s %s -s %s" nupkg key url
    let dir = __SOURCE_DIRECTORY__ @@ "NuGet"
    ProcessHelper.Shell.Exec(nugetExe, args, dir) |> ignore

ensureDirectory androidBuildDir
ensureDirectory prodDir
ensureDirectory nugetsDir

Target "Clean" (fun _ ->
    CleanDir androidBuildDir
    CleanDir prodDir
    CleanDir nugetsDir
)

Target "NuGet" (fun _ ->

  let copyToNugetDir dir = 
    dir
    |> directoryInfo
    |> filesInDir
    |> Array.map(fun f -> f.FullName)
    |> CopyFiles nugetsDir

  let removeNotNugetFiles () =
    nugetsDir
    |> directoryInfo
    |> filesInDir
    |> Array.map(fun f -> f.FullName)
    |> Array.filter(fun n -> n.EndsWith ".nupkg" |> not)
    |> DeleteFiles

  copyToNugetDir iosBuildDir

  let version = "17.46.459.0"

  let androidArmVersion = nugetsVersions "Xamarin.Droid.CrossWalkLite.Arm" version
  let androidx86Version = nugetsVersions "Xamarin.Droid.CrossWalkLite.x86" version

  removeNotNugetFiles()

  copyToNugetDir androidBuildDir

  NuGet (fun p -> 
    { p with
        Authors = ["Yann ROBIN"]
        Project = "Xamarin.Droid.CrossWalkLite.Arm"
        Description = "CrossWalkLite for Xamarin Android ARM"
        OutputPath = nugetsDir
        Files = [
                  ("Xamarin.Droid.CrossWalkLite.Arm.dll", Some @"lib\MonoAndroid44\", None)
                ]
        Dependencies = []
        AccessKey = nugetKey
        PublishUrl = nugetPublishUrl
        Version = androidVersion
        Publish = true
        Properties = [("Configuration","Release")]
    }) "template.nuspec"
    
    NuGet (fun p -> 
    { p with
        Authors = ["Yann ROBIN"]
        Project = "Xamarin.Droid.CrossWalkLite.x86"
        Description = "CrossWalkLite for Xamarin Android x86"
        OutputPath = nugetsDir
        Files = [
                  ("Xamarin.Droid.CrossWalkLite.x86.dll", Some @"lib\MonoAndroid44\", None)
                ]
        Dependencies = []
        AccessKey = nugetKey
        PublishUrl = nugetPublishUrl
        Version = androidVersion
        Publish = true
        Properties = [("Configuration","Release")]
    }) "template.nuspec"

  removeNotNugetFiles()
)

Target "Build-Android-Core" (fun _ ->
    !! "**/Xamarin.Droid.CrossWalkLite.Arm.fsproj"
        |> MSBuildRelease androidBuildDir "Build"
        |> Log "BuildAndroidLib-Output: "

    !! "**/Xamarin.Droid.CrossWalkLite.x86.fsproj"
        |> MSBuildRelease androidBuildDir "Build"
        |> Log "BuildAndroidLib-Output: "
)

"Clean"
    ==> "Build-Android"
    ==> "NuGet"

RunTargetOrDefault "NuGet"
