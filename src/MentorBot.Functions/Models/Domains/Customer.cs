// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>A project entity.</summary>
    public sealed class Customer : IEquatable<Customer>, IComparable<Customer>
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long OpenAirId { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Implements the operator ==.</summary>
        public static bool operator ==(Customer left, Customer right) => left.Equals(right);

        /// <summary>Implements the operator !=.</summary>
        public static bool operator !=(Customer left, Customer right) => !left.Equals(right);

        /// <summary>Implements the operator $lt;.</summary>
        public static bool operator <(Customer left, Customer right) => left.OpenAirId < right.OpenAirId;

        /// <summary>Implements the operator $lt;=.</summary>
        public static bool operator <=(Customer left, Customer right) => left.OpenAirId <= right.OpenAirId;

        /// <summary>Implements the operator $gt;.</summary>
        public static bool operator >(Customer left, Customer right) => left.OpenAirId > right.OpenAirId;

        /// <summary>Implements the operator $gt;=.</summary>
        public static bool operator >=(Customer left, Customer right) => left.OpenAirId >= right.OpenAirId;

        /// <inheritdoc/>
        public int CompareTo(Customer other) => OpenAirId.CompareTo(other.OpenAirId);

        /// <inheritdoc/>
        public bool Equals(Customer other) => OpenAirId.Equals(other.OpenAirId);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Customer customer && Equals(customer);

        /// <inheritdoc/>
        public override int GetHashCode() => OpenAirId.GetHashCode();
    }
}
