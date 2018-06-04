using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MapVision.Helpers;
using MapVision.Services;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;

namespace MapVision.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        // TODO WTS: Set your preferred default zoom level
        private const double DefaultZoomLevel = 17.1968;

        private MapControl _mapControl;

        private readonly IAutomaticLocationService _locationService;

        // TODO WTS: Set your preferred default location if a geolock can't be found.
        private readonly BasicGeoposition _defaultPosition = new BasicGeoposition()
        {
            Latitude = 52.319859646768876,
            Longitude = 5.3346147994552311
        };

        private string _mapServiceToken;

        public string MapServiceToken
        {
            get { return _mapServiceToken; }
            set { SetProperty(ref _mapServiceToken, value); }
        }

        private double _zoomLevel;

        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set { SetProperty(ref _zoomLevel, value); }
        }

        private Geopoint _center;

        public Geopoint Center
        {
            get { return _center; }
            set { SetProperty(ref _center, value); }
        }

        private ObservableCollection<MapIcon> _mapIcons = new ObservableCollection<MapIcon>();

        public ObservableCollection<MapIcon> MapIcons
        {
            get { return _mapIcons; }
            set { SetProperty(ref _mapIcons, value); }
        }

        private TurbineNoTurbineV2Model ModelGen = new TurbineNoTurbineV2Model();

        public ICommand SnapCurrentWindowCommand => new DelegateCommand<object>(async (elementToRender) =>
        {
            var uiElement = (UIElement)elementToRender;
            var videoFrame = await uiElement.RenderToVideoFrameAsync();
            var croppedVideoFrame = await videoFrame.CropVideoFrameAsync(256, 256);
            await croppedVideoFrame.SaveToFileAsync();
        });

        public ICommand CheckCurrentWindowCommand => new DelegateCommand<object>(async (elementToRender) =>
        {
            var uiElement = (UIElement)elementToRender;
            TurbineNoTurbineV2ModelOutput modelOutput = await EvaluateMapControl(uiElement);

            var message = $"Turbine probability: {modelOutput.loss["turbine"]}\n"
                        + $"No turbine probability: {modelOutput.loss["no_turbine"]}";

            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        });

        public ICommand StartAutomaticCheckingCommand => new DelegateCommand<object>((mapControl) =>
       {
           _mapControl = mapControl as MapControl;

           if (_mapControl != null)
           {
               _locationService.StartListeningAsync();
           }
       });

        public ICommand StopAutomaticCheckingCommand => new DelegateCommand(() =>
       {
           _locationService.StopListening();
       });

        private async Task<TurbineNoTurbineV2ModelOutput> EvaluateMapControl(UIElement uiElement)
        {
            var videoFrame = await uiElement.RenderToVideoFrameAsync();
            var croppedVideoFrame = await videoFrame.CropVideoFrameAsync(256, 256);

            var modelInput = new TurbineNoTurbineV2ModelInput
            {
                data = croppedVideoFrame
            };
            var modelOutput = await ModelGen.EvaluateAsync(modelInput);
            return modelOutput;
        }

        public MapViewModel(IAutomaticLocationService locationServiceInstance)
        {
            _locationService = locationServiceInstance;

            Center = new Geopoint(_defaultPosition);
            ZoomLevel = DefaultZoomLevel;

            // TODO WTS: Set your map service token. If you don't have one, request from https://www.bingmapsportal.com/
            // MapServiceToken = string.Empty;
            
            LoadModel();
        }

        private async void LoadModel()
        {
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/TurbineNoTurbineV2.onnx"));
            ModelGen = await TurbineNoTurbineV2Model.CreateTurbineNoTurbineV2Model(modelFile);
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            if (_locationService != null)
            {
                _locationService.PositionChanged += LocationServicePositionChanged;

                await _locationService.InitializeAsync();

                Center = new Geopoint(_defaultPosition);
            }

            //AddMapIcon(Center, "Map_YourLocation");
        }

        private void AddMapIcon(Geopoint location, string title)
        {
            var mapIcon = new MapIcon()
            {
                Location = location,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                Title = title.GetLocalized(),
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/map.png")),
                ZIndex = 0
            };
            MapIcons.Add(mapIcon);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
            if (_locationService != null)
            {
                _locationService.PositionChanged -= LocationServicePositionChanged;
                _locationService.StopListening();
            }
        }

        private async void LocationServicePositionChanged(object sender, Geopoint geoposition)
        {
            if (geoposition != null)
            {
                Center = geoposition;

                if (_mapControl != null)
                {
                    var modelOutput = await EvaluateMapControl(_mapControl);
                    if (modelOutput.loss["turbine"] > 0.7)
                    {
                        AddMapIcon(geoposition, "Turbine");
                    }
                }
            }
        }

    }
}
