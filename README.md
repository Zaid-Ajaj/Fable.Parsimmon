# Fable.Parsimmon
[Fable](http://fable.io/) binding and helpers for the [Parsimmon](https://github.com/jneen/parsimmon) parser combinator library.

# Installation
You can install the library from Nuget using Paket:
```
paket add nuget Fable.Parsimmon --project path/to/YourProject.fsproj 
```
It includes the javascript dependency [(this file)](https://github.com/Zaid-Ajaj/Fable.Parsimmon/blob/master/Fable.Parsimmon/Parsimmon.js) in the Nuget package so there no need to install anything else using `npm`


To understand how to use it, refer to the project `Fable.Parsimmon.Tests` where most of the combinators are tested, for example:
```fs
open Fable.Parsimmon 

QUnit.test "Parsing list of numbers works" <| fun test ->
    let comma = Parsimmon.str ","

    let commaSeperatedNumbers = 
        Parsimmon.digit
        |> Parsimmon.many
        |> Parsimmon.tie
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
``` 