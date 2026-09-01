# Ocad.Gnss - Project Documentation

### Project Overview 

The aim of this project was to build a cross-platform .NET/MAUI module that exposes low-level GNSS (Global
Navigation Satellite System) data and provides a processing layer for quality monitoring
and diagnostics, giving the OCAD app more insight into the device's current
positioning than the standard mobile location APIs allow. 

The standard mobile location APIs are built for consumer use, and abstract away most GNSS detail, exposing only
basic position and accuracy data. For professional 
mapping, where positioning accuracy and reliability are vital to the quality of the map produced, 
this is limiting. The project was to build a module that removes that abstraction and gives access to more detailed
location, satellite and constellation, and raw measurement data. 
The project also aimed to add an evaluation and monitoring layer that assesses location and signal quality.

The module is built as a self-contained library with a single interface as
its entry point with platform-specific implementations abstracted. The module is intended as an initial prototype 
for the OCAD mobile app, providing a foundation where functionality can be added and built upon.

### Design 

The OCAD app will talk to `IGnssManager`, the main interface in which all GNSS data and events are exposed, 
so the app never interacts with platform-specific code directly.

Behind the interface, inline with MAUI design, each platform has its own implementation. Both Android and iOS
have their own `GnssManager` which wires together the platform specific location listeners, maps their data into
shared object models, evaluates signal quality, and delivers everything back to the app through events. The app can
then subscribe to these events and control the module through the `Start()` and `Stop()` cycle.

Because both platforms sit behind the same `IGnssManager`, the app uses them identically, though the amount of data
available is different. 

### Limitations

Android's location API gives access to far more data than iOS's. Android provides
satellite status, constellation data and raw measurements, while iOS only gives access to basic position and 
accuracy. As a result, Android users have access to better processing and quality metrics, while 
iOS users get a more limited API. Both platforms sit behind a single, 
unified interface, so the app doesn't need to handle Android and iOS separately.

For most of the project no Android test device was available, so development relied heavily on the Android emulator. Because the emulator
only provides synthetic GNSS data, the signal-quality evaluation and filtering logic could be built and structurally verified but not
properly validated against real-world signals until late in the project, when an Android device became available intermittently. As a result
real-device testing was limited. It was enough to confirm the module works 
sensibly but not enough to thoroughly tune the quality thresholds or the filter
across the range of environments relevant to orienteering. More consistent access to Android hardware would be the single biggest enabler for
validating and refining the module's quality and processing layers.

### Features 

In addition to designing a cross-platform module, several features were built within the API to demonstrate its capabilities and deliver on the project's secondary goals:

- Access to detailed live satellite and signal data - the API exposes all data that each of the platform API provides. A test app UI shows location, satellite, raw measurement, and quality data updating in real time, which was used for development.

- Configuration - some values can be adjusted at runtime rather than being hardcoded, including update rate, filter selection, and quality thresholds.

- Prototype signal quality monitoring - the module evaluates GNSS quality for satellite count, signal strength (C/N0), and accuracy, and reports when the quality level changes. This is a prototype which could be used to abstract some of the detailed GNSS data whilst still giving users quality information on the current fix. 

- Prototype filtering - a filter layer allows location data to be smoothed (e.g. exponential smoothing) or passed through unfiltered, with the option to add further filters in future.

### Research and findings 

Originally a single combined quality score was planned, but this proved difficult. There are few references online for how to weight GNSS metrics against one another, and limited real-world testing made it hard to calibrate. Finding the exact crossover point where two different metrics represent similar quality was a particular challenge, as was deciding how much to trust each metric in isolation, for example a low signal strength does not necessarily mean poor accuracy, as those signals can still produce a reliable position. Rather than force an arbitrary combined quality score, the evaluator was changed to assess each metric independently and against configurable thresholds, letting the app decide how to interpret them. This was simpler, more suited to a prototype, and still allows the app to have an abstracted view of how good the positional fix is.

The estimated horizontal accuracy of a location can drop due to a variety of reasons. The most common, and most relevant, is signal obstruction and reflection. Buildings, tree canopy, cliffs, and terrain can block satellites or cause their signals to bounce off surfaces before reaching the device (multipath), both of which throw off the position calculation. 

GNSS chips calculate this estimate using dilution of precision, along with other GNSS data such as signal strength, the number of satellites used, and the errors left over when the position is computed. Android's Location.getAccuracy() then returns the estimated horizontal accuracy radius at the 68th percentile confidence level. So if you draw a circle around the current location with a radius equal to the accuracy, there is a 68% probability that the true location is inside the circle. This means that ~32% of the time the actual location is further out than this radius, and this value can be greater depending on the manufacturer of the chip.

For professional mapping this isn't too much of a problem. If the estimated accuracy is 1 metre then users can reliably use the location; if the accuracy is 10m then they would be more cautious. For this reason it's best to let the user see their accuracy and make their own decision in the field. If they have high accuracy they can be confident their location is correct; if not, they can either wait for a stronger signal or, if they are confident in their position, carry on working.

Signal strength is another important metric. Measured in dBHz, the C/N0 (Carrier-to-Noise-density ratio) measures the strength of a satellite's signal relative to the background noise, essentially how clearly the receiver can hear each satellite. A higher value means a cleaner, stronger signal that the receiver can track reliably; a lower value means the signal is closer to the noise floor and harder to use.

It was found in field testing that signal strength could be particularly useful for mapping, as it tends to degrade before the position itself does. When moving into forest terrain, C/N0 starts dropping while the accuracy is often still holding. This makes it an early indicator as a falling average C/N0 can warn that conditions are worsening and accuracy is likely to follow, giving the user a chance to react while they are still in open ground rather than after the fix has already deteriorated under canopy. For this reason, showing signal strength alongside accuracy gives the user a fuller and more anticipatory picture of how much they can trust their current position.

Thresholds have defaults based on general GNSS rules of thumb. Signals of 40–45 dBHz represent an excellent, clear-sky fix (unlikely to reach without an external antenna), around 30 dBHz and above is generally reliable for mapping, 20–30 dBHz is usable but degraded, and below roughly 15–20 dBHz tracking becomes unreliable.

[Measuring GNSS Acuracy on Android Devices (medium.com)](https://barbeau.medium.com/measuring-gnss-accuracy-on-android-devices-6824492a1389)

[Android `getAccuracy()`](https://developer.android.com/reference/android/location/Location#getAccuracy)

### Summary 

This project delivered a cross-platform GNSS module that exposes low-level positioning data and layers signal-quality monitoring, configurable filtering, and runtime configuration on top, all behind a single interface. The Android implementation is functional and tested on real hardware, the iOS implementation works within Core Location's constraints behind the same interface.

The module is intended as a foundation, with its design, findings, and documented limitations providing a basis for the team to extend towards production use. The most valuable next steps would be to implement for Windows as well as build a more in-depth processing layer for filtering, signal processing, and quality evaluation, and to integrate external Bluetooth GNSS receivers. The processing layer could add more advanced filtering and noise reduction, while Bluetooth integration would allow external receivers to feed data through the same interface via NMEA parsing.

This project was a valuable opportunity to work with a new language and framework. I've gained experience with C# and .NET MAUI, cross-platform mobile development, and the GNSS hardware and the challenges of real-world testing.