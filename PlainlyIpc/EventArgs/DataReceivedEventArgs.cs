using System.Diagnostics.CodeAnalysis;

namespace PlainlyIpc.EventArgs;

/// <summary>
/// Event class for received data.
/// </summary>
public sealed class DataReceivedEventArgs : System.EventArgs
{
    /// <summary>
    /// The received data.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "For performance reasons.")]
    public byte[] Data { get; }

    /// <summary>
    /// Creates a new event for received data.
    /// </summary>
    /// <param name="data">The received data.</param>
    public DataReceivedEventArgs(byte[] data)
    {
        Data = data;
    }

}
