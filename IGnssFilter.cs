/* Interface for all GNSS location filter.
Implementaions take a raw location and return a processed one. */

namespace Ocad.Gnss
{
    public interface IGnssFilter
    {
        // Process a raw location and return a processed location
        GnssLocation Process(GnssLocation raw); 

        // Reset filtering when a fix is lost or listening restarts
        void Reset();
    }
}