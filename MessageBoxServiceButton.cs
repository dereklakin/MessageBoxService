namespace PixelLab.Common
{
    /// <summary>
    /// Specifies the buttons that are displayed by the message box service. Used as an argument 
    /// to the <see cref="MessageBoxService.Show"/> method.
    /// </summary>
    /// <remarks>The value of the button that a user taps is returned by 
    /// <see cref="MessageBoxService.Show"/> and is one of the values of the 
    /// <see cref="MessageBoxResult"/> enumeration.</remarks>
    public enum MessageBoxServiceButton
    {
        /// <summary>
        /// The message box service displays an <b>OK</b> button.
        /// </summary>
        OK = 0,

        /// <summary>
        /// The message box service displays <b>OK</b> and <b>Cancel</b> buttons.
        /// </summary>
        OKCancel = 1,

        /// <summary>
        /// The message box service displays <b>Yes</b>, <b>No</b>, and <b>Cancel</b> buttons.
        /// </summary>
        YesNoCancel = 2,

        /// <summary>
        /// The message box service displays <b>Yes</b> and <b>No</b> buttons.
        /// </summary>
        YesNo = 3
    }
}