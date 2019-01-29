using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GeolocatorSample.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(GetLocationProvider))]

namespace GeolocatorSample.Droid
{
    public class GetLocationProvider : IGetLocationProvider
    {
        public string GetProvider()
        {
            string returnString = "";

            LocationManager manager = (LocationManager)MainActivity.Context.GetSystemService(Android.Content.Context.LocationService);
            bool gpsProviderEnabled = manager.IsProviderEnabled(LocationManager.GpsProvider);
            bool networkProviderEnabled = manager.IsProviderEnabled(LocationManager.NetworkProvider);
            bool passiveProviderEnabled = manager.IsProviderEnabled(LocationManager.PassiveProvider);


            // var networkProvider = manager
            returnString += gpsProviderEnabled ? "GPS," : ",";
            returnString += networkProviderEnabled ? "Network," : ",";
            returnString += passiveProviderEnabled ? "Passive" : "";

            return returnString;
        }
    }
}
