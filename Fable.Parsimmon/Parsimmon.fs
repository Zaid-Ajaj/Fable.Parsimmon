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
    abstract fallback : 't -> IParser<'t>
    abstract trim : IParser<'u> -> IParser<'t>
    abstract notFollowedBy : IParser<'u> -> IParser<'t>
    abstract atMost : int -> IParser<'t[]>
    abstract atLeast : int -> IParser<'t[]>
    [<Emit("$0.or($1)")>]
    abstract orTry : IParser<'t> -> IParser<'t>
    abstract sepBy1 : IParser<'u> -> IParser<'t []>

module Parsimmon = 
    let parseRaw (input: string) (parser: IParser<'t>) =
        parser.parse input

    let parse (input: string) (parser: IParser<'t>) = 
        parser.parse input
        |> fun result -> 
            match result.status with
            | true -> Some result.value
            | false -> None

    /// Returns a new parser which tries parser, and if it fails uses otherParser. Example:
    let orTry (otherParser: IParser<'t>) (parser: IParser<'t>) : IParser<'t> = 
        parser.orTry(otherParser)

    let times<'t> (n: int) (parser : IParser<'t>) : IParser<'t[]> = 
        parser.times n

    /// Expects parser at least n times. Yields an array of the results.
    let atLeast (n: int) (parser: IParser<'t>) : IParser<'t[]> = 
        parser.atLeast n


    /// Expects parser at most n times. Yields an array of the results.
    let atMost (n: int) (parser: IParser<'t>) : IParser<'t[]> = 
        parser.atMost n

    let skip (skipped: IParser<'u>) (keep: IParser<'t>) : IParser<'t> = 
        keep.skip skipped

    let many (parser : IParser<'t>) : IParser<'t[]> = 
        parser.many()

    /// This is the same as Parsimmon.sepBy, but matches the parser at least once.
    let seperateByAtLeastOne (seperator : IParser<'u>) (parser: IParser<'t>) : IParser<'t[]> = 
        parser.sepBy1(seperator)

    /// Expects parser "after" to follow parser "before", and yields the result of "before".
    let chain  (after: IParser<'u>) (before: IParser<'t>) : IParser<'u> =    
        before.chain after
    
    /// Returns a new parser which tries parser "p", and on success calls the function "f" with the result of the parse, which is expected to return another parser, which will be tried next. This allows you to dynamically decide how to continue the parse, which is impossible with the other combinators.    
    let bind (f: 't -> IParser<'u>) (p: IParser<'t>) : IParser<'u> =    
        p.bind f

    /// A parser that consumes one letter
    let letter : IParser<string> = 
        import "letter" "./Parsimmon.js"

    /// A parser that consumes one or more letters
    let letters : IParser<string[]> = 
        import "letters" "./Parsimmon.js"

    /// A parser that expects to be at the end of the input (zero characters left).
    let endOfFile : IParser<string> =   
        import "eof" "./Parsimmon.js"
      
    /// Returns a parser that looks for anything but whatever "p" wants to parse, and does not consume it. Yields the same result as "before".
    let notFollowedBy (p: IParser<'u>) (before: IParser<'t>) : IParser<'t> = 
        before.notFollowedBy p

    // A parser that consumes one digit
    let digit : IParser<string> = 
        import "digit" "./Parsimmon.js"

    // A parser that consumes one or more digits
    let digits : IParser<string[]> = 
        import "digits" "./Parsimmon.js"

    /// Returns a new parser which tries "parser" and, if it fails, yields value without consuming any input.
    let fallback (value: 't) (parser: IParser<'t>) : IParser<'t> =
        parser.fallback value 

    let seperateBy (content: IParser<'u>) (others: IParser<'t>) : IParser<'t[]> =
        others.sepBy(content)



    let between (left: IParser<'t>) (right: IParser<'u>) (middle: IParser<'v>) =
        left 
        |> chain middle 
        |> skip right

    let map (f: 't -> 'u) (parser: IParser<'t>) = parser.map f

    let tie (parser: IParser<string[]>) : IParser<string> = 
        map (String.concat "") parser 

    /// A parser that consumes and yields the next character of the input.
    let any : IParser<string> = 
        import "any" "./Parsimmon.js"

    /// Accepts any number of parsers, yielding the value of the first one that succeeds, backtracking in between.
    let choose (ps: IParser<'t> list) : IParser<'t> = 
        List.reduce (fun acc parser -> acc.orTry(parser)) ps

    /// A parser that consumes and yields the entire remainder of the input.
    let all : IParser<string> = 
        import "all"  "./Parsimmon.js"

    /// Returns a failing parser with the given message.
    let fail (input: string) : IParser<string> = 
        import "fail" "./Parsimmon.js"

    /// Returns a parser that yield a single character if it passes the predicate function.
    let satisfy (f: string -> bool) : IParser<string> = 
        import "test" "./Parsimmon.js"

    /// Returns a parser yield a string containing all the next characters that pass the predicate "f"
    let takeWhile (f: string -> bool) : IParser<string> =
        import "takeWhile" "./Parsimmon.js"



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
    
    let seq2 (p1: IParser<'t>) (p2:IParser<'u>) :  IParser<'t * 'u> =  
        import "seq" "./Parsimmon.js"
    let trim (trimmed: IParser<'a>) (p: IParser<'t>) : IParser<'t> = 
        p.trim trimmed
    let seq3 (p1: IParser<'t>) 
             (p2: IParser<'u>) 
             (p3:IParser<'v>) :  IParser<'t * 'u * 'v> =  
        import "seq" "./Parsimmon.js"

    let seq4 (p1: IParser<'t>) 
             (p2: IParser<'u>) 
             (p3:IParser<'v>)
             (p4:IParser<'w>) :  IParser<'t * 'u * 'v * 'w> =  
        import "seq" "./Parsimmon.js"

    let seq5 (p1: IParser<'t>) 
             (p2: IParser<'u>) 
             (p3: IParser<'v>)
             (p4: IParser<'w>) 
             (p5: IParser<'q>) : IParser<'t * 'u * 'v * 'w * 'q> =  
        import "seq" "./Parsimmon.js"