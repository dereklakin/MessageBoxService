using System;
using System.Collections;
using System.Windows;
using Microsoft.Phone.Controls;
using PixelLab.Common;

namespace MessageBoxService
{
    public partial class MainPage : PhoneApplicationPage
    {
        private PixelLab.Common.MessageBoxService _service = new PixelLab.Common.MessageBoxService();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
                {
                    this._buttons.Items.Add(new DictionaryEntry("OK", MessageBoxServiceButton.OK));
                    this._buttons.Items.Add(new DictionaryEntry("OK/Cancel", MessageBoxServiceButton.OKCancel));
                    this._buttons.Items.Add(new DictionaryEntry("Yes/No", MessageBoxServiceButton.YesNo));
                    this._buttons.Items.Add(new DictionaryEntry("Yes/No/Cancel", MessageBoxServiceButton.YesNoCancel));
                    this._buttons.SelectedIndex = 1;
                };
        }

        private void MessageBox_Click(object sender, RoutedEventArgs e)
        {
            var button = (MessageBoxServiceButton)((DictionaryEntry)this._buttons.SelectedItem).Value;
            MessageBox.Show(
                this._message.Text, 
                this._caption.Text,
                button == MessageBoxServiceButton.OK ? MessageBoxButton.OK : MessageBoxButton.OKCancel);
        }

        private void MessageBoxService_Click(object sender, RoutedEventArgs e)
        {
            this._service.Closed += this.MessageBoxService_Closed;
            this._service.Show(
                this._message.Text,
                this._caption.Text,
                (MessageBoxServiceButton)((DictionaryEntry)this._buttons.SelectedItem).Value);
        }

        private void MessageBoxService_Closed(object sender, EventArgs e)
        {
            this._service.Closed -= this.MessageBoxService_Closed;
            this._result.Text = this._service.Result.ToString();
        }
    }
}