# GPUPowerAdjuster - NOTE! Only works on NVIDIA GPUs for now
## Windows x64 for now - possibly portable to other platforms (if NVIDIA API works)
GPU power adjustment based on active processes (for example folding vs gaming)

## TLDR
Sets lower power limit when Folding@Home, sets higher when not. Works with "When idle" or "Paused" - when the FahCore_22 process is not running.


## Installation:
- Installs to given location, adds automatic startable service

## Function:
- Polls every 10 seconds the lists of processes to figure out the limit to set
- Sets the new limit if its different from the currently applied

## Configuration:
- Open Control Panel => Programs => GPU Power Adjuster Config
- ... or just edit in installdir (default "C:\Program Files\ProtonIT\GPU Power Adjuster") appsettings.json
- **Restart service after modifications**

"ControlLimits" part allows modifying the default limit and certain process specific ones
`
      {
        "Name": "Folding@Home",
        "ProcName": "FahCore_22",
        "Limit": 72
      },
`

The software goes the options in order, first one that matches partial ProcName (ProcessName containing the ProcName part) sets the Power Limit to the given percentage.

If no processes match, the last one with just "Limit" is applied.

Current default values are 72 for Folding@Home and 100 for default. RTX 2070 seems to have minimum valid value at 71, so just to be safe the minimum is set to 72.

## Troubleshooting:

If there is an error, the service just stops (usually right after it tries to start). To troubleshoot just run the "GPUPowerAdjusterSvc.exe" on command line to see the actual error.

