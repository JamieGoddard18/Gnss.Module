/* Default filter which does nothing - using when no filter is required. */

namespace Ocad.Gnss
{
    public class NoFilter : IGnssFilter 
    {
        // No filter - return unfiltered location
        public GnssLocation Process(GnssLocation raw)
        {
            return raw; 
        }

        public void Reset(){}
    }
}