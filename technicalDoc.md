# Ocad.Gnss - Technical Documentation

### Cross-platform 

### `IGnssManager`

The core interface of the module and the entry point where the app interacts with GNSS functionality. The app depends only on this interface and not on platform-specific code directly, so all Android or iOS detail stays abstracted behind it.

The interface defines the manager blueprint for all platform implementations: a `Start()` and `Stop()` method to control the GNSS lifecycle, an `IsRunning` property indicating whether the service is active, a `CurrentQuality` property which holds the latest evaluated signal quality for each metric, and events: `LocationReceived`, `QualityChanged`, `SatelliteStatusChanged`, and `RawMeasurementsReceived`, that the app subscribes to for updates.

### `GnssQualityEvaluator`

Static class that evaluates the overall signal quality from the current satellite and fix data. It takes the latest satellite list and location, applies thresholds for satellite count, average signal strength (C/N0), and horizontal accuracy, and returns a `GnssQuality` value for each. If no satellites are flagged as used in the fix (iOS and emulator) it falls back to the total satellites in view so evaluation still produces a sensible result. 

[Understanding GPS Signal Levels (onesdr.com)](https://www.onesdr.com/gps-signal-levels/)

GNSS chips determine position through trilateration (measuring the distance to each satellite and finding where those spheres intersect). Three satellites give a 2D fix (latitude and longitude), and a fourth is required for a 3D fix that also has altitude. Accuracy improves with more satellites, as extra measurements and wider spread reduce the impact of a single weak signal. The `GnssQualityEvaluator` therefore treats fewer than four usable satellites as an unreliable fix. The higher thresholds are set by judgement rather than any standard. Six or more is treated as good and eight or more as excellent - this can be tuned with real-device testing.

[How Many Satellites Get A Good Signal Lock? (whitelabeltracking.com)](https://www.whitelabeltracking.com/how-many-satellites-get-a-good-gps-lock-and-why/)

[GPS - Who, What, Where?(actisense.com)](https://actisense.com/news/gps/)

### `GnssQuality`

Enum representing quality of the metrics in the current fix: `Excellent`, `Good`, `Low`, `Lost` and `Unavailable`. It could give the app a simple indicator of positioning reliability without needing to expose raw satellite or signal data itself.

### `GnssLocation`

The shared cross-platform model to represent a position fix. It holds the guaranteed fields: latitude, longitude, horizontal accuracy, and timestamp, along with a set of optional fields that are not always available: altitude, speed, bearing, the vertical/speed/bearing accuracy values, and the provider that produced the fix.

[Apple `Core Location`](https://developer.apple.com/documentation/corelocation/cllocation)

[Android `Location`](https://developer.android.com/reference/android/location/Location)

### `GnssRawMeasurement`

Only available on Android, this is the shared cross-platform model representing raw measurement data for a single satellite. It holds the satellite's vehicle ID and constellation type, its signal strength (C/N0), the AGC (Automatic Gain Control) level used for interference and multipath diagnostics, the pseudorange rate (Doppler), and the carrier phase (Accumulated Delta Range) measurement along with a flag indicating whether that carrier phase value is valid. Raw measurements are hardware-dependent, so some fields may not be populated on all devices.

[Android `GnssMeasurement`](https://developer.android.com/reference/android/location/GnssMeasurement)

### `GnssSatelliteInfo`

Only available on Android, this is the shared cross-platform model representing a single satellite's status. It holds the satellite's vehicle ID, constellation type, signal strength (C/N0), elevation and azimuth angles, and whether the satellite was used in the position fix. A collection of these objects describes the full set of satellites currently visible to the device.

[Android `GnssStatus`](https://developer.android.com/reference/android/location/GnssStatus)


### `GnssConstellationType`

Enum to represent GNSS constellations a satellite can belong to: `Unknown`, `Gps`, `Sbas`, `Glonass`, `Qzss`, `Beidou`, `Galileo`, and `Irnss`. Provides a cross-platform set of constellation types that Android's native constellation values are mapped into by the listeners, so the module's interface doesn't depend on Android's own enum.

### `IGnssFilter` 

The interface for a GNSS location filter. Filters sit between raw location data and the app, taking a raw location and returning a processed version. It has two methods: `Process()`, which takes a raw GnssLocation and returns the filtered result, and `Reset()`, which clears any internal values.

### `NoFilter` 

An implementation of `IGnssInterface` which does nothing. Was using when no filtering was needed. 

### `SmoothingFilter` 

An implementation of `IGnssFilter` that applies exponential smoothing to reduce noise (Exponetial moving average EMA). Each new location is blended with the previous smoothed estimate using the formula smoothed = α × new + (1 − α) × previous, where the smoothing factor α (between 0 and 1) controls the balance between responsiveness and smoothness, a higher value tracks new readings closely, a lower value smooths more heavily at the cost of lag. The default is 0.3, giving moderate smoothing.

The first reading is passed through unchanged, as there is no previous estimate to blend against, and it seeds the filter. Only latitude and longitude are smoothed, all other fields are copied through from the raw reading unchanged.

[Approaches To Real Time Smoothing](https://medium.com/@dmitriy.bolotov/six-approaches-to-time-series-smoothing-cc3ea9d6b64f#07a8)

[Exponential Smoothing Formula](https://en.wikipedia.org/wiki/Exponential_smoothing)

### `GnssConfig` 

Shared class holding changeable settings for the GNSS module, to allow users or devs to adjust values. 

It currently has three types of settings. The `UpdateIntervalMs`, the filtering options, type of filter and alpha value (`Filter`, `SmoothingAlpha`) and quality thresholds (satellite counts, signal-strength and accuracy).

`GnssConfig` is passed to the specific platform `GnssManager` when it's created. `GnssManager` uses it to choose the filter and update rate and passes it to `GnssQualityEvaluator` for the quality thresholds to be configured. Settings are currentely changed from the settings page in the Android test MAUI build. 

`UpdateIntervalMs` sets the minimum time between location updates, although this is not a guaranteed rate. GNSS chips produce locations at a fixed rate, typically 1Hz (every 1000ms) on most user devices, with only some supporting higher rates. So requesting a shorter interval than the hardware's capability has no effect. Larger values do slow updates effectively (e.g. to save battery). Therefore very low values (e.g. 1ms) are effectively floored to the device's real update rate rather than honoured literally, so the setting is a "no more often than this" value rather than an exact rate.

[.NET `RequestLocationUpdates()`](https://learn.microsoft.com/en-us/dotnet/api/android.locations.locationmanager.requestlocationupdates?view=net-android-35.0)

### `GnssQualityStatus`

A shared model that holds the evaluated GNSS quality for each metric signal strength, satellite count, and accuracy so they can be passed around and reported as a single object.

### `GnssApp`

A holder for Gnss config and manager so test app can access both.

---

### Android-specific 

### `GnssManager`(Android)

Android-specific class that implements `IGnssManager`, acting as the single point for the app to access all GNSS functionality.

It wires together the three Android listeners: `GnssLocationListener`, `GnssStatusListener` and `GnssMeasurementListener`, maps their data into the shared models, evaluates signal quality, and sends everything back to the `IGnssManager` through events. It receives a `LocationManager` object through its constructor rather than creating one itself, keeping it separate from how the LocationManager is obtained.

When `Start()` is called the class creates the three listeners, subscribes to their events, and registers them with Android's `LocationManager` to begin receiving updates. When `Stop()` is called it unregisters each listener, clears them, and resets its state. Both of these methods protect against being called when the manager is already in the requested state.

When location and satellite data arrive, the manager stores latest values are stored, sends the data to the app via the events, and re-evaluates the signal quality. `QualityChanged` event fires each time quality is re-evaluated, on every location or satellite update.

### `GnssLocationListener`

Android-specific class that receives `Location` objects from Android's `LocationManager` and converts them into the module's shared `GnssLocation` model. It implements `ILocationListener`, the interface required by `LocationManager` to register for location updates.

When a new location is found the listener's `OnLocationChanged()` is called with the new `Location` passed. Guaranteed fields on a `Location` are mapped to a new `GnssLocation` and optional fields are checked to exists before being mapped as well.

The interface also requires methods for for provider enabled/disabled and status changes. These are not used as did not seem neccessary currently although could be useful in the future. 

[Android `LocationListener`](https://developer.android.com/reference/android/location/LocationListener)

[.NET `ILocationListener`](https://learn.microsoft.com/en-us/dotnet/api/android.locations.ilocationlistener?view=net-android-35.0)

[Android `LocationManager`](https://developer.android.com/reference/android/location/LocationManager)

[.NET `LocationManager`](https://learn.microsoft.com/en-us/dotnet/api/android.locations.locationmanager?view=net-android-35.0)

### `GnssStatusListener`

Android-specific class that receives satellite data from Android's `GnssStatus` API and converts it into a list of `GnssSatelliteInfo` objects. It extends `GnssStatus.Callback`, the class required by `LocationManager.RegisterGnssStatusCallback()` to register for satellite status updates. When Android calls `OnSatelliteStatusChanged()` with a new `GnssStatus` object, the class iterates over every satellite and maps each one since `GnssStatus` is a group of multiple satellites.

[Android `GnssStatus`](https://developer.android.com/reference/android/location/GnssStatus)

[.NET `GnssStatus`](https://learn.microsoft.com/en-us/dotnet/api/android.locations.gnssstatus?view=net-android-35.0)

### `GnssMeasurementListener`

Android-specific class that receives raw measurement data from Android's `GnssMeasurementsEvent` API and converts it into a list of `GnssRawMeasurement` objects. It extends `GnssMeasurementsEvent.Callback`, the class required by `LocationManager.RegisterGnssMeasurementsCallback()` to register for raw measurement updates. When a new set of measurements is available the listener's `OnGnssMeasurementsReceived()` is called with a `GnssMeasurementsEvent`. The class iterates over the list of measurements and maps each one into a `GnssRawMeasurement`, translating Android's constellation type into the shared `GnssConstellationType`. Raw measurements are hardware-dependent, fields such as AGC and carrier phase (ADR) may not be populated on all devices.

[Android `GnssMeasurementEvent`](https://developer.android.com/reference/android/location/GnssMeasurementsEvent)

[.NET `GnssMeasurementEvent`](https://learn.microsoft.com/en-us/dotnet/api/android.locations.gnssmeasurementsevent?view=net-android-35.0)

--- 

### IOS-specific 

### `GnssLocationListener` 

iOS-specific class that receives `CLLocation` objects from `Core Location` and converts them into the module's shared `GnssLocation` model. 

It extends `CLLocationManagerDelegate`, the class Core Location uses to deliver location callbacks to the app.

When new locations are available the `LocationsUpdated()` is called from `CLLocationManagerDelegate` with an array of CLLocation objects. The last entry is the most recent location. Guaranteed fields (latitude, longitude, horizontal accuracy) are mapped to a new `GnssLocation`, and optional fields (altitude, speed, bearing) are checked to be valid before being mapped. Unlike Android, which signals unavailable fields with HasX boolean checks, Core Location uses negative values to indicate an invalid or unavailable reading.

[Apple `CLLocationManagerDelegate`](https://developer.apple.com/documentation/corelocation/cllocationmanagerdelegate)

[.NET `CLLocationManagerDelegate`](https://learn.microsoft.com/en-us/dotnet/api/corelocation.cllocationmanagerdelegate?view=net-ios-26.4-10.0)

[Apple `CLLocation`](https://developer.apple.com/documentation/corelocation/cllocation)

[.NET `CLLocation`](https://learn.microsoft.com/en-us/dotnet/api/corelocation.cllocation?view=net-ios-26.4-10.0)

[Apple `CLLocationManager`](https://developer.apple.com/documentation/corelocation/cllocationmanager)

[.NET `CLLocationManager`](https://learn.microsoft.com/en-us/dotnet/api/corelocation.cllocationmanager)

### `GnssManager` (iOS) 

iOS-specific class which implements `IGnssManager`, acting as the single point where the app accesses iOS GNSS functionality.

It uses `CLLocationManager` and `GnssLocationListener` to receive location updates, evaluates signal quality, and sends everything back to the app through events. It creates its own `CLLocationManager` internally as opposed to receiving one, as Core Location is accessed directly rather than through a system service.

When `Start()` is called the class creates the location listener, assigns it as the `CLLocationManager` delegate, requests location permission, and begins location updates. As locations arrive from the listener, it forwards them to the app via the `LocationReceived` event and re-evaluates signal quality. When `Stop()`is called it stops updates, clears the delegate, and resets its state.

Because iOS Core Location exposes no satellite or raw measurement data, the `SatelliteStatusChanged` event is never fired and there is no satellite listener. Signal quality is therefore evaluated from accuracy alone, the Signal and Satellites metrics report `Unavailable`, since there is no data to assess. The iOS manager implementation populates location data only, while still exposing it through the same `IGnssManager` interface as Android.