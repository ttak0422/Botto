namespace Botto.Options

type Key = string
type JsonString = string
type ErrMsg = string
type Index = int

[<AutoOpen>]
module Prelude =

    [<Literal>]
    let APP_NAME = "BOTTO"

    [<Literal>]
    let ENTER_KEY = "Enter"

    [<LiteralAttribute>]
    let CONFIG_KEY =
        "a2d6c2748bd5230d031bf100fbd5ca143d4ca6d82e64f6ed9e77203852d6e37f"

    let inline (>=>) f g = f >> Result.bind g

    let inline getDefault () : ^a =
        (^a: (static member Default : unit -> ^a) ())
