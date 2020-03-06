module CoronaFetch.App

open System



[<EntryPoint>]
let rec main args = 
    args 
        |> List.ofArray 
        |> function 
            | ["fetch"] -> fetchStats()
            | ["read"; country] -> WorldOMeter.readCountry(country)
                                    |> Csv.printRecords
            | _ -> showHelp()
    0

and readCountry(country) = 
    let records = WorldOMeter.readCountry(country)

    printfn "Files: %A" records

and showHelp() = 
    printfn "%A" [
        "fetch - Fetches the latest data"
        "read $country - Fetches data for a specific country"
    ]

and fetchStats() =
    Console.Clear()
    
    let worldoMeter = WorldOMeter.WorldOMeter()


    worldoMeter.CountrySummary
        |> CsvArchive.writeRecords "world-o-meter"
        |> ignore

