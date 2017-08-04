namespace Fable.Parsimmon

open Fable.Core
open Fable.Core.JsInterop

type ParseResult<'t> = 
    abstract status : bool
    abstract value : 't 

type IParser<'t> = 
    abstract map<'u> : ('t -> 'u) -> IParser<'u>
    abstract parse : string -> ParseResult<'t>
    abstract times<'t> : int -> IParser<'t []>
    abstract many<'t> : unit -> IParser<'t []>


module Parsimmon = 
    let parse<'t> (input: string) (parser: IParser<'t>) = 
        parser.parse input
        |> fun result -> 
            match result.status with
            | true -> Some result.value
            | false -> None

    let times<'t> (n: int) (parser : IParser<'t>) : IParser<'t[]> = 
        parser.times n

    let many (parser : IParser<'t>) : IParser<'t[]> = 
        parser.many()

    let map (f: 't -> 'u) (parser: IParser<'t>) = parser.map f

    let ofString (input: string) : IParser<string> = 
        import "string" "./Parsimmon.js"

    let oneOf (input: string) : IParser<string> = 
        import "oneOf" "./Parsimmon"

    let noneOf (input: string) : IParser<string> = 
        import "noneOf" "./Parsimmon"


