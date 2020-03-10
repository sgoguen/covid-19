module CoronaFetch.App

open System
open System.Text.RegularExpressions
open System.IO


let nonLetters = Regex("[^\\w]")
let encodeCountryName(name:string) = 
    nonLetters.Replace(name, "-")



[<EntryPoint>]
let rec main args = 
    args 
        |> List.ofArray 
        |> function 
            | ["stats"; "fetch"] -> fetchStats()
            | ["stats"; "save"] -> saveStats()
            | ["read"; country] -> WorldOMeter.readCountry(country)
                                    |> Csv.printRecords
            | ["writeAll"] ->
                let countries = WorldOMeter.allCountries.Value
                for c in countries do
                    let filename = encodeCountryName(c) + ".csv"
                    let path = Path.GetFullPath("./reports/" + filename)
                    let records = WorldOMeter.readCountry(c)
                    Csv.writeRecords path records
                    printfn "Country: %s - %s" c filename
                    ()
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

    printfn "Stats: %A" [ for o in worldoMeter.CountrySummary -> o ]

and saveStats() =
    Console.Clear()
    
    let worldoMeter = WorldOMeter.WorldOMeter()


    worldoMeter.CountrySummary
        |> CsvArchive.writeRecords "world-o-meter"
        |> ignore

