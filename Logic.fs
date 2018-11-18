module Logic
open Command
open System.Linq

type public ObjectType =  {
    id: string;
    passable: bool;
}

type public WorldObject  = {
    objectType: ObjectType;
    position: int*int;
}


type public World = WorldObject list

let shift direction (x,y) = 
        match direction with
            | Direction.Up -> (x,y-1)
            | Direction.Down -> (x,y+1)
            | Direction.Left -> (x-1,y)
            | Direction.Right -> (x+1,y)
let move dir obj = 
    { obj with position = shift dir obj.position}

let getPlayer (world:World) = world.First(fun v->v.objectType.id="player")
let movePlayer dir v = 
    if v.objectType.id="player" then 
        move dir v 
    else v

let isPassable (world:World) position = 
    not <| world.Any(fun v->v.position=position && not v.objectType.passable)

let isValid world = function
    | Move dir -> world |> getPlayer |> fun v->v.position |> shift dir |> isPassable world
    | _ -> true

let reduce world cmd = 
    if (not <| isValid world cmd) then world else
        match cmd with
            | Command.Move dir -> world |> List.map (movePlayer dir)
            | _->world
