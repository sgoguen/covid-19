namespace CoronaFetch

module WorldOMeter =
    open System
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
        TotalCases: int
        TotalDeaths: int
        TotalRecovered: int
        ActiveCases: int
        NewCases: int
        NewDeaths: int
        SeriousAndCritical: int
        LastUpdated: DateTimeOffset
    }

    let byDate c = c.LastUpdated.Date
    let lastByDate = 
        List.groupBy byDate
        >> List.sortBy fst
        >> List.map (snd >> List.sortBy byDate >> List.head)

    let load(filename:string): CountryStatInfo list = 
        let stats = CountryStatFile.Load(filename)
        let readInt(f) = try f() |> max 0 with _ -> 0
        let (?|) (n:Nullable<_>) v = if n.HasValue then n.Value else v
        [ for r in stats.Rows do 
            {
                Country = r.Country
                LastUpdated = r.LastUpdated
                TotalCases = readInt(fun _ -> r.TotalCases)
                TotalDeaths = readInt(fun _ -> r.TotalDeaths)
                TotalRecovered = readInt(fun _ -> r.TotalRecovered)
                ActiveCases = readInt(fun _ -> r.ActiveCases)
                NewCases = readInt(fun _ -> r.NewCases)
                NewDeaths = readInt(fun _ -> r.NewDeaths ?| 0)
                SeriousAndCritical = readInt(fun _ -> r.SeriousAndCritical)
            }
        ]

    let readData() = 
        let directory = System.IO.Path.GetFullPath("./data/world-o-meter/")
        let files = System.IO.Directory.EnumerateFiles(directory)
        [ for f in files do 
            for r in load(f) do
                r 
        ]

    let allCountries = lazy Set.ofList [ for r in readData() -> r.Country ]

    type DailyStat(r:CountryStatInfo, prevDay:CountryStatInfo option) = 
        let hasPrevDay = prevDay.IsSome
        let rateOf(f: CountryStatInfo -> int): float = 
            match prevDay with
            | Some(p) -> let n = double(f(r))
                         let d = double(f(p))
                         if n = 0.0 || d = 0.0 then 0.0
                         else Math.Round((n / d), 4)
            | None -> 0.0
        member this.TotalCases = r.TotalCases
        member this.CaseGrowthRate = rateOf(fun r -> r.TotalCases)
        member this.TotalDeaths = r.TotalDeaths
        member this.DeathGrowthRate = rateOf(fun r -> r.TotalDeaths)
        member this.ActiveCases = r.ActiveCases
        member this.ActiveCasesChange = rateOf(fun r -> r.ActiveCases)
        member this.NewCases = r.NewCases
        member this.NewDeaths = r.NewDeaths
        member this.SeriousAndCritical = r.SeriousAndCritical
        member this.SeriousAndCriticalChange = rateOf(fun r -> r.SeriousAndCritical)
        member this.LastUpdated = r.LastUpdated

    let readCountry(country: string) = 
        let records = readData()
                        |> List.filter (fun r -> r.Country = country)
                        |> lastByDate

        let mutable previousDay = None
        [ for r in records do 
            DailyStat(r, previousDay)
            previousDay <- Some(r)
        ]