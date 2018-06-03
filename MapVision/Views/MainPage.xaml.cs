using System;

using MapVision.ViewModels;

using Windows.UI.Xaml.Controls;

namespace MapVision.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
