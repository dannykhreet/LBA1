using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GeolocatorSample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomePage : TabbedPage
	{
		int count;
		bool tracking;
		Position savedPosition;
        Position GPSPosition;

        Position NetworkPosition;

        public ObservableCollection<Position> Positions { get; } = new ObservableCollection<Position>();
        //LBA 1
        IMyLocation loc;
        public HomePage()
		{
			InitializeComponent();

            NetworkPosition = new Position();
            GPSPosition = new Position();

            ListViewPositions.ItemsSource = Positions;


            LocationStatus.Text = " Available Providers :" + DependencyService.Get<IGetLocationProvider>().GetProvider();

            loc = DependencyService.Get<IMyLocation>();
            loc.locationObtained += (object sender,
                ILocationEventArgs e) =>
            {
                var lat = e.lat;
                var lng = e.lng;
                // LatNET.Text = lat.ToString();
                // LanNET.Text = lng.ToString();
            };
            //loc.ObtainMyLocation();
        }

		private async void ButtonCached_Clicked(object sender, EventArgs e)
		{
			try
			{
				var hasPermission = await Utils.CheckPermissions(Permission.Location);
				if (!hasPermission)
					return;

				ButtonCached.IsEnabled = false;

				var locator = CrossGeolocator.Current;
				locator.DesiredAccuracy = DesiredAccuracy.Value;
				LabelCached.Text = "Getting gps...";

				var position = await locator.GetLastKnownLocationAsync();

				if (position == null)
				{
					LabelCached.Text = "null cached location :(";
					return;
				}

				savedPosition = position;
				ButtonAddressForPosition.IsEnabled = true;
				LabelCached.Text = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
					position.Timestamp, position.Latitude, position.Longitude,
					position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);

			}
			catch (Exception ex)
			{
				await DisplayAlert("Uh oh", "Something went wrong, but don't worry we captured for analysis! Thanks.", "OK");
			}
			finally
			{
				ButtonCached.IsEnabled = true;
			}
		}

		private async void ButtonGetGPS_Clicked(object sender, EventArgs e)
		{
			try
			{
                GetGPSTime.Text = "";
                var hasPermission = await Utils.CheckPermissions(Permission.Location);
				if (!hasPermission)
					return;

				ButtonGetGPS.IsEnabled = false;

				var locator = CrossGeolocator.Current;
				locator.DesiredAccuracy = DesiredAccuracy.Value;
				labelGPS.Text = "Getting gps...";

                var watchGPS = System.Diagnostics.Stopwatch.StartNew();

                var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(Timeout.Value), null, IncludeHeading.IsToggled);
                watchGPS.Stop();
                if (position == null)
				{
					labelGPS.Text = "null gps :(";
					return;
				}
				savedPosition = position;
                GPSPosition = position;

                ButtonAddressForPosition.IsEnabled = true;
				labelGPS.Text = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7} \n Done",
					position.Timestamp, position.Latitude, position.Longitude,
					position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);
                GetGPSTime.Text ="Total Time : " + watchGPS.ElapsedMilliseconds.ToString() + " ms ";
            }
			catch (Exception ex)
			{
				await DisplayAlert("Uh oh", "Something went wrong, but don't worry we captured for analysis! Thanks.", "OK");
			}
			finally
			{
				ButtonGetGPS.IsEnabled = true;
			}
		}

		private async void ButtonAddressForPosition_Clicked(object sender, EventArgs e)
		{
			try
			{
				if (savedPosition == null)
					return;

				var hasPermission = await Utils.CheckPermissions(Permission.Location);
				if (!hasPermission)
					return;

				ButtonAddressForPosition.IsEnabled = false;

				var locator = CrossGeolocator.Current;

				var address = await locator.GetAddressesForPositionAsync(savedPosition, "RJHqIE53Onrqons5CNOx~FrDr3XhjDTyEXEjng-CRoA~Aj69MhNManYUKxo6QcwZ0wmXBtyva0zwuHB04rFYAPf7qqGJ5cHb03RCDw1jIW8l");
				if (address == null || address.Count() == 0)
				{
					LabelAddress.Text = "Unable to find address";
				}

				var a = address.FirstOrDefault();
				LabelAddress.Text = $"Address: Thoroughfare = {a.Thoroughfare}\nLocality = {a.Locality}\nCountryCode = {a.CountryCode}\nCountryName = {a.CountryName}\nPostalCode = {a.PostalCode}\nSubLocality = {a.SubLocality}\nSubThoroughfare = {a.SubThoroughfare}";

			}
			catch (Exception ex)
			{
				await DisplayAlert("Uh oh", "Something went wrong, but don't worry we captured for analysis! Thanks.", "OK");
			}
			finally
			{
				ButtonAddressForPosition.IsEnabled = true;
			}
		}

		private async void ButtonTrack_Clicked(object sender, EventArgs e)
		{
			try
			{
				var hasPermission = await Utils.CheckPermissions(Permission.Location);
				if (!hasPermission)
					return;

				if (tracking)
				{
					CrossGeolocator.Current.PositionChanged -= CrossGeolocator_Current_PositionChanged;
					CrossGeolocator.Current.PositionError -= CrossGeolocator_Current_PositionError;
				}
				else
				{
					CrossGeolocator.Current.PositionChanged += CrossGeolocator_Current_PositionChanged;
					CrossGeolocator.Current.PositionError += CrossGeolocator_Current_PositionError;
				}

				if (CrossGeolocator.Current.IsListening)
				{
					await CrossGeolocator.Current.StopListeningAsync();
					labelGPSTrack.Text = "Stopped tracking";
					ButtonTrack.Text = "Start Tracking";
					tracking = false;
					count = 0;
				}
				else
				{
					Positions.Clear();
					if (await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(TrackTimeout.Value), TrackDistance.Value,
						TrackIncludeHeading.IsToggled, new ListenerSettings
						{
							ActivityType = (ActivityType)ActivityTypePicker.SelectedIndex,
							AllowBackgroundUpdates = AllowBackgroundUpdates.IsToggled,
							DeferLocationUpdates = DeferUpdates.IsToggled,
							DeferralDistanceMeters = DeferalDistance.Value,
							DeferralTime = TimeSpan.FromSeconds(DeferalTIme.Value),
							ListenForSignificantChanges = ListenForSig.IsToggled,
							PauseLocationUpdatesAutomatically = PauseLocation.IsToggled
						}))
					{
						labelGPSTrack.Text = "Started tracking";
						ButtonTrack.Text = "Stop Tracking";
						tracking = true;
					}
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Uh oh", "Something went wrong, but don't worry we captured for analysis! Thanks.", "OK");
			}
		}
	



	void CrossGeolocator_Current_PositionError(object sender, PositionErrorEventArgs e)
	{

		labelGPSTrack.Text = "Location error: " + e.Error.ToString();
	}

	    void CrossGeolocator_Current_PositionChanged(object sender, PositionEventArgs e)
	    {

		    Device.BeginInvokeOnMainThread(() =>
		    {
			    var position = e.Position;
			    Positions.Add(position);
			    count++;
			    LabelCount.Text = $"{count} updates";
			    labelGPSTrack.Text = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
				    position.Timestamp, position.Latitude, position.Longitude,
				    position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);

		    });
	    }




        //private async void Get_loc_GPS_Clicked(object sender, EventArgs e)
        //{

        //    bool GPSProviderStatus = loc.IsGPSProviderAvailable();

        //    LocationStatus.Text = " Available Providers :" + DependencyService.Get<IGetLocationProvider>().GetProvider();
        //    // the code that you want to measure comes here
        //    try
        //    {
        //        var hasPermission = await Utils.CheckPermissions(Permission.Location);
        //        if (!hasPermission)
        //            return;

        //        if (GPSProviderStatus)
        //        {
        //            Get_loc_GPS.IsEnabled = false;

        //            var locator = CrossGeolocator.Current;
        //            GPSStatus.Text = "Getting gps...";
        //            //var watchGPS = System.Diagnostics.Stopwatch.StartNew();

        //            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(2), null, false);
        //           // watchGPS.Stop();
        //            if (position == null)
        //            {
        //                GPSStatus.Text = "null gps :(";
        //                return;
        //            }
        //            GPSStatus.Text = "Get Finished Gps";
        //            LatGPS.Text = position.Latitude.ToString();
        //            LanGPS.Text = position.Longitude.ToString();
                    
        //            //GetGPSTime.Text = watchGPS.ElapsedMilliseconds.ToString() + " ms ";
        //            GetGPSStatus.Text = "Done Correctly ";

        //        }
        //        else
        //        {
        //            GPSStatus.Text = "GPS Provider Not Available";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Uh oh", "Something went wrong, but don't worry we captured for analysis! Thanks.", "OK");
        //        GetGPSStatus.Text = "No GPS signal found ";
        //    }
        //    finally
        //    {
        //        Get_loc_GPS.IsEnabled = true;
        //    }

        //}

        private void Get_loc_NET_Clicked(object sender, EventArgs e)
        {
            try
            {
                LocationStatus.Text = " Available Providers :" + DependencyService.Get<IGetLocationProvider>().GetProvider();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // the code that you want to measure comes here

                loc.ObtainMyLocation();
                LatNET.Text = loc.GetLatNetwork().ToString();
                LanNET.Text = loc.GetlngNetwork().ToString();

                watch.Stop();
                NetworkPosition.Latitude = loc.GetLatNetwork();
                NetworkPosition.Longitude = loc.GetlngNetwork();
                GetNETTime.Text = watch.ElapsedMilliseconds.ToString() + " ms ";
                GetNETStatus.Text = "Status : Done Correctly";
            }
            catch (Exception ee)
            {
                GetNETStatus.Text = "Status : Network Provider not found";
                //throw ee;
            }
        }

        private void CalculateDistance_Clicked(object sender, EventArgs e)
        {
            try
            {
                Location LocationNetworkPosition = new Location(NetworkPosition.Latitude, NetworkPosition.Longitude);
                Location LocationGPSPosition = new Location(GPSPosition.Latitude, GPSPosition.Longitude);
                if (NetworkPosition.Latitude == 0)
                {
                    CalculateDistance.Text = "Status :  sorry i do not have network location";
                    return;
                }
                if (GPSPosition.Latitude == 0)
                {
                    CalculateDistance.Text = "Status :  sorry i do not have GPS location";
                    return;
                }
                double Kilometers = Location.CalculateDistance(LocationNetworkPosition, LocationGPSPosition, DistanceUnits.Kilometers);
                double meter = Kilometers * 1000;
                CalculateDistance.Text = " Last Calculate Distance : " + meter.ToString() + "  m";
            }
            catch (Exception ee)
            {
                CalculateDistance.Text = "Status :  not worked";
                //throw ee;
            }
        }

        
    }
}