module YahooPriceLoader

open System
open System.IO
open System.Net
open FSharp.Data

// Provides sample data which the type provider will use
// to infer types for the Yahoo stocks csv.
type Stocks = CsvProvider< @"SampleFiles\SampleStocks.csv">

/// Yahoo provides the following options for data frequency
type yahooStockFrequency = 
    | Daily 
    | Weekly 
    | Monthly
    | DividendsOnly

/// Gets a sequence of stock from Yahoo between the given 
/// start and end dates, for the given ticker and with the 
/// given frequency.
let getYahooStockPrices (startDate:DateTime) (endDate:DateTime) frequency ticker = 
    let buildUrl (startDate:DateTime) (endDate:DateTime) frequency ticker = 
        let freq = match frequency with 
                    | Daily -> "d" 
                    | Weekly -> "w" 
                    | Monthly -> "m"         
                    | DividendsOnly -> "v"
        let baseUrl = "http://ichart.finance.yahoo.com/table.csv?s"
        sprintf "%s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i&g=%s&ignore=.csv" 
                 baseUrl 
                 ticker 
                 startDate.Month             
                 startDate.Day  
                 startDate.Year  
                 endDate.Month  
                 endDate.Day
                 endDate.Year  
                 freq
    let csvData = buildUrl startDate endDate Monthly ticker |> (new WebClient()).DownloadString
    Stocks.Parse(csvData)
