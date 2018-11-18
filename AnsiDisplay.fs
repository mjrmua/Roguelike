module AnsiDisplay
    open System
    open System.Linq
    open System.Collections.Generic
    open Command
    open System.Reactive.Subjects
    open System.Threading.Tasks
    type Colour = int*int*int

    type public WorldObject = { 
        id: string
        position: int*int
    }

    type Character = {
        zIndex: int;
        foreground: Colour;
        background: Colour;
        character: char
    }

    type Screen = Character list list

    let move (x,y) object =  
        let (o_x, o_y) = object.position
        { object with position = (x+o_x, y+o_y)}

    let mappings = dict [ 
            ("player", {
                 zIndex=1
                ;foreground=(256,255,128)
                ;background=(0,0,0)
                ;character='@'
                }) 
           ;("wall", {
                 zIndex=1
                ;foreground=(0,0,0)
                ;background=(255,255,255)
                ;character=' '
                }) 
           ;("floor", {
                 zIndex=0
                ;foreground=(128,128,128)
                ;background=(0,0,0)
                ;character='.'
                }); 
        ]

    let backgroundCode (r,g,b) =  sprintf "\x1b[48;2;%i;%i;%im" r g b
    let foregroundCode (r,g,b) =  sprintf "\x1b[38;2;%i;%i;%im" r g b

    let write (x,y) (character:Character) = 
        Console.ForegroundColor <- ConsoleColor.Red
        Console.SetCursorPosition (x,y)
        let (r,g,b) = character.foreground
        Console.Write(sprintf "%s%s%c\x1b[0m"  
                        (foregroundCode character.foreground) 
                        (backgroundCode character.background) 
                        character.character)

    let viewPortSize = (Console.WindowWidth, Console.WindowHeight)

    let centerOnPlayer (objects:WorldObject list) = 
        let (view_x, view_y) = (Console.WindowWidth, Console.WindowHeight)
        let (player_x, player_y) = objects.Single(fun v->v.id="player").position
        let offset = (view_x/2-player_x, view_y/2-player_y)
        objects |> List.map (move offset)
                |> List.filter (fun {position=(x,y)}->x<view_x && y< view_y && x>0 && y>0)


    let render (mappings:IDictionary<string,Character>) (objects:WorldObject list) = 
        Console.BackgroundColor <- ConsoleColor.Black
        Console.Clear()
        Console.CursorVisible <- false 

        let toDraw = 
            objects 
                |> centerOnPlayer
                |> List.sortBy (fun v -> mappings.Item(v.id).zIndex)

        for obj in toDraw do
            write obj.position (mappings.Item(obj.id))

    let inputMap key = 
        match key with
            | ConsoleKey.UpArrow -> Command.Move Direction.Up
            | ConsoleKey.DownArrow -> Command.Move Direction.Down
            | ConsoleKey.LeftArrow -> Command.Move Direction.Left
            | ConsoleKey.RightArrow -> Command.Move Direction.Right
            | _ -> Command.NoOp

    let launch (worldState: WorldObject list IObservable) (commandSubject: Subject<Command.Command>)= 
        worldState |> Observable.subscribe (render mappings) |> ignore
        (Task.Factory.StartNew (fun ()->
            while true do 
            let key = Console.ReadKey().Key
            commandSubject.OnNext(inputMap key)
        )).Wait()