# Fable.Parsimmon
[Fable](http://fable.io/) binding for the [Parsimmon](https://github.com/jneen/parsimmon) parser with a simple API. 

Example from the tests:
```fs
QUnit.test "Parsing list of numbers works" <| fun test ->
    let commaSeperatedNumbers = 
        Parsimmon.digit
        |> Parsimmon.many
        |> Parsimmon.map (String.concat "")
        |> Parsimmon.map int
        |> Parsimmon.seperateBy (Parsimmon.str ",")

    let leftBracket = Parsimmon.str "["
    let rightBraket = Parsimmon.str "]"
    
    commaSeperatedNumbers
    |> Parsimmon.between leftBracket rightBraket
    |> Parsimmon.parse "[5,10,15,20,25]"
    |> function
        | Some [| 5; 10; 15; 20; 25 |] -> test.pass()
        | otherwise -> test.fail()
``` 