# Ocad.Gnss

### Overview

Ocad.Gnss is a cross-platform .NET/MAUI module that exposes low-level GNSS data and provides 
a processing layer for filtering and quality monitoring to give OCAD app more insight about 
their current GNSS location than the standard mobile location APIs provides.


The standard mobile location APIs abstract away most GNSS detail, exposing only basic
position and accuracy. This module removes that abstraction, giving access
to satellite-level data, signal quality metrics, and raw measurements inline with
platform support.


### Using the module

The module is accessed through the IGnssManager interface. Each platform (Android and iOS) has its own GnssManager implementation behind that interface, but they are used the same. The included test app is a working reference for how to wire the module. MainActivity (in the Android platform folder) shows the full integration, and the steps below map directly to it.

(OPTIONAL) Create a config. GnssConfig holds the tunable settings (update rate, filter, quality thresholds). Create one with defaults, or adjust as needed:

`var config = new GnssConfig();`

<br>

Create the platform manager. On Android, this takes the system LocationManager and the config:

`var locationManager = (LocationManager)GetSystemService(LocationService);`
`IGnssManager manager = new Platforms.Android.GnssManager(locationManager, config);`

<br>

Subscribe to the relevant events. The manager exposes three main:

`manager.LocationReceived += location => { /* new position */ };`

`manager.SatelliteStatusChanged += satellites => { /* satellites in view */ };`

`manager.RawMeasurementsReceived += measurements => { /* raw GNSS data */ };`

and a prototype fourth: 

`manager.QualityChanged += quality => { /* signal / satellites / accuracy */ };` 

(On iOS, only LocationReceived and QualityChanged fire. Core Location provides no satellite or raw data.)

<br>

Start and stop the module to control its lifecycle:

`manager.Start();`  
`manager.Stop();`    

The app creates a manager, subscribes to events, and calls Start()/Stop(). The module handles the platforms, mapping, filtering, and quality eval internally.

Note: the test app uses a static GnssApp holder to share the config and manager between pages. This was a prototype shortcut for the example app.
See MainActivity for the complete working example, as well as projectDoc.md and technicalDoc.md for a full explanation of the module's design.