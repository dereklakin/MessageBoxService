using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using PixelLab.Common.Extensions;

namespace PixelLab.Common
{
    public class MessageBoxService
    {
        /// <summary>
        /// Occurs when the message box is closed.
        /// </summary>
        public event EventHandler Closed;

        private static readonly string SwivelInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation BeginTime=""0:0:0"" Duration=""0"" 
                             Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.CenterOfRotationY)"" 
                             Storyboard.TargetName=""LayoutRoot""
                             To="".5""/>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"" Storyboard.TargetName=""MBSRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-30""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)""
                                           Storyboard.TargetName=""MBSRoot"">
                <DiscreteDoubleKeyFrame KeyTime=""0"" Value=""1"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        private static readonly string SwivelOutStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation BeginTime=""0:0:0"" Duration=""0"" 
                             Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.CenterOfRotationY)"" 
                             Storyboard.TargetName=""LayoutRoot""
                             To="".5""/>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"" Storyboard.TargetName=""MBSRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""45"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)""
                                           Storyboard.TargetName=""MBSRoot"">
                <DiscreteDoubleKeyFrame KeyTime=""0"" Value=""1"" />
                <DiscreteDoubleKeyFrame KeyTime=""0:0:0.267"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        private bool _appBarIsVisible;
        private Storyboard _hideStoryboard;
        private PhoneApplicationFrame _frame;
        private Grid _mbsRoot;
        private PhoneApplicationPage _page;
        private MessageBoxResult _result;
        private Grid _rootElement;
        private Storyboard _showStoryboard;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxService"/> class.
        /// </summary>
        public MessageBoxService()
        {
            this._hideStoryboard = XamlReader.Load(SwivelOutStoryboard) as Storyboard;
            this._showStoryboard = XamlReader.Load(SwivelInStoryboard) as Storyboard;
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        /// <value>The current page.</value>
        public PhoneApplicationPage CurrentPage
        {
            get
            {
                if ((null == this._page) &&
                    (null != this.RootFrame))
                {
                    this._page = this
                        .RootFrame
                        .GetVisualDescendants()
                        .OfType<PhoneApplicationPage>()
                        .FirstOrDefault();
                    if (null == this._page)
                    {
                        this._page = this.RootFrame.Content as PhoneApplicationPage;
                    }
                }

                return this._page;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a message box is open.
        /// </summary>
        /// <value><c>True</c> if a message box is open; otherwise, <c>false</c>.</value>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets or sets the message box result value, which indicates the button that the user tapped.
        /// </summary>
        /// <value>A <see cref="MessageBoxResult"></see> value. The default is <b>None</b>.</value>
        public MessageBoxResult Result
        {
            get
            {
                return this._result;
            }

            private set
            {
                this._result = value;
                if (MessageBoxResult.None != value)
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Gets the root frame.
        /// </summary>
        /// <value>The root frame.</value>
        public PhoneApplicationFrame RootFrame
        {
            get
            {
                if (null == this._frame)
                {
                    this._frame = Application.Current.RootVisual as PhoneApplicationFrame;
                }

                return this._frame;
            }
        }

        /// <summary>
        /// Displays a message box that contains the specified text, title bar caption, and
        /// response buttons.
        /// </summary>
        /// <param name="messageBoxText">The message to display.</param>
        /// <param name="caption">The title of the message box.</param>
        /// <param name="button">A value that indicates the button or buttons to display.</param>
        /// <param name="customLabels">A list of custom labels in the same order as the buttons to be displayed.</param>
        /// <exception cref="ArgumentNullException">The <b>messageBoxText</b> or <b>caption</b>
        /// parameter is null.</exception>
        /// <exception cref="ArgumentException">The <b>button</b> parameter is not a valid
        /// <see cref="MessageBoxServiceButton"/> value.</exception>
        /// <remarks>Handle the <see cref="MessageBoxService.Closed"/> event to determine which
        /// button the user tapped.</remarks>
        public void Show(
            string messageBoxText,
            string caption = "",
            MessageBoxServiceButton button = MessageBoxServiceButton.OK,
            List<string> customLabels = null)
        {
            if (null == messageBoxText)
            {
                throw new ArgumentNullException("messageBoxText");
            }

            if (null == caption)
            {
                throw new ArgumentNullException("caption");
            }

            if (((int)button < (int)MessageBoxServiceButton.OK) ||
                ((int)button > (int)MessageBoxServiceButton.YesNo))
            {
                throw new ArgumentException(
                    "The button property must be a valid value of the MesageBoxServiceButton enumeration",
                    "button");
            }

            // Reset the result.
            this.Result = MessageBoxResult.None;

            // Hide the application bar if it's currently visible.
            if ((null != this.CurrentPage) &&
                (null != this.CurrentPage.ApplicationBar))
            {
                this._appBarIsVisible = this.CurrentPage.ApplicationBar.IsVisible;
                this.CurrentPage.ApplicationBar.IsVisible = false;
            }

            // Hook up the BackKeyPress to close the message box.
            if (null != this.CurrentPage)
            {
                this.CurrentPage.BackKeyPress += this.OnBackKeyPress;
            }

            // Build the message box and start the show storyboard.
            this.BuildMessageBox(messageBoxText, caption, button, customLabels);
            this.IsOpen = true;
        }

        private void BuildMessageBox(
            string messageBoxText,
            string caption,
            MessageBoxServiceButton button,
            List<string> customLabels)
        {
            var rootVisual = this.FindRootVisual();
            if (null != rootVisual)
            {
                var bgColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
                var overlayColor = Color.FromArgb(127, bgColor.R, bgColor.G, bgColor.B);
                var fgColor = (Color)Application.Current.Resources["PhoneForegroundColor"];
                var opacityColor = Color.FromArgb(200, bgColor.R, bgColor.G, bgColor.B);
                this._rootElement = new Grid { Background = new SolidColorBrush(overlayColor) };
                this._rootElement.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                this._rootElement.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                this._mbsRoot = new Grid
                {
                    Background = new SolidColorBrush(overlayColor),
                    Projection = new PlaneProjection()
                };
                this._mbsRoot.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                this._mbsRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                this._mbsRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                this._rootElement.Children.Add(this._mbsRoot);

                // Make sure that our grid spans all rows and columns of it's parent, if the
                // parent is a Grid.
                if (rootVisual is Grid)
                {
                    var parent = (Grid)rootVisual;
                    int columnCount = parent.ColumnDefinitions.Count;
                    if (columnCount > 0)
                    {
                        this._rootElement.SetValue(Grid.ColumnSpanProperty, columnCount);
                    }

                    int rowCount = parent.RowDefinitions.Count;
                    if (rowCount > 0)
                    {
                        this._rootElement.SetValue(Grid.RowSpanProperty, rowCount);
                    }
                }

                Border background = new Border { Background = new SolidColorBrush(fgColor) };
                background.SetValue(Grid.ColumnSpanProperty, 2);
                background.SetValue(Grid.RowSpanProperty, 2);
                this._mbsRoot.Children.Add(background);
                Border opaqueOverlay = new Border { Background = new SolidColorBrush(opacityColor) };
                opaqueOverlay.SetValue(Grid.ColumnSpanProperty, 2);
                opaqueOverlay.SetValue(Grid.RowSpanProperty, 2);
                this._mbsRoot.Children.Add(opaqueOverlay);

                // Add the caption.
                StackPanel container = new StackPanel { Margin = new Thickness(12, 0, 12, 12) };
                container.SetValue(Grid.ColumnSpanProperty, 2);
                TextBlock title = new TextBlock
                {
                    FontFamily = new FontFamily("Segoe WP Semibold"),
                    FontSize = 32,
                    Margin = new Thickness(12),
                    Text = caption,
                    TextWrapping = TextWrapping.Wrap
                };
                title.SetValue(TextOptions.TextHintingModeProperty, TextHintingMode.Animated);
                container.Children.Add(title);

                // Add the message text.
                TextBlock message = new TextBlock
                {
                    FontSize = 24,
                    Margin = new Thickness(12, 0, 12, 0),
                    Text = messageBoxText,
                    TextWrapping = TextWrapping.Wrap
                };
                container.Children.Add(message);
                this._mbsRoot.Children.Add(container);
                rootVisual.Children.Add(this._rootElement);

                // Add the required buttons.
                switch (button)
                {
                    case MessageBoxServiceButton.OK:
                        this.CreateOKButton(
                            null == customLabels ? MessageBoxServiceStrings.OK : customLabels[0],
                            2);
                        container.SetValue(Grid.ColumnSpanProperty, 1);
                        break;
                    case MessageBoxServiceButton.OKCancel:
                        this._mbsRoot.ColumnDefinitions.Add(
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        this.CreateOKButton(
                            null == customLabels ? MessageBoxServiceStrings.OK : customLabels[0]);
                        this.CreateCancelButton(
                            null == customLabels ? MessageBoxServiceStrings.Cancel : customLabels[1],
                            1);
                        break;
                    case MessageBoxServiceButton.YesNoCancel:
                        this._mbsRoot.ColumnDefinitions.Add(
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        this._mbsRoot.ColumnDefinitions.Add(
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        background.SetValue(Grid.ColumnSpanProperty, 3);
                        opaqueOverlay.SetValue(Grid.ColumnSpanProperty, 3);
                        container.SetValue(Grid.ColumnSpanProperty, 3);
                        this.CreateYesButton(
                            null == customLabels ? MessageBoxServiceStrings.Yes : customLabels[0]);
                        this.CreateNoButton(
                            null == customLabels ? MessageBoxServiceStrings.No : customLabels[1],
                            3);
                        this.CreateCancelButton(
                            null == customLabels ? MessageBoxServiceStrings.Cancel : customLabels[2],
                            2);
                        break;
                    case MessageBoxServiceButton.YesNo:
                        this._mbsRoot.ColumnDefinitions.Add(
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        this.CreateYesButton(
                            null == customLabels ? MessageBoxServiceStrings.Yes : customLabels[0]);
                        this.CreateNoButton(
                            null == customLabels ? MessageBoxServiceStrings.No : customLabels[1]);
                        break;
                    default:
                        // Not possible to hit this.
                        break;
                }

                // Update and start the storyboard to show the message box.
                foreach (var timeline in this._showStoryboard.Children)
                {
                    Storyboard.SetTarget(timeline, this._mbsRoot);
                }

                // Once the elements are ready, start the storyboard to show them.
                this._mbsRoot.InvokeOnLayoutUpdated(() =>
                {
                    this._showStoryboard.Begin();
                });
            }
        }

        private void Close()
        {
            this.IsOpen = false;
            this._showStoryboard.Stop();
            this._hideStoryboard.Stop();
            foreach (var timeline in this._hideStoryboard.Children)
            {
                Storyboard.SetTarget(timeline, this._mbsRoot);
            }

            this._hideStoryboard.Completed += this.HideStoryboard_Complete;
            this._hideStoryboard.Begin();
        }

        private void CreateCancelButton(string label, int column)
        {
            Button cancel = new Button
            {
                Content = label,
                Margin = new Thickness(0, 0, 12, 12)
            };
            cancel.Click += this.OnCancel;
            cancel.SetValue(Grid.ColumnProperty, column);
            cancel.SetValue(Grid.RowProperty, 1);
            this._mbsRoot.Children.Add(cancel);
        }

        private void CreateNoButton(string label, int columnCount = 2)
        {
            Button no = new Button
            {
                Content = label,
                Margin = new Thickness(0, 0, columnCount > 2 ? 0 : 12, 12)
            };
            no.Click += this.OnNo;
            no.SetValue(Grid.ColumnProperty, 1);
            no.SetValue(Grid.RowProperty, 1);
            this._mbsRoot.Children.Add(no);
        }

        private void CreateOKButton(string label, int columnSpan = -1)
        {
            Button ok = new Button
            {
                Content = label,
                Margin = new Thickness(12, 0, 0, 12)
            };
            ok.Click += this.OnOK;
            ok.SetValue(Grid.RowProperty, 1);
            if (columnSpan > -1)
            {
                ok.Margin = new Thickness(12, 0, 12, 12);
                ok.SetValue(Grid.ColumnSpanProperty, columnSpan);
            }

            this._mbsRoot.Children.Add(ok);
        }

        private void CreateYesButton(string label)
        {
            Button yes = new Button
            {
                Content = label,
                Margin = new Thickness(12, 0, 0, 12)
            };
            yes.Click += this.OnYes;
            yes.SetValue(Grid.RowProperty, 1);
            this._mbsRoot.Children.Add(yes);
        }

        private Panel FindRootVisual()
        {
            if (null != this.CurrentPage)
            {
                // Return the first Panel element.
                return this.CurrentPage.GetVisualDescendants().OfType<Panel>().FirstOrDefault();
            }

            return null;
        }

        private void HideStoryboard_Complete(object sender, EventArgs e)
        {
            if (null != this._hideStoryboard)
            {
                this._hideStoryboard.Completed -= this.HideStoryboard_Complete;
            }

            this.RaiseClosed();
        }

        private void OnBackKeyPress(object sender, CancelEventArgs args)
        {
            if (true == this.IsOpen)
            {
                args.Cancel = true;
                this.Close();
            }
        }

        private void OnCancel(object sender, RoutedEventArgs args)
        {
            this.Result = MessageBoxResult.Cancel;
        }

        private void OnNo(object sender, RoutedEventArgs args)
        {
            this.Result = MessageBoxResult.No;
        }

        private void OnOK(object sender, RoutedEventArgs args)
        {
            this.Result = MessageBoxResult.OK;
        }

        private void OnYes(object sender, RoutedEventArgs args)
        {
            this.Result = MessageBoxResult.Yes;
        }

        private void RaiseClosed()
        {
            // Set this._page to null so that we re-evaluate it on the next Show to ensure that
            // we're referencing the right page every time.
            this._page = null;
            if ((null != this._rootElement) &&
                (null != this._rootElement.Parent))
            {
                (this._rootElement.Parent as Panel).Children.Remove(this._rootElement);
                this._rootElement = null;
                this._mbsRoot = null;
            }

            if ((null != this.CurrentPage) &&
                (null != this.CurrentPage.ApplicationBar))
            {
                this.CurrentPage.ApplicationBar.IsVisible = this._appBarIsVisible;
            }

            var handler = this.Closed;
            if (null != handler)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
