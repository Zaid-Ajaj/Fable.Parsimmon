namespace Fable.Parsimmon

open Fable.Core
open Fable.Core.JsInterop

type ParseResult<'t> = 
    abstract status : bool
    abstract value : 't 

type IParser<'t> = 
    abstract map<'u> : ('t -> 'u) -> IParser<'u>
    abstract parse : string -> ParseResult<'t>
    abstract times : int -> IParser<'t []>
    abstract many : unit -> IParser<'t []>
    [<Emit("$0.then($1)")>]
    abstract chain : IParser<'u> -> IParser<'u>
    [<Emit("$0.chain($1)")>]
    abstract bind : ('t -> IParser<'u>) -> IParser<'u>
    abstract skip : IParser<'u> -> IParser<'t>
    abstract sepBy : IParser<'u> -> IParser<'t []>

module Parsimmon = 
    let parseRaw (input: string) (parser: IParser<'t>) =
        parser.parse input

    let parse (input: string) (parser: IParser<'t>) = 
        parser.parse input
        |> fun result -> 
            match result.status with
            | true -> Some result.value
            | false -> None

    let times<'t> (n: int) (parser : IParser<'t>) : IParser<'t[]> = 
        parser.times n

    let skip (skipped: IParser<'u>) (keep: IParser<'t>) : IParser<'t> = 
        keep.skip skipped

    let many (parser : IParser<'t>) : IParser<'t[]> = 
        parser.many()

    let chain  (after: IParser<'u>) (before: IParser<'t>) : IParser<'u> =    
        before.chain after
    
    let bind (f: 't -> IParser<'u>) (p: IParser<'t>) : IParser<'u> =    
        p.bind f

    let letter : IParser<string> = 
        import "letter" "./Parsimmon.js"

    let letters : IParser<string> = 
        import "letters" "./Parsimmon.js"

    let digit : IParser<string> = 
        import "digit" "./Parsimmon.js"

    let digits : IParser<string[]> = 
        import "digits" "./Parsimmon.js"

    let seperateBy (content: IParser<'u>) (others: IParser<'t>) : IParser<'t[]> =
        others.sepBy(content)

    let between (left: IParser<'t>) (right: IParser<'u>) (middle: IParser<'v>) =
        left 
        |> chain middle 
        |> skip right

    let map (f: 't -> 'u) (parser: IParser<'t>) = parser.map f

    let satisfy (f: string -> bool) : IParser<'t> = 
        import "test" "./Parsimmon.js"

    let str (input: string) : IParser<string> = 
        import "string" "./Parsimmon.js"

    let oneOf (input: string) : IParser<string> = 
        import "oneOf" "./Parsimmon.js"

    let whitespace : IParser<string> = 
        import "whitespace" "./Parsimmon.js"

    let optionalWhitespace : IParser<string> = 
        import "optWhitespace" "./Parsimmon.js"

    let noneOf (input: string) : IParser<string> = 
        import "noneOf" "./Parsimmon.js"