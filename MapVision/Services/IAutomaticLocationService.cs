using System;
using System.Threading.Tasks;

using Windows.Devices.Geolocation;

namespace MapVision.Services
{
    public interface IAutomaticLocationService
    {
        Geopoint CurrentPosition { get; }

        event EventHandler<Geopoint> PositionChanged;

        Task<bool> InitializeAsync();

        Task StartListeningAsync();

        void StopListening();
    }
}
