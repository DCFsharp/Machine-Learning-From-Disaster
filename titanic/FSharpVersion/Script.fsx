// completed tutorial from https://gist.github.com/mathias-brandewinder/5558573

open System
open System.Diagnostics
open System.IO

let timer = Stopwatch()

timer.Start()

type Record  = { Label:int; Pixels:int[] }

let parseRecords path = 
    path
    |> File.ReadAllLines
    |> Array.map (fun (s:string) -> s.Split(',')) 
    |> (fun (a:string[][]) -> a.[ 1 .. ])
    |> Array.map (fun (line: string[]) -> line |> Array.map (Convert.ToInt32)) 
    |> Array.map (fun (i: int[]) -> { Label = i.[0]; Pixels = i.[ 1 .. ] }) 
    
                                // from http://brandewinder.blob.core.windows.net/public/trainingsample.csv
let knownRecords = parseRecords "../../../trainingsample.csv"

let distanceBetweenArrays (a1 : int[]) (a2 : int[]) = Array.map2(fun p1 p2 -> (p1 - p2) * (p1 - p2)) a1 a2

let aggDistanceBetweenArrays a1 a2 = distanceBetweenArrays a1 a2 |> Array.sum 
  
let classify (unknown:Record) =
    knownRecords |> Array.minBy (fun (r : Record) -> aggDistanceBetweenArrays unknown.Pixels r.Pixels)


// Now on to validating our classifier..
 
type TestRecord  = { ExpectedLabel:int; FoundLabel:int; CorrectPercent:double}

                                  // from http://brandewinder.blob.core.windows.net/public/validationsample.csv
let unknownRecords = parseRecords "../../../validationsample.csv"

let testedRecords = unknownRecords |> Array.map (fun (r:Record) -> 
        let foundLabel = (classify r).Label
        {
            FoundLabel = foundLabel; 
            ExpectedLabel = r.Label;      
            CorrectPercent = if foundLabel.Equals(r.Label) then 1.0 else 0.0
        })

testedRecords |> Array.iter (fun (r:TestRecord) -> 
                    printfn 
                        "Expected: %i   Found: %i    Match: %b" 
                        r.ExpectedLabel 
                        r.FoundLabel 
                        (r.ExpectedLabel.Equals(r.FoundLabel)))

let percentCorrect = testedRecords |> Array.averageBy (fun (r:TestRecord) -> r.CorrectPercent)

timer.Stop()

printfn "Percent Correct: %f in %f seconds" percentCorrect timer.Elapsed.TotalSeconds

