module Fable.Parsimmon.Tests

open FSharp.Core
open Fable.Parsimmon
open Fable.Import

QUnit.registerModule "Parsimmon Tests"

QUnit.test "Parsimmon.str works" <| fun test ->
    let parser = Parsimmon.str "hello"
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
    Parsimmon.str "hello"
    |> Parsimmon.times 2
    |> Parsimmon.parse "hellohello"
    |> function 
        | Some [| "hello"; "hello" |] -> test.pass()
        | Some other -> test.fail()
        | None -> test.fail()

QUnit.test "Parsimmon.map works" <| fun test ->
    Parsimmon.str "hello"
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

QUnit.test "Parsimmon.letters works" <| fun test ->
    Parsimmon.letters
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
    |> Parsimmon.map int
    |> Parsimmon.parse " 5 "
    |> function
        | Some 5 -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.seperateBy works" <| fun test ->
    Parsimmon.digit
    |> Parsimmon.map int
    |> Parsimmon.seperateBy (Parsimmon.str ",")
    |> Parsimmon.parse "1,2,3,4,5"
    |> function
        | Some xs -> Array.sum xs |> test.equal 15
        | otherwise -> test.fail()

QUnit.test "Parsimmon.between works" <| fun test ->
    let leftBraket = Parsimmon.str "["
    let rightBracket = Parsimmon.str "]"
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
    Parsimmon.str "a"
    |> Parsimmon.chain Parsimmon.digit
    |> Parsimmon.map int
    |> Parsimmon.parse "a5"
    |> function 
        | Some 5 -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsing list of numbers" <| fun test ->
    let comma = Parsimmon.str ","

    let commaSeperatedNumbers = 
        Parsimmon.digit
        |> Parsimmon.many
        |> Parsimmon.concat
        |> Parsimmon.map int
        |> Parsimmon.seperateBy comma

    let leftBracket = Parsimmon.str "["
    let rightBracket = Parsimmon.str "]"
    
    commaSeperatedNumbers
    |> Parsimmon.between leftBracket rightBracket
    |> Parsimmon.parse "[5,10,15,20,25]"
    |> function
        | Some [| 5; 10; 15; 20; 25 |] -> test.pass()
        | otherwise -> test.fail()


QUnit.test "Parsing list of numbers works with whitespace" <| fun test ->
    let optWs = Parsimmon.optionalWhitespace
    let commaSeperatedNumbers = 
        Parsimmon.digit 
        |> Parsimmon.trim optWs
        |> Parsimmon.many
        |> Parsimmon.tie
        |> Parsimmon.map int
        |> Parsimmon.seperateBy (Parsimmon.str ",")

    let leftBracket = Parsimmon.str "["
    let rightBracket = Parsimmon.str "]"
    commaSeperatedNumbers
    |> Parsimmon.between leftBracket rightBracket
    |> Parsimmon.parse "[ 5 ,10  , 15 , 20,25]"
    |> function
        | Some [| 5; 10; 15; 20; 25 |] -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.satisfy works" <| fun test ->
    Parsimmon.satisfy (fun input -> input.ToUpper() = input)
    |> fun parser -> 
        ["a"; "+"; "b" ]
        |> List.map (fun token -> Parsimmon.parse token parser)
        |> function 
            | [None; Some "+"; None] -> test.pass()
            | otherwise -> test.fail()


QUnit.test "Parsimmon.fallback works" <| fun test ->
    Parsimmon.digit
    |> Parsimmon.map int
    |> Parsimmon.fallback 0
    |> fun parser -> 
        ["5"; ""; "1"]
        |> List.map (fun input -> Parsimmon.parse input parser)
        |> function
            | [Some 5; Some 0; Some 1] -> test.pass()
            | otherwise -> test.fail()

QUnit.test "Parsimmon.seq2 works" <| fun test -> 
    Parsimmon.seq2 
         (Parsimmon.digit |> Parsimmon.map int)
         (Parsimmon.digit |> Parsimmon.map (int >> (+) 1))
    |> Parsimmon.map (fun (a, b) -> a + b)
    |> Parsimmon.parse "56"
    |> function 
        | Some 12 -> test.pass()
        | otherwise -> test.fail()
    
QUnit.test "Parsimmon.seq3 works" <| fun test -> 
    Parsimmon.seq3 
         (Parsimmon.digit |> Parsimmon.map (int >> (+) 1))
         (Parsimmon.digit |> Parsimmon.map (int >> (+) 2))
         (Parsimmon.digit |> Parsimmon.map (int >> (+) 3))
    |> Parsimmon.map (fun (a, b, c) -> a + b + c)
    |> Parsimmon.parse "123"
    |> function 
        | Some 12 -> test.pass()
        | otherwise -> test.fail()


QUnit.test "Parsimmon.orTry works" <| fun test ->
    let parser = 
      Parsimmon.str "+"
      |> Parsimmon.orTry (Parsimmon.str "-")

    parser
    |> Parsimmon.parse "+"
    |> Option.isSome
    |> fun result -> test.equalWithMsg result true "First parser works"

    parser
    |> Parsimmon.parse "-"
    |> Option.isSome
    |> fun result -> test.equalWithMsg result true "Second parser works"

    parser
    |> Parsimmon.parse "other-text"
    |> function
        | None -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.choose works" <| fun test -> 
    [ Parsimmon.str "a"
      Parsimmon.str "ab" ] 
    |> Parsimmon.choose
    |> Parsimmon.parse "ab"
    |> function
        | None -> test.pass()
        | otherwise -> test.fail()

    [ Parsimmon.str "ab" 
      Parsimmon.str "a"  ]
    |> Parsimmon.choose
    |> Parsimmon.parse "ab"
    |> function
        | Some "ab" -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.ofLazy works" <| fun test -> 
    
    let rec lazyValue = Parsimmon.ofLazy <| fun () -> 
        [ Parsimmon.str "X" 
          Parsimmon.str "("
             |> Parsimmon.chain lazyValue
             |> Parsimmon.skip (Parsimmon.str ")") ]
        |> Parsimmon.choose
    

    ["X"; "(X)"; "((X))"] 
    |> List.map (fun token -> Parsimmon.parse token lazyValue)
    |> function 
        | [ Some "X"; Some "X"; Some "X" ] -> test.pass()
        | otherwise -> test.fail()
