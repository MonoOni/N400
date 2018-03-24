using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Globalization
{
    /// <summary>
    /// Contains utilities for translating system locale information into
    /// AS/400 locale information.
    /// </summary>
    public static class LocaleMapping
    {
        // TODO: actually implement mappings beyond "default English"

        /// <summary>
        /// Gets the AS/400 "national language variant" for a culture.
        /// </summary>
        /// <param name="culture">The culture to get the NLV for.</param>
        /// <returns>The NLV ID.</returns>
        public static int GetNLV(CultureInfo culture)
        {
            switch (culture.LCID)
            {
                default:
                    return 2924; // US EN EBCID
            }
        }

        /// <summary>
        /// Gets the AS/400 "coded character set identifier" for a culture.
        /// </summary>
        /// <param name="culture"></param>
        public static int GetCCSID(CultureInfo culture)
        {
            switch (culture.LCID)
            {
                default:
                    return 37;
            }
        }
    }
}
