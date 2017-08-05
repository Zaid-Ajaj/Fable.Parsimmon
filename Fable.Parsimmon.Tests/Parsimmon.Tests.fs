module Fable.Parsimmon.Tests

open FSharp.Core
open Fable.Parsimmon
open Fable.Import.Browser

QUnit.registerModule "Parsimmon Tests"

QUnit.test "Parsimmon.ofString works" <| fun test ->
    let parser = Parsimmon.ofString "hello"
    parser
    |> Parsimmon.parse "hello"
    |> function 
        | Some value -> test.equal value "hello"
        | None -> test.fail()

    parser
    |> Parsimmon.parse "other"
    |> function
        | Some value -> test.failWith "Should not have parsed the string"
        | None -> test.pass()

QUnit.test "Parsimmon.oneOf works" <| fun test -> 
    let parser = Parsimmon.oneOf "abc"
    ["a"; "b"; "c"]
    |> List.choose (fun token -> Parsimmon.parse token parser)
    |> List.length
    |> test.equal 3 

    ["e"; "f"; "g"]
    |> List.choose (fun token -> Parsimmon.parse token parser)
    |> List.length 
    |> test.equal 0

QUnit.test "Parsimmon.times works" <| fun test -> 
    Parsimmon.ofString "hello"
    |> Parsimmon.times 2
    |> Parsimmon.parse "hellohello"
    |> function 
        | Some [| "hello"; "hello" |] -> test.pass()
        | Some other -> test.fail()
        | None -> test.fail()

QUnit.test "Parsimmon.map works" <| fun test ->
    Parsimmon.ofString "hello"
    |> Parsimmon.map (fun result -> result.Length)
    |> Parsimmon.parse "hello"
    |> function 
        | Some 5 -> test.pass()
        | _ -> test.fail()

QUnit.test "Parsimmon.many works" <| fun test ->
    Parsimmon.oneOf "abc"
    |> Parsimmon.many
    |> Parsimmon.parse "abc"
    |> function 
        | Some [| "a"; "b"; "c" |] -> test.pass()
        | _ -> test.fail()

QUnit.test "Parsimmon.noneOf works" <| fun test ->
    let parser = Parsimmon.noneOf "abc"
    ["a"; "b"; "c"]
    |> List.choose (fun token -> Parsimmon.parse token parser)
    |> List.length
    |> test.equal 0

    ["e"; "f"; "g"]
    |> List.choose (fun token -> Parsimmon.parse token parser)
    |> List.length 
    |> test.equal 3