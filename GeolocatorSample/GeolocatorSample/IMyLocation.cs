using System;
using System.Collections.Generic;
using System.Text;

namespace GeolocatorSample
{
    public interface IMyLocation
    {
        void ObtainMyLocation();
        event EventHandler<ILocationEventArgs> locationObtained;
        bool IsGPSProviderAvailable();

        double GetLatNetwork();
        double GetlngNetwork();


        double GetLatGPS();
        double GetlngGPS();
    }
    public interface ILocationEventArgs
    {
        double lat { get; set; }
        double lng { get; set; }
    }
}
