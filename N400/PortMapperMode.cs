using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400
{
    /// <summary>
    /// Represents the behaviours that can be taken when finding a service's
    /// port.
    /// </summary>
    public enum PortMapperMode
    {
        /// <summary>
        /// If the port mapper service and cache should always be used.
        /// </summary>
        AlwaysUsePortMapper,
        /// <summary>
        /// If the port mapper service and cache should be used, but if
        /// unavailable, is allowed to fall back to asumptions.
        /// </summary>
        FallbackToAssumed,
        /// <summary>
        /// If the port mapper is not to be used and instead rely on assumed
        /// ports.
        /// </summary>
        AlwaysAssumed
    }
}
