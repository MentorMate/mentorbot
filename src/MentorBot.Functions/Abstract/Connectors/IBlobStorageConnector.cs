using System.IO;
using System.Threading.Tasks;

namespace MentorBot.Functions.Abstract.Connectors
{
    /// <summary>A cloud blob storage connector.</summary>
    public interface IBlobStorageConnector
    {
        /// <summary>Gets a value indicating whether this connector is connected.</summary>
        bool IsConnected { get; }

        /// <summary>Gets the file stream asynchronous.</summary>
        Task<Stream> GetFileStreamAsync(string path);
    }
}
