namespace TestContainers.Containers.Mounts
{
    /// <summary>
    /// Different access modes in bind mounting
    /// </summary>
    public class BindMode

    {
        /// <summary>
        /// ReadOnly access mode. AKA "ro"
        /// </summary>
        public static readonly BindMode ReadOnly = new BindMode("ro");

        /// <summary>
        /// ReadWrite access mode. AKA "rw"
        /// </summary>
        public static readonly BindMode ReadWrite = new BindMode("rw");

        /// <summary>
        /// String representation of the mode for docker client consumption
        /// </summary>
        public string Value { get; }

        private BindMode(string value)
        {
            Value = value;
        }
    }
}
