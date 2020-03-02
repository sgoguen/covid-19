﻿// Learn more about F# at http://fsharp.org

open System

module Csv = 
    open CsvHelper
    open System.IO
    open System.Globalization

    let writeRecords (filename: string) (records: 'a list) = 
        use writer = new StreamWriter(filename)
        use csv = new CsvWriter(writer, CultureInfo.InvariantCulture)
        csv.WriteRecords(records)

module CsvArchive =
    open System.IO
    let writeRecords (archiveName: string) (records: 'a list) = 
        let date = DateTime.UtcNow.ToString("yyyy-MM-dd--HH")
        let path = sprintf "./data/%s" archiveName |> Path.GetFullPath
        ignore(Directory.CreateDirectory(path))
        let filename = sprintf "%s/%s.csv" path date
        printfn "File: %s" filename
        printfn "Path: %s" path
        Csv.writeRecords filename records

module WorldOMeter =
    open FSharp.Data
    type CoronaPage = HtmlProvider<"https://www.worldometers.info/coronavirus/">

    type CountryStatCsv = {
        Country: string
        TotalCases: Nullable<int>
        TotalDeaths: Nullable<int>
        TotalRecovered: Nullable<int>
        ActiveCases: Nullable<int>
        NewCases: Nullable<int>
        NewDeaths: Nullable<int>
        SeriousAndCritical: Nullable<int>
        LastUpdated: string
    }

    let inline toInt x = try Nullable<int>(int(x)) with | _ -> Nullable()

    let getSnapshot() = 
        let dateTime = DateTime.UtcNow
        [ for r in CoronaPage().Tables.Main_table_countries.Rows ->
            { 
                Country = r.``Country, Other``
                TotalCases = r.``Total Cases`` |> toInt
                TotalDeaths = r.``Total Deaths`` |> toInt
                TotalRecovered = r.``Total Recovered`` |> toInt
                ActiveCases = r.``Active Cases`` |> toInt
                NewCases = r.``New Cases`` |> toInt
                NewDeaths = r.``New Deaths`` |> toInt
                SeriousAndCritical = r.``Serious, Critical`` |> toInt
                LastUpdated = dateTime.ToString("o")
            } 
        ]



[<EntryPoint>]
let main argv =
    Console.Clear()
    // let filename = File.getFilename "world-o-meter" (DateTime.Now)
    
    WorldOMeter.getSnapshot()
        |> CsvArchive.writeRecords "world-o-meter"
        |> ignore

    // let csvTest = [
    //     {| X = 5; Name = "Steve" |}
    //     {| X = 8; Name = "Jim" |}
    // ]
    // // printfn "Writing to %s" filename
    // ignore(CsvArchive.writeRecords "test" csvTest)

    0