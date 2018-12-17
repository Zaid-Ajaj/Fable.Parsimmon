module Fable.Parsimmon.Tests

open FSharp.Core
open Fable.Parsimmon
open Fable.Import
open Fable.Core.JsInterop

#nowarn "40"

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
    |> Parsimmon.map (List.ofArray >> List.map int)
    |> Parsimmon.parse "123"
    |> function 
        | Some xs -> List.sum xs |> test.equal 6
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

QUnit.test "Parsimmon.between with many works" <| fun test ->
    let leftBraket = Parsimmon.str "["
    let rightBracket = Parsimmon.str "]"
    Parsimmon.digit
    |> Parsimmon.between leftBraket rightBracket
    |> Parsimmon.map int
    |> Parsimmon.many 
    |> Parsimmon.parse "[5][6][7]"
    |> function
        | Some [| 5; 6; 7 |] -> test.pass()
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

QUnit.test "Parsimmon.timesBetween min max works" <| fun test ->
   
    Parsimmon.str "a"
    |> Parsimmon.timesBetween 3 5
    |> Parsimmon.parse "aaa"
    |> function 
        | Some [| "a"; "a"; "a" |] -> test.pass() 
        | otherwise -> test.fail() 

    Parsimmon.str "a"
    |> Parsimmon.timesBetween 3 5
    |> Parsimmon.parse "aa"
    |> function 
        | None -> test.pass()
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

QUnit.test "Parsimmon.ofLazy works with single character strings" <| fun test -> 
    
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

QUnit.test "Parsimmon.ofLazy works with multiple character strings" <| fun test -> 
    let rec lazyValue = Parsimmon.ofLazy <| fun () -> 
        [ Parsimmon.str "XY" 
          Parsimmon.str "("
             |> Parsimmon.chain lazyValue
             |> Parsimmon.skip (Parsimmon.str ")") ]
        |> Parsimmon.choose

    ["XY"; "(XY)"; "((XY))"] 
    |> List.map (fun token -> Parsimmon.parse token lazyValue)
    |> function 
        | [ Some "XY"; Some "XY"; Some "XY" ] -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.ofLazy works with single digit parser" <| fun test -> 
    
    let rec lazyValue = Parsimmon.ofLazy <| fun () -> 
        [ Parsimmon.digit |> Parsimmon.map int
          Parsimmon.str "("
             |> Parsimmon.chain lazyValue
             |> Parsimmon.skip (Parsimmon.str ")") ]
        |> Parsimmon.choose
    

    ["5"; "(6)"; "((7))"] 
    |> List.map (fun token -> Parsimmon.parse token lazyValue)
    |> function 
        | [ Some 5; Some 6; Some 7 ] -> test.pass()
        | otherwise -> test.fail()

QUnit.test "Parsimmon.ofLazy works with multiple digit parser" <| fun test -> 
    let rec lazyValue = Parsimmon.ofLazy <| fun () -> 
        [ Parsimmon.digit 
             |> Parsimmon.atLeastOneOrMany // this is a must, Parsimmon.many won't work
             |> Parsimmon.concat
             |> Parsimmon.map int
          Parsimmon.str "("
             |> Parsimmon.chain lazyValue
             |> Parsimmon.skip (Parsimmon.str ")") ]
        |> Parsimmon.choose
    
    lazyValue
    |> Parsimmon.parse "52"
    |> function
        | Some 52 -> test.passWith "No parenthesis case works"
        | otherwise -> test.failWith "No parenthesis case does not work"

    lazyValue
    |> Parsimmon.parse "(65)"
    |> function
        | Some 65 -> test.passWith "One parenthesis case works"
        | otherwise -> test.failWith "One parenthesis case does not work"

    lazyValue
    |> Parsimmon.parse "((89))"
    |> function
        | Some 89 -> test.passWith "Multiple parenthesis case works"
        | otherwise -> test.failWith "Multiple parenthesis case does not work"


// from https://github.com/jneen/parsimmon/blob/master/API.md#parsimmonindex
QUnit.test "Parsimmon.index works" <| fun test ->
    Parsimmon.seq3 
        (Parsimmon.oneOf "Q\n" |> Parsimmon.many)
        (Parsimmon.str "B")
        Parsimmon.index
    |> Parsimmon.parse "QQ\n\nQQQB"
    |> function
        | Some (_, _, index) -> 
            test.equal index.offset 8
            test.equal index.line 3
            test.equal index.column 5

        | otherwise -> test.fail()


type NestedList<'a> = 
    | Element of 'a
    | Many of NestedList<'a> list


QUnit.test "Parsimmon.ofLazy works with list of digits parser" <| fun test -> 
    
    let rec lazyValue = Parsimmon.ofLazy <| fun () -> 

        let elementParser = 
          Parsimmon.digit 
          |> Parsimmon.atLeastOneOrMany // this is a must, Parsimmon.many won't work
          |> Parsimmon.concat
          |> Parsimmon.map (int >> Element)
          
        let expression = 
            lazyValue
            |> Parsimmon.seperateBy (Parsimmon.str " ")
        
        let listParser = 
          Parsimmon.str "["
          |> Parsimmon.chain expression
          |> Parsimmon.skip (Parsimmon.str "]")
          |> Parsimmon.map (List.ofArray >> Many)   
        
        Parsimmon.choose [ listParser; elementParser ]
    
    Parsimmon.parse "555" lazyValue
    |> function 
        | Some (Element 555) -> test.passWith "Single element case works"
        | otherwise -> test.failWith "Single element case fails"

    Parsimmon.parse "[5]" lazyValue
    |> function 
        | Some (Many [Element 5]) -> test.passWith "Single element list case works"
        | otherwise -> test.failWith "Single element list case fails"

    Parsimmon.parse "[]" lazyValue
    |> function 
        | Some (Many []) -> test.passWith "empty list case works"
        | otherwise -> test.failWith "empty list case fails"

    Parsimmon.parse "[5 6 7]" lazyValue
    |> function 
        | Some (Many [Element 5; Element 6; Element 7]) -> test.passWith "many elements single list case works"
        | otherwise -> test.failWith "many elements single list case fails"

    Parsimmon.parse "[[5 6 7]]" lazyValue
    |> function 
        | Some (Many [Many [Element 5; Element 6; Element 7]]) -> test.passWith "many nested elements single list case works"
        | otherwise -> test.failWith "many nested elements single list case fails"

    Parsimmon.parse "[1 [5 6 7]]" lazyValue
    |> function 
        | Some (Many [Element 1; Many [Element 5; Element 6; Element 7]]) -> test.passWith "many nested elements many list case works"
        | otherwise -> test.failWith "many nested elements many list case fails"

    Parsimmon.parse "[1 [5 6 7] [1]]" lazyValue
    |> function 
        | Some (Many [Element 1; Many [Element 5; Element 6; Element 7]; Many [Element 1]]) -> test.passWith "many nested elements many list case works"
        | otherwise -> test.failWith "many nested elements many list case fails"

QUnit.test "Parsimmon.node works with correct positions" <| fun test -> 
    let pA = Parsimmon.letter "a" |> Parsimmon.many
    let p = Parsimmon.between pA Parsimmon.digits pA
    let result =
        Parsimmon.parse "ab12dc" p
        |> Parsimmon.node "digits"

    test.isTrue result.status
    test.equal "12" result.value