using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KMaps
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MessageDialog messageDialog;
        public BasicGeoposition MyPosition;
        public BasicGeoposition FinalPosition;
        public String PushPinWPCN = "ms-appx:///Icons/pushpinWPCN.png";
        public String PushPinGeneric = "ms-appx:///Icons/pushpin.png";
        public string FindOrRoute = "Find";
        public Image MyPushPin;

        readonly GeofenceMonitor _monitor = GeofenceMonitor.Current;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            //Microsoft Office
            GeofenceMark(47.6397, -122.1289);
            ForceEnabledLocation();
            GetCurrentCoordinate();
        }

        public void ForceEnabledLocation()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["LocationConsent"];
            if (value == null)
            {
                localSettings.Values["LocationConsent"] = true;
            }
        }

      
private async Task Init_Geofence(double lati, double longi)
{
    var geofenceMonitor = GeofenceMonitor.Current;
    var loc = await new Geolocator().GetGeopositionAsync(
        TimeSpan.FromMinutes(2),
        TimeSpan.FromSeconds(5));

    geofenceMonitor.Geofences.Clear();

    geofenceMonitor.GeofenceStateChanged += (sender, args) =>
    {
        var geoReports = geofenceMonitor.ReadReports();
        foreach (var geofenceStateChangeReport in geoReports)
        {
            
                var id = geofenceStateChangeReport.Geofence.Id;
                var newState = geofenceStateChangeReport.NewState;

                switch (newState)
                {
                    case GeofenceState.Entered:
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                      new MessageDialog("Kevin Ricardo Sejin this will be your favorite place :)", "Welcome")
                      .ShowAsync());
                        break;
                    case GeofenceState.Exited:
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                      new MessageDialog("Please come back soon", "GoodBye")
                      .ShowAsync());
                        break;
                    case GeofenceState.None:
                        break;
                    case GeofenceState.Removed:
                        break;
                    default:
                        break;
                }
            }
        
    };
 
    var jayway = new Geopoint(new BasicGeoposition()
    {
        Latitude = lati, Longitude = longi
    });
    var geofence = new Geofence("has llegado", new Geocircle(jayway.Position, 400),
        MonitoredGeofenceStates.Entered | MonitoredGeofenceStates.Exited,
        false, TimeSpan.FromSeconds(10));
    geofenceMonitor.Geofences.Add(geofence);
}

        public void GeofenceMark(double lati, double longi)
        {
            _monitor.GeofenceStateChanged += _monitor_GeofenceStateChanged;
            _monitor.Geofences.Clear();
            //Ruta N, Medellin building
            BasicGeoposition pos = new BasicGeoposition { Latitude = lati, Longitude = longi };
            Geofence fence = new Geofence("Building 9", new Geocircle(pos, 100));

            try
            {
                _monitor.Geofences.Add(fence);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

         void _monitor_GeofenceStateChanged(GeofenceMonitor sender, object args)
        {
            
            var fences = sender.ReadReports();
            foreach (var report in fences)
            {
                if (report.Geofence.Id != "Building 9")
                    continue;

                switch (report.NewState)
                {
                    case GeofenceState.Entered:
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            MessageDialog dialog = new MessageDialog("Welcome to Building 9, Microsoft Offices");
                        await dialog.ShowAsync();
                        });
                        break;
                    case GeofenceState.Exited:
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            MessageDialog dialog = new MessageDialog("Leaving Building 9, Microsoft Offices");

                            await dialog.ShowAsync();
                        });
                        break;
                    case GeofenceState.None:
                        break;
                    case GeofenceState.Removed:
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            searchtext.Visibility = Visibility.Collapsed;
        }

        public void EnableTextSearch()
        {
            if (searchtext.Visibility == Visibility.Collapsed)
            {
                searchtext.SelectAll();
                searchtext.Visibility = Visibility.Visible;
                searchtext.Focus(FocusState.Keyboard);
            }
            else
            {
                searchtext.Visibility = Visibility.Collapsed;
            }
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            FindOrRoute = "Find";
            EnableTextSearch();
        }

        private void route_Click(object sender, RoutedEventArgs e)
        {
            FindOrRoute = "Route";
            EnableTextSearch();
        }

        private void LocateMe_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentCoordinate();
        }


        private void roadmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MyMap.Style = MapStyle.Road;
        }

        private void aerialmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MyMap.Style = MapStyle.Aerial;
        }

        private void lightmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MyMap.ColorScheme = MapColorScheme.Light;
        }

        private void darkmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MyMap.ColorScheme = MapColorScheme.Dark;
        }

        private async void GetCurrentCoordinate()
        {
            PanelLoad.Visibility = Visibility.Visible;
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;

            try
            {
                Geoposition currentPosition = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
                MapControl.SetLocation(DrawMarker(PushPinWPCN), currentPosition.Coordinate.Point);
                MapControl.SetNormalizedAnchorPoint(DrawMarker(PushPinWPCN), new Point(0.5, 0.5));
                MyPosition.Latitude = currentPosition.Coordinate.Latitude;
                MyPosition.Longitude = currentPosition.Coordinate.Longitude;
                PanelLoad.Visibility = Visibility.Collapsed;
                await MyMap.TrySetViewAsync(currentPosition.Coordinate.Point, 15, 0, 0, MapAnimationKind.Bow);
                //await Init_Geofence(MyPosition.Latitude, MyPosition.Longitude);
            }
            catch (Exception ex)
            {
                MessageBox("Couldn't get current location, Your location might be Enabled in settings", "Alert");    
            }
           
        }

        public async void MessageBox(string content, string tittle)
        {
            messageDialog = new MessageDialog(content, tittle);
            await messageDialog.ShowAsync();
        }

        public Image DrawMarker(string PathIcon)
        {
            MyPushPin = new Image();
            MyPushPin.Source = new BitmapImage(new Uri(PathIcon));
            MyMap.Children.Add(MyPushPin);
            return MyPushPin;
        }

        private void Maps_Click(object sender, RoutedEventArgs e)
        {
            if (TypesMaps.Visibility == Visibility.Collapsed)
            {
                TypesMaps.Visibility = Visibility.Visible;
            }
            else
            {
                TypesMaps.Visibility = Visibility.Collapsed;
            }
        }

        private void DownloadMaps_Click(object sender, RoutedEventArgs e)
        {
            MapManager.ShowMapsUpdateUI();
        }

        private void Landmarks_Click(object sender, RoutedEventArgs e)
        {
            string landmarks = "Landmarks ";
            if (MyMap.LandmarksVisible)
            {
                MyMap.LandmarksVisible = false;
                Landmarks.Label = landmarks + "Off";
            }
            else
            {
                MyMap.LandmarksVisible = true;
                Landmarks.Label = landmarks + "On";
            }
        }

        private void Pedestrian_Click(object sender, RoutedEventArgs e)
        {
            string pedestrian = "Pedestrian ";
            if (MyMap.PedestrianFeaturesVisible)
            {
                MyMap.PedestrianFeaturesVisible = false;
                Pedestrian.Label = pedestrian + "Off";
            }
            else
            {
                MyMap.PedestrianFeaturesVisible = true;
                Pedestrian.Label = pedestrian + "On";
            }
        }

        private void MyMaps_Click(object sender, RoutedEventArgs e)
        {
            MapManager.ShowDownloadedMapsUI();
        }

        private void searchtext_LostFocus(object sender, RoutedEventArgs e)
        {
            searchtext.Visibility = Visibility.Collapsed;
        }

        private async void searchtext_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter )
            {
                if (searchtext.Text.Length > 0)   
                {
                        searchtext.Visibility = Visibility.Collapsed;
                        RouteDetails.Text = "";
                        //MyMap.Children.Clear();

                        // Start, Begin in My Current Position
                        Geopoint startPoint = new Geopoint(MyPosition);
                        MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(searchtext.Text, startPoint, 3);
                        if (result.Status == MapLocationFinderStatus.Success)
                        {

                            // End, Ended in My find position
                            FinalPosition.Latitude = result.Locations[0].Point.Position.Latitude;
                            FinalPosition.Longitude = result.Locations[0].Point.Position.Longitude;
                            Geopoint endPoint = new Geopoint(FinalPosition);
                            MapControl.SetLocation(DrawMarker(PushPinGeneric), endPoint);
                            MapControl.SetNormalizedAnchorPoint(DrawMarker(PushPinGeneric), new Point(0.5, 0.5));

                            if (FindOrRoute.Equals("Find"))
                            {
                                // Show Find                                
                                await MyMap.TrySetViewAsync(endPoint, 15, 0, 0, MapAnimationKind.Bow);
                            }
                            else
                            {

                                //Show Route
                                MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(
                                    startPoint,endPoint,MapRouteOptimization.Time,MapRouteRestrictions.None);

                                if (routeResult.Status == MapRouteFinderStatus.Success)
                                {
                                    // Display summary
                                    RouteDetails.Inlines.Add(new LineBreak());
                                    RouteDetails.Inlines.Add(new Run() 
                                    {
                                        Text = searchtext.Text.ToUpper()
                                    });
                                    RouteDetails.Inlines.Add(new LineBreak());


                                    string hours = "";
                                    if (routeResult.Route.EstimatedDuration.TotalHours > 1)
                                    {
                                        hours = routeResult.Route.EstimatedDuration.TotalHours.ToString() + " hrs,";
                                    } 

                                    RouteDetails.Inlines.Add(new Run()
                                    {
                                        Text = (routeResult.Route.LengthInMeters / 1000).ToString() + " km," +
                                               hours +
                                               routeResult.Route.EstimatedDuration.TotalMinutes.ToString() + " mins."
                                    });

                                    RouteDetails.Inlines.Add(new LineBreak());
                                    RouteDetails.Inlines.Add(new LineBreak());

                                    // Display the directions.
                                    RouteDetails.Inlines.Add(new Run()
                                    {
                                        Text = "DIRECTIONS"
                                    });
                                    RouteDetails.Inlines.Add(new LineBreak());
                                    foreach (MapRouteLeg leg in routeResult.Route.Legs)
                                    {
                                        foreach (MapRouteManeuver maneuver in leg.Maneuvers)
                                        {
                                            RouteDetails.Inlines.Add(new Run()
                                            {
                                                Text = maneuver.InstructionText
                                            });
                                            RouteDetails.Inlines.Add(new LineBreak());
                                        }
                                    }

                                    MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                                    viewOfRoute.RouteColor = Colors.Red;
                                    viewOfRoute.OutlineColor = Colors.Black;
                                    MyMap.Routes.Add(viewOfRoute);
                                    await MyMap.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.Linear);
                                }
                                else
                                {
                                    RouteDetails.Text = "A problem occurred: " + routeResult.Status.ToString();
                                }

                            }
                        }
                    
                }
                else
                {
                    MessageBox("Please insert your place to find", "Information");
                }
            }
        }

        private void terrainmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MyMap.Style = MapStyle.Terrain;
        }

        private void hybridmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MyMap.Style = MapStyle.AerialWithRoads;
        }

        private void Directions_Click(object sender, RoutedEventArgs e)
        {
            string Direction = "Directions ";
            if (PanelRoutes.Visibility == Visibility.Visible)
            {
                PanelRoutes.Visibility = Visibility.Collapsed;
                Directions.Label = Direction + "Off";
            }
            else
            {
                PanelRoutes.Visibility = Visibility.Visible;
                Directions.Label = Direction + "On";
            }
        }

        private void PitchSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (PitchSlider != null)
            {
                MyMap.DesiredPitch = PitchSlider.Value;
            }
        }

        private void PitchSlider2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (PitchSlider2 != null)
            {
                MyMap.Heading = PitchSlider2.Value;
            }
        }

    }
}
