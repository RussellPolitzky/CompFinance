//#if INTERACTIVE
//#r @"lib\RDotNet.dll"
//#r @"lib\RProvider.dll"
//#endif

//module SimpleGraph

// Bring in R
//open RDotNet
//open RProvider
//open RProvider.``base``
//open RProvider.graphics
//open RProvider.stats
//open RProvider.grDevices

/// Utility function to plot a standard 
/// line graph in R.  The defaults in this
/// window are reasonable for the purposes of
/// this tutorial.
let plotGraph domain range xlab ylab title = 
    /// Helper function to create a name
    /// value pair where the value has been 
    /// cast to an object
    let p name value =
        name, value :> obj // note the cast to object here ...
    /// Causes R to start a new plot window
    let startNewRPlotWindow = R.dev_new() |> ignore
     
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
    R.grid(namedParams
            [ 
              p "nx" null; 
              p "ny" null; 
              p "col" "lightgray";
              p "lty" "dotted"; 
              p "lwd" 1.5;
              p "equilogs" true
            ]
        ) |> ignore
    result 
