module Fable.Parsimmon.Tests

open FSharp.Core
open Fable.Parsimmon
open Fable.Import

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
        | otherwise -> test.fail()

QUnit.test "Parsimmon.many works" <| fun test ->
    Parsimmon.oneOf "abc"
    |> Parsimmon.many
    |> Parsimmon.parse "abc"
    |> function 
        | Some [| "a"; "b"; "c" |] -> test.pass()
        | otherwise -> test.fail()

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


QUnit.test "Parsimmon.digit works" <| fun test ->
    Parsimmon.digit
    |> Parsimmon.map int
    |> Parsimmon.many
    |> Parsimmon.parse "123"
    |> function
        | Some [| 1; 2; 3 |] -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.digit should not consume multiple digits" <| fun test ->
    Parsimmon.digit
    |> Parsimmon.parse "12"
    |> Option.isNone
    |> test.equal true

QUnit.test "Parsimmon.digits works" <| fun test ->
    Parsimmon.digits
    |> Parsimmon.map (Array.map int)
    |> Parsimmon.parse "123"
    |> function 
        | Some xs -> Array.sum xs |> test.equal 6
        | otherwise -> test.fail()

QUnit.test "Parsimmon.letter works" <| fun test ->
    Parsimmon.letter
    |> Parsimmon.many
    |> Parsimmon.map (Array.map  (fun token -> token.ToUpper()))
    |> Parsimmon.parse "abc"
    |> function
        | Some [| "A"; "B"; "C" |] -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Optional Whitesapce works" <| fun test -> 
    Parsimmon.optionalWhitespace
    |> Parsimmon.chain Parsimmon.digit
    |> Parsimmon.skip Parsimmon.optionalWhitespace
    |> Parsimmon.parse " 5 "
    |> function
        | Some "5" -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.seperateBy works" <| fun test ->
    Parsimmon.digit
    |> Parsimmon.map int
    |> Parsimmon.seperateBy (Parsimmon.ofString ",")
    |> Parsimmon.parse "1,2,3,4,5"
    |> function
        | Some xs -> Array.sum xs |> test.equal 15
        | otherwise -> test.fail()

QUnit.test "Parsimmon.between works" <| fun test ->
    let leftBraket = Parsimmon.ofString "["
    let rightBracket = Parsimmon.ofString "]"
    Parsimmon.digit
    |> Parsimmon.between leftBraket rightBracket
    |> Parsimmon.map int
    |> Parsimmon.parse "[5]"
    |> function
        | Some 5 -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.whitespace works" <| fun test -> 
    Parsimmon.letter
    |> Parsimmon.seperateBy Parsimmon.whitespace
    |> Parsimmon.parse "a b c"
    |> function
        | Some [| "a"; "b"; "c" |] -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.chain works" <| fun test ->
    Parsimmon.ofString "a"
    |> Parsimmon.chain Parsimmon.digit
    |> Parsimmon.map int
    |> Parsimmon.parse "a5"
    |> function 
        | Some 5 -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsing list of numbers works" <| fun test ->
    let commaSeperatedNumbers = 
        Parsimmon.digit
        |> Parsimmon.many
        |> Parsimmon.map (String.concat "")
        |> Parsimmon.map int
        |> Parsimmon.seperateBy (Parsimmon.ofString ",")

    let leftBracket = Parsimmon.ofString "["
    let rightBraket = Parsimmon.ofString "]"
    commaSeperatedNumbers
    |> Parsimmon.between leftBracket rightBraket
    |> Parsimmon.parse "[5,10,15,20,25]"
    |> function
        | Some [| 5; 10; 15; 20; 25 |] -> test.pass()
        | otherwise -> test.fail()


QUnit.test "Parsing list of numbers works with whitespace" <| fun test ->
    let optWs = Parsimmon.optionalWhitespace
    let commaSeperatedNumbers = 
        Parsimmon.digit 
        |> Parsimmon.between optWs optWs
        |> Parsimmon.many
        |> Parsimmon.map (String.concat "")
        |> Parsimmon.map int
        |> Parsimmon.seperateBy (Parsimmon.ofString ",")

    let leftBracket = Parsimmon.ofString "["
    let rightBraket = Parsimmon.ofString "]"
    commaSeperatedNumbers
    |> Parsimmon.between leftBracket rightBraket
    |> Parsimmon.parse "[ 5 ,10  , 15 , 20,25]"
    |> function
        | Some [| 5; 10; 15; 20; 25 |] -> test.pass()
        | otherwise -> test.fail()