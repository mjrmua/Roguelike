module Command
type public Direction =   Up 
                        | Down 
                        | Left 
                        | Right


type public Command =  Move of Direction 
                     | NoOp
