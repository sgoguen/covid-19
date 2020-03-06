namespace CoronaFetch

module Csv =

    open CsvHelper
    open System.IO
    open System.Globalization
    open System.Linq

    //  Writes records to a CSV file
    let writeRecords (filename: string) (records: 'a list) = 
        use writer = new StreamWriter(filename)
        use csv = new CsvWriter(writer, CultureInfo.InvariantCulture)
        csv.WriteRecords(records)

    //  Reads records from a CSV file
    let readRecords<'a> (filename: string) = 
        use reader = new StreamReader(filename)
        use csv = new CsvReader(reader, CultureInfo.InvariantCulture)
        csv.Configuration.HasHeaderRecord <- true
        //csv.Configuration.PrepareHeaderForMatch <- (fun header index -> header)
        csv.GetRecords<'a>().ToArray()

    let printRecords (records: 'a list) = 
        use writer = new StringWriter()
        use csv = new CsvWriter(writer, CultureInfo.InvariantCulture)
        csv.WriteRecords(records)        
        printfn "%s" (writer.ToString())

module CsvArchive =
    open System
    open System.IO
    let writeRecords (archiveName: string) (records: 'a list) = 
        let date = DateTime.UtcNow.ToString("yyyy-MM-dd--HH")
        let path = sprintf "./data/%s" archiveName |> Path.GetFullPath
        ignore(Directory.CreateDirectory(path))
        let filename = sprintf "%s/%s.csv" path date
        printfn "File: %s" filename
        printfn "Path: %s" path
        Csv.writeRecords filename records