using System;

using MapVision.ViewModels;

using Windows.UI.Xaml.Controls;

namespace MapVision.Views
{
    public sealed partial class MapPage : Page
    {
        private MapViewModel ViewModel => DataContext as MapViewModel;

        public MapPage()
        {
            InitializeComponent();
        }
    }
}
