using System;
using System.Threading.Tasks;

using Windows.Devices.Geolocation;

namespace MapVision.Services
{
    public class MapWalkerLocationService : IAutomaticLocationService
    {
        private Geopoint _currentPosition;
        public Geopoint CurrentPosition => _currentPosition;

        public event EventHandler<Geopoint> PositionChanged;

        private bool _isListening;

        public async Task<bool> InitializeAsync()
        {
            var position = new BasicGeoposition
            {
                Latitude = 52.319859646768876,
                Longitude = 5.3346147994552311
            };
            _currentPosition = new Geopoint(position);

            _isListening = false;

            return true;
        }

        public async Task StartListeningAsync()
        {
            _isListening = true;
            while (_isListening)
            {
                var longitude = _currentPosition.Position.Longitude;
                var latitude = _currentPosition.Position.Latitude;
                longitude += 0.0025;

                _currentPosition = new Geopoint(new BasicGeoposition
                {
                    Latitude = latitude,
                    Longitude = longitude
                });
                PositionChanged?.Invoke(this, _currentPosition);

                await Task.Delay(1000);
            }
        }

        public void StopListening()
        {
            _isListening = false;
        }
    }
}
