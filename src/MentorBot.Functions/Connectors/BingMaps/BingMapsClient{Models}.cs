using System.Collections.Generic;

namespace MentorBot.Functions.Connectors.BingMaps
{
    /// <summary>The model needed for Bing Maps client to use.</summary>
    public sealed partial class BingMapsClient
    {
        /// <summary>The query responses.</summary>
        public sealed class QueryResponse
        {
            /// <summary>Gets or sets the resource sets.</summary>
            public IEnumerable<ResourceSet> ResourceSets { get; set; }
        }

        /// <summary>The query resource set.</summary>
        public sealed class ResourceSet
        {
            /// <summary>Gets or sets the resources.</summary>
            public IEnumerable<Resource> Resources { get; set; }
        }

        /// <summary>The query result resource.</summary>
        public sealed class Resource
        {
            /// <summary>Gets or sets the time zone at location.</summary>
            public IEnumerable<TimeZoneAtLocation> TimeZoneAtLocation { get; set; }
        }

        /// <summary>The query result timezone info.</summary>
        public sealed class TimeZoneAtLocation
        {
            /// <summary>Gets or sets the name of the place.</summary>
            public string PlaceName { get; set; }

            /// <summary>Gets or sets the time zone.</summary>
            public IEnumerable<TimeZoneData> TimeZone { get; set; }
        }

        /// <summary>The query result timezone info data.</summary>
        public sealed class TimeZoneData
        {
            /// <summary>Gets or sets the name of the time zone.</summary>
            public string GenericName { get; set; }
        }
    }
}
