#r @"packages/build/FAKE/tools/FakeLib.dll"
open System.Threading.Tasks

open System

open Fake

let libPath = "./Fable.Parsimmon"
let testsPath = "./Fable.Parsimmon.Tests"

let platformTool tool winTool =
  let tool = if isUnix then tool else winTool
  tool
  |> ProcessHelper.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"

let mutable dotnetCli = "dotnet"

let run cmd args workingDir =
  let result =
    ExecProcess (fun info ->
      info.FileName <- cmd
      info.WorkingDirectory <- workingDir
      info.Arguments <- args) TimeSpan.MaxValue
  if result <> 0 then failwithf "'%s %s' failed" cmd args

Target "Clean" <| fun _ ->
    CleanDir (testsPath </> "bin")
    CleanDir (testsPath </> "obj")
    CleanDir (libPath </> "bin")
    CleanDir (libPath </> "obj")



Target "InstallClient" (fun _ ->
  printfn "Node version:"
  run nodeTool "--version" __SOURCE_DIRECTORY__
  run "npm" "--version" __SOURCE_DIRECTORY__
  run "npm" "install" __SOURCE_DIRECTORY__
  run dotnetCli "restore" testsPath
)

Target "RunTests" <| fun _ ->
    run dotnetCli "fable npm-run start" testsPath

let publish projectPath = fun () ->
    [ projectPath </> "bin"
      projectPath </> "obj" ] |> CleanDirs
    run dotnetCli "restore --no-cache" projectPath
    run dotnetCli "pack -c Release" projectPath
    let nugetKey =
        match environVarOrNone "NUGET_KEY" with
        | Some nugetKey -> nugetKey
        | None -> failwith "The Nuget API key must be set in a NUGET_KEY environmental variable"
    let nupkg = 
        System.IO.Directory.GetFiles(projectPath </> "bin" </> "Release") 
        |> Seq.head 
        |> System.IO.Path.GetFullPath
        
    let pushCmd = sprintf "nuget push %s -s nuget.org -k %s" nupkg nugetKey
    run dotnetCli pushCmd projectPath

Target "PublishNuget" (publish libPath)

"Clean"
  ==> "InstallClient"
  ==> "RunTests"

RunTargetOrDefault "RunTests"