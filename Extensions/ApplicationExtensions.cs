using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace PixelLab.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="Application"/> class.
    /// </summary>
    public static class ApplicationExtensions
    {
        private static readonly string PhoneLightThemeVisibility = "PhoneLightThemeVisibility";

        private static bool _listeningToNavEvents;
        private static bool _isNavigating;

        /// <summary>
        /// Gets the current Windows Phone <see cref="Theme"/>.
        /// </summary>
        /// <param name="application">The application instance.</param>
        /// <returns>An instance of the <see cref="Theme"/> enumeration.</returns>
        public static Theme GetTheme(this Application application)
        {
            var visibility = (Visibility)Application.Current.Resources[PhoneLightThemeVisibility];
            return (visibility == Visibility.Visible) ? Theme.Light : Theme.Dark;
        }

        /// <summary>
        /// Navigates backwards.
        /// </summary>
        /// <param name="application">The application.</param>
        public static void GoBack(this Application application)
        {
            var frame = application.RootVisual as PhoneApplicationFrame;
            EnsureListeningToNaviation(frame);

            if (frame == null)
                return;
            else if (_isNavigating)
                return;
            else if (frame.CanGoBack)
                frame.GoBack();
        }

        /// <summary>
        /// Navigates forwards.
        /// </summary>
        /// <param name="application">The application.</param>
        public static void GoForward(this Application application)
        {
            var frame = application.RootVisual as PhoneApplicationFrame;
            EnsureListeningToNaviation(frame);

            if (frame == null)
                return;
            else if (_isNavigating)
                return;
            else if (frame.CanGoForward)
                frame.GoForward();
        }

        /// <summary>
        /// Navigates to the specified page.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="uri">The URI.</param>
        public static void Navigate(this Application application, Uri uri)
        {
            var frame = application.RootVisual as PhoneApplicationFrame;
            EnsureListeningToNaviation(frame);

            if (frame == null)
                return;
            if (uri == null)
                return;
            if (uri.OriginalString == frame.CurrentSource.OriginalString)
                return;
            if (_isNavigating)
            {
                return;
            }
            else
            {
                frame.Navigate(uri);
            }
        }

        /// <summary>
        /// Navigates to the specified page.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="uri">The URI.</param>
        public static void Navigate(this Application application, string uri)
        {
            Navigate(application, new Uri(uri, UriKind.Relative));
        }

        /// <summary>
        /// Navigates to the specified page and sets the DataContext.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="uri">The URI of the destination page.</param>
        /// <param name="context">The new DataContext.</param>
        public static void Navigate(this Application application, string uri, object context)
        {
            FrameworkElement root = Application.Current.RootVisual as FrameworkElement;
            root.DataContext = context;
            Navigate(application, new Uri(uri, UriKind.Relative));
        }

        /// <summary>
        /// Navigates to the specified page and sets the DataContext.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="uri">The URI of the destination page.</param>
        /// <param name="context">The new DataContext.</param>
        public static void Navigate(this Application application, Uri uri, object context)
        {
            FrameworkElement root = Application.Current.RootVisual as FrameworkElement;
            root.DataContext = context;
            Navigate(application, uri);
        }

        private static void EnsureListeningToNaviation(PhoneApplicationFrame frame)
        {
            if (!_listeningToNavEvents)
            {
                frame.Navigating += (sender, e) => _isNavigating = true;
                frame.Navigated += (sender, e) => _isNavigating = false;
                frame.NavigationStopped += (sender, e) => _isNavigating = false;
                frame.NavigationFailed += (sender, e) => _isNavigating = false;
                _listeningToNavEvents = true;
            }
        }
    }

    /// <summary>
    /// Enumeration for the Windows Phone themes.
    /// </summary>
    public enum Theme
    {
        /// <summary>
        /// The Windows Phone Light theme.
        /// </summary>
        Light,

        /// <summary>
        /// The Windows Phone Dark theme.
        /// </summary>
        Dark
    }
}
