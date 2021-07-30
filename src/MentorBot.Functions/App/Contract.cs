using System;
using System.Diagnostics.CodeAnalysis;

namespace MentorBot.Functions
{
    /// <summary>Similar to System.Diagnostics.Contracts.Contract.</summary>
    public static class Contract
    {
        /// <summary>Gets the current date time.</summary>
        [SuppressMessage("Usage", "MEN013:Use UTC time", Justification = "The azure function should be setup to the correct local.")]
        public static DateTime LocalDateTime =>
            DateTime.Now;

        /// <summary>Ensures the condition is valid.</summary>
        /// <exception cref="Exception">Throws exception when the condition is false.</exception>
        public static bool Ensures(bool condition, string message) =>
            condition ? condition : throw new Exception(message);
    }
}
