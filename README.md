# Fable.Parsimmon
[Fable](http://fable.io/) binding and helpers for the [Parsimmon](https://github.com/jneen/parsimmon) parser combinator library.

[![Nuget](https://img.shields.io/nuget/v/Fable.Parsimmon.svg?colorB=green)](https://www.nuget.org/packages/Fable.Parsimmon) 

# Installation
You can install the library from Nuget using Paket:
```
paket add nuget Fable.Parsimmon --project path/to/YourProject.fsproj 
```
It includes the javascript dependency [(this file)](https://github.com/Zaid-Ajaj/Fable.Parsimmon/blob/master/Fable.Parsimmon/Parsimmon.js) in the Nuget package so there no need to install anything else using `npm`

Make sure the references are added to your paket files
```
// paket.dependencies (solution-wide dependencies)
nuget Fable.Parsimmon

// paket.refernces (project-specific dependencies)
Fable.Parsimmon
```


To understand how to use it, refer to the project `Fable.Parsimmon.Tests` where most of the combinators are tested, for example:
```fs
open Fable.Parsimmon 

QUnit.test "Parsing list of numbers works" <| fun test ->
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
``` 
Recursive parsers as values such as [this one](https://github.com/jneen/parsimmon/blob/master/API.md#parsimmonlazyfn) are also supported:
```fs
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
```