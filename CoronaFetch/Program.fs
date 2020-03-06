// Learn more about F# at http://fsharp.org

open System

module Csv = 
    open CsvHelper
    open System.IO
    open System.Globalization

    let writeRecords (filename: string) (records: 'a list) = 
        use writer = new StreamWriter(filename)
        use csv = new CsvWriter(writer, CultureInfo.InvariantCulture)
        csv.WriteRecords(records)

    let readRecords<'a> (filename: string) = 
        use reader = new StreamReader(filename)
        use csv = new CsvReader(reader, CultureInfo.InvariantCulture)
        csv.Configuration.HasHeaderRecord <- true
        //csv.Configuration.PrepareHeaderForMatch <- (fun header index -> header)
        csv.GetRecords<'a>()

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

    type WorldOMeter() = 
        let page = CoronaPage()
        let dateTime = DateTime.UtcNow
        member this.CountrySummary =         
            [ for r in page.Tables.Main_table_countries.Rows ->
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

    type CountryStatFile = CsvProvider<Sample="./data/world-o-meter/2020-03-01--21.csv">

    type CountryStatInfo = {
        Country: string
        TotalCases: Nullable<int>
        TotalDeaths: Nullable<int>
        TotalRecovered: Nullable<int>
        ActiveCases: Nullable<int>
        NewCases: Nullable<int>
        NewDeaths: Nullable<int>
        SeriousAndCritical: Nullable<int>
        LastUpdated: DateTimeOffset
    }

    let byDate c = c.LastUpdated.Date
    let lastByDate = 
        List.groupBy byDate
        >> List.sortBy fst
        >> List.map (snd >> List.sortBy byDate >> List.head)

    let load(filename:string): CountryStatInfo list = 
        let stats = CountryStatFile.Load(filename)
        [ for r in stats.Rows do 
            {
                Country = r.Country
                LastUpdated = r.LastUpdated
                TotalCases = Nullable(r.TotalCases)
                TotalDeaths = Nullable(r.TotalDeaths)
                TotalRecovered = Nullable(0)
                ActiveCases = Nullable(0)
                NewCases = Nullable(0)
                NewDeaths = Nullable(0)
                SeriousAndCritical = Nullable(0)
            }
        ]

    let readData() = 
        let directory = System.IO.Path.GetFullPath("./data/world-o-meter/")
        let files = System.IO.Directory.EnumerateFiles(directory)
        [ for f in files do 
            for r in load(f) do
                r 
        ]

[<EntryPoint>]
let rec main args = 
    args 
        |> List.ofArray 
        |> function 
            | ["fetch"] -> fetchStats()
            | _ -> readData()
    0

and readData() = 
    let records = WorldOMeter.readData()
                    |> List.groupBy(fun r -> r.Country)
                    |> List.map(fun (country, list) -> country, WorldOMeter.lastByDate list )
                    // |> List.filter (fun r -> r.Country = "USA")
                    // |> WorldOMeter.lastByDate

    printfn "Files: %A" records

and showHelp() = 
    printfn "%A" [
        "fetch - Fetches the latest data"
    ]

and fetchStats() =
    Console.Clear()
    
    let worldoMeter = WorldOMeter.WorldOMeter()


    worldoMeter.CountrySummary
        |> CsvArchive.writeRecords "world-o-meter"
        |> ignore

