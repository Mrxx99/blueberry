using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

namespace Blueberry.Desktop.WindowsApp.Bluetooth
{
    /// <summary>
    /// Wraps an makes use of the <see cref="BluetoothLEAdvertisementWatcher"/>
    /// for easier consumption.
    /// </summary>
    public class DnaBluetoothLEAdvertisementWatcher
    {
        #region Private Members

        /// <summary>
        /// The underlying bluetooth watcher class
        /// </summary>
        private readonly BluetoothLEAdvertisementWatcher mWatcher;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        private readonly Dictionary<ulong, DnaBluetoothLEDevice> mDiscoverdDevices = new Dictionary<ulong, DnaBluetoothLEDevice>();

        /// <summary>
        /// A thread lock object for this class
        /// </summary>
        private readonly object mThreadLock = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if this watcher is listening for advertisements
        /// </summary>
        public bool Listening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        public IReadOnlyCollection<DnaBluetoothLEDevice> DisocveredDevices
        {
            get
            {
                // Clean up any timeouts
                CleanupTimeouts();


                // ensure thread-safety
                lock (mThreadLock)
                {
                    // Convert to read-only list
                    return mDiscoverdDevices.Values.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// The timeout in seconds that a device is removed from the <see cref="DisocveredDevices"/>
        /// list if it is not re-advertised within this time.
        /// </summary>
        public int HeartbeatTimeout { get; set; } = 30;

        #endregion

        #region Public Events

        /// <summary>
        /// Fired when the bluetooth watcher stops listening
        /// </summary>
        public event Action StoppedListening = () => { };

        /// <summary>
        /// Fired when the bluetooth watcher starts listening
        /// </summary>
        public event Action StartedListening = () => { };

        /// <summary>
        /// Fired when a device is discovered
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceDiscoverd = (device) => { };

        /// <summary>
        /// Fired when a new device is discovered
        /// </summary>
        public event Action<DnaBluetoothLEDevice> NewDeviceDiscoverd = (device) => { };

        /// <summary>
        /// Fired when a device name changes
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceNameChanged = (device) => { };

        /// <summary>
        /// Fired when a device is removed for timing out
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceTimedOut = (device) => { };

        #endregion

        #region Constructor

        /// <summary>
        /// The default constructor
        /// </summary>
        public DnaBluetoothLEAdvertisementWatcher()
        {
            // Create bluetooth listener
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            // Listen out for new advertisement
            mWatcher.Received += WatcherAdvertismentRecieved;

            // LIsten out for when the watcher stops listening
            mWatcher.Stopped += (watcher, e) =>
            {
                // Inform listeners
                StoppedListening();
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Prune any timed out devices that we have not heard off
        /// </summary>
        private void CleanupTimeouts()
        {
            lock (mThreadLock)
            {
                // The date in time that if less than means a device has timed out
                var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                // Any devices that have not sent a new broadcast within the heartbeat time
                mDiscoverdDevices.Where(f => f.Value.BroadcastTime < threshold).ToList().ForEach(device =>
                {
                    // Remove device
                    mDiscoverdDevices.Remove(device.Key);

                    // Inform listeners
                    DeviceTimedOut(device.Value);
                });
            }
        }

        /// <summary>
        /// Listens out for watcher advertisements
        /// </summary>
        /// <param name="sender">The watcher</param>
        /// <param name="args">The arguments</param>
        private void WatcherAdvertismentRecieved(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Clean Timeouts
            CleanupTimeouts();

            DnaBluetoothLEDevice device = null;

            // Is new discovery?
            var newDiscovery = !mDiscoverdDevices.ContainsKey(args.BluetoothAddress);

            // Name changed?
            var nameChanged =
                // If it already exists
                !newDiscovery &&
                // And its not a blank name
                !string.IsNullOrEmpty(args.Advertisement.LocalName) &&
                // And the name is different
                mDiscoverdDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;

            lock (mThreadLock)
            {
                //Get the name of the device
                var name = args.Advertisement.LocalName;

                // If new name is blank, and we already have a device...
                if (string.IsNullOrEmpty(name) && !newDiscovery)
                    // Don't override what could be an actual name already
                    name = mDiscoverdDevices[args.BluetoothAddress].Name;

                // Create new devices info class
                device = new DnaBluetoothLEDevice
                (
                    // Bluetooth address
                    address: args.BluetoothAddress,
                    // Name
                    name: name,
                    // Broadcast Time
                    broadcastTime: args.Timestamp,
                    // Signal Strength
                    rssi: args.RawSignalStrengthInDBm
                );

                // Add/update the device in the dictionary
                mDiscoverdDevices[args.BluetoothAddress] = device;
            }

            // Inform listeners
            DeviceDiscoverd(device);

            // If name changed...
            if (nameChanged)
                DeviceNameChanged(device);

            // If new discovery...
            if (newDiscovery)
                // Inform listeners
                NewDeviceDiscoverd(device);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts listening for advertisements
        /// </summary>
        public void StartListening()
        {
            lock (mThreadLock)
            {
                // If already listening...
                if (Listening)
                    // Do nothing more
                    return;

                // Start the underlying watcher
                mWatcher.Start();

                // Inform listeners 
                StartedListening();
            }
        }

        /// <summary>
        /// Stops listening for advertisements
        /// </summary>
        public void StopListening()
        {
            lock (mThreadLock)
            {
                // If we are no currently listening...
                if (!Listening)
                    // Do nothing more
                    return;

                // Stop listening
                mWatcher.Stop();


                // Clear any devices
                mDiscoverdDevices.Clear();
            }
        }

        #endregion
    }
}
