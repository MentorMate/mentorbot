using System;

namespace MentorBot.Functions
{
    /// <summary>Simular to System.Diagnostics.Contracts.Contract.</summary>
    public static class Contract
    {
        /// <summary>Ensureses the condition is valid.</summary>
        /// <exception cref="Exception">Throws exception when the condition is false.</exception>
        public static bool Ensures(bool condition, string message) =>
            condition ? condition : throw new Exception(message);
    }
}
