﻿open System

// Set the working directory to that of the source file.  Useful when running from within FSI launched from VS
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

#r @"lib\FSharp.Data.dll"
#r @"lib\RDotNet.dll"
#r @"lib\RProvider.dll"
#load @"YahooPriceLoader.fs"

open System.IO
open YahooPriceLoader
open RDotNet
open RProvider
open RProvider.``base``
open RProvider.graphics
open RProvider.stats
open RProvider.grDevices


// Utility function to plot a standard 
// line graph in R.  The defaults in this
// window are reasonable for the purposes of
// this tutorial.
let plotGraph domain range xlab ylab title = 
    /// Helper function to create a name
    /// value pair where the value has been 
    /// cast to an object
    let p name value = name, value :> obj // note the cast to object here ...
    let startNewRPlotWindow = R.dev_new() |> ignore  // Causes R to start a new plot window
    startNewRPlotWindow
    let result = R.plot(namedParams
                            [ 
                              p "x" domain; 
                              p "y" range; 
                              p "main" title;
                              p "xlab" xlab; 
                              p "ylab" ylab;
                              p "type" "l";   // always make a line graph
                              p "lwd" 2.5;    // line width
                              p "col" "blue"; // Make the trace blue.
                              p "lty" 1
                            ]
                        ) 
                        
    R.grid(nx = null, 
           ny = null, 
           col = "lightgray",
           lty = "dotted", 
           lwd = 1.5,
           equilogs = true) |> ignore
    result 

let startDate  = new DateTime(1993, 02, 20) 
let endDate    = new DateTime(2008, 03, 31) 
let tickerName = "SBUX"

type DateClosingtype = { Date: DateTime; ClosingPrice: float }

// Retrieve monthly sbux stock prices.
let sbux = (getYahooStockPrices startDate endDate Monthly tickerName).Data
           |> Seq.map (fun row -> { Date=row.Date; ClosingPrice=(float row.AdjClose)}) // R doesn't like decimals and has no real support for quad precision numbers
           |> Seq.filter (fun dp -> dp.Date >= startDate && dp.Date <= endDate)        // Filter for the date range
           |> Seq.sortBy (fun dp -> dp.Date)                                           // Order by date ascending.

let foundStartDate = (sbux |> Seq.head).Date
let foundEndDate   = (sbux |> Seq.last).Date

printf "Found data range - Start: %A, End: %A\r\n" foundStartDate foundEndDate
sbux |> Seq.iter (fun dp -> printf "Date: %A, Closing Price: %A\r\n" dp.Date dp.ClosingPrice)

let domain   = (sbux |> Seq.map (fun dp -> dp.Date)) 
let range    = (sbux |> Seq.map (fun dp -> dp.ClosingPrice))
let logRange = range |> Seq.map (fun i -> Math.Log(i))
 
let simpleMonthlyReturns = 
    range 
    |> Seq.pairwise 
    |> Seq.map (fun i -> let pt   = snd i
                         let ptm1 = fst i
                         (pt/ptm1)-1.0)

let overlappingAnnualReturns = 
    range 
    |> Seq.windowed 13 // To get to the same month in the next year.
    |> Seq.map (fun i -> let pt   = i.[12]
                         let ptm1 = i.[0]
                         (pt/ptm1)-1.0)
  
let ccMonthlyReturns = 
    logRange
    |> Seq.pairwise
    |> Seq.map (fun i -> let pt   = snd i
                         let ptm1 = fst i
                         pt-ptm1)

let overlappingCCAnnualReturns = 
    logRange 
    |> Seq.windowed 13 // To get to the same month in the next year.
    |> Seq.map (fun i -> let rt   = i.[12]
                         let rtm1 = i.[0]
                         rt-rtm1)
   
plotGraph domain                  range                      "Date" "Adjusted Closing Price"     "SBUX Adjusted Closing Prices"
plotGraph domain                  logRange                   "Date" "Log Adjusted Closing Price" "SBUX Log Adjusted Closing Prices"
plotGraph (domain |> Seq.skip 1)  simpleMonthlyReturns       "Date" "RM"                         "Simple Monthly Returns" 
plotGraph (domain |> Seq.skip 12) overlappingAnnualReturns   "Date" "RA"                         "Overlapping Annual Returns" 
plotGraph (domain |> Seq.skip 1)  ccMonthlyReturns           "Date" "rM"                         "CC Monthly Returns" 
plotGraph (domain |> Seq.skip 12) overlappingCCAnnualReturns "Date" "rA"                         "Overlapping CC Annual Returns" 