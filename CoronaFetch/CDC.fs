namespace CoronaFetch

module CDC =
    open System
    open FSharp.Data

    [<Literal>]
    let WeeklyFluReportUrl =  "https://www.cdc.gov/flu/weekly/weeklyarchives2019-2020/data/whoAllregt_cl09.html"

    // type FluReport = HtmlProvider<WeeklyFluReportUrl>

    // let getData() = 
    //     let tables = FluReport.Load(WeeklyFluReportUrl).Tables
    //     //let rows = tables.`` Influenza Positive Tests Reported to CDC by U.S. Clinical Laboratories 2019-2020 Season ``
    //     [ for r in rows -> r ]
// 