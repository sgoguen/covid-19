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
                TotalCases = Nullable(r.TotalCases |> max 0)
                TotalDeaths = Nullable(r.TotalDeaths |> max 0)
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

    type DailyStat(r:CountryStatInfo, prevDay:CountryStatInfo option) = 
        let hasPrevDay = prevDay.IsSome
        let rateOf(f: CountryStatInfo -> Nullable<int>): float = 
            match prevDay with
            | Some(p) -> let n = double(f(r).Value)
                         let d = double(f(p).Value)
                         if n = 0.0 || d = 0.0 then 0.0
                         else Math.Round((n / d), 4)
            | None -> 0.0
        member this.TotalCases = r.TotalCases
        member this.CaseGrowthRate = rateOf(fun r -> r.TotalCases)
        member this.TotalDeaths = r.TotalDeaths
        member this.DeathGrowthRate = rateOf(fun r -> r.TotalDeaths)
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