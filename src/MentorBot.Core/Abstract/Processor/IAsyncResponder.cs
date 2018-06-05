// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

namespace MentorBot.Core.Abstract.Processor
{
    /// <summary>A responder that sends asyncronus messages.</summary>
    public interface IAsyncResponder
    {
        /// <summary>Sends async message back to the caht room or person.</summary>
        /// <param name="message">The message.</param>
        /// <param name="path">The path to room or thread.</param>
        Task SendMessageAsync(string message, string path);
    }
}
