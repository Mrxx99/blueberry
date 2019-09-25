namespace Blueberry.Desktop.WindowsApp.Bluetooth
{
    /// <summary>
    /// Details about a specific GATT service
    /// <seealso cref="https://www.bluetooth.com/specifications/gatt/services/"/>
    /// </summary>
    public class GattService
    {
        #region Public Properties

        /// <summary>
        /// The human-readable name for the service
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The uniform identifier that is unique to this service
        /// </summary>
        public string UniformTypeIdentifier { get; }

        /// <summary>
        /// The 16-bit assigned number for this service.
        /// The Bluetooth GATT Service UUID contains this.
        /// </summary>
        public ushort AssignedNumber { get; }

        /// <summary>
        /// The type of specification that this service is.
        /// <seealso cref="https://www.bluetooth.com/specifications/gatt/"/>
        /// </summary>
        public string ProfileSepcification { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GattService(string name, string uniformTypeIdentifier, ushort assignedNumber, string profileSepcification)
        {
            Name = name;
            UniformTypeIdentifier = uniformTypeIdentifier;
            AssignedNumber = assignedNumber;
            ProfileSepcification = profileSepcification;
        }

        #endregion
    }
}
