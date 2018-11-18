open System
open FSharp.Control.Reactive
open AnsiDisplay
open System.Reactive.Subjects
open Command
open Logic
open System.Reactive.Subjects
open System.IO

let floor position : Logic.WorldObject = 
            {   objectType={id="floor"; passable=true;}; 
                position=position
            }

let player position : Logic.WorldObject = 
            {   objectType={id="player"; passable=false;}; 
                position=position
            }

let wall position : Logic.WorldObject = 
            {   objectType={id="wall"; passable=false;}; 
                position=position
            }

let mapWorldToDisplay (world:World) : AnsiDisplay.WorldObject list = 
    world |> List.map (fun obj -> {
        id = obj.objectType.id;
        position=obj.position
    }) 


let mapKey = function
    | '#' -> wall >> List.singleton
    | '.' -> floor >> List.singleton
    | '@' -> fun v->[floor v; player v]
    | _' -> fun v -> []

let rows (y:int) (x:int) (c:char) = mapKey c (x,y)

let readMap file = 
    File.ReadAllLines(file) 
        |> Array.toList
        |> List.mapi (fun y row -> Seq.mapi (rows y) row)
        |> Seq.collect (fun v->v)
        |> Seq.collect (fun v->v)
        |> Seq.toList


[<EntryPoint>]
let main argv =
    let initialState = readMap "map.txt"
    let state = new BehaviorSubject<World>(initialState)
    let input = new Subject<Command.Command>()
    input.Subscribe(fun cmd -> state.OnNext(
        Logic.reduce state.Value cmd
    )) |> ignore

    AnsiDisplay.launch (state |> Observable.map mapWorldToDisplay) input 
    0 // return an integer exit code
