#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.JavaScript.Yarn //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.JavaScript

let inline makeYarnParams (ws: string) (yarnParams: Yarn.YarnParams) =
    { yarnParams with
          WorkingDirectory = ws }

let optionProject = "./src/Botto.Options"
let targets = [ optionProject ]

Target.initEnvironment ()

Target.create
    "Clean"
    (fun _ ->
        !! "src/**/bin" ++ "src/**/obj" ++ "dist"
        |> Shell.cleanDirs)

Target.create
    "Restore"
    (fun _ ->
        Yarn.install id
        DotNet.restore id "./Botto.sln")

Target.create
    "WatchOption"
    (fun _ ->
        Yarn.exec "webpack serve" (makeYarnParams optionProject))

Target.create
    "Build"
    (fun _ ->
        Environment.setEnvironVar "NODE_ENV" "production"
        targets
        |> Seq.iter (fun target -> Yarn.exec "webpack" (makeYarnParams target)))

Target.create "All" ignore

"Clean" ==> "Restore" ==> "Build" ==> "All"

Target.runOrDefault "All"
