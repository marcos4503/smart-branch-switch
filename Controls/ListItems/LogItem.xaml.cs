using System.Windows;
using System.Windows.Controls;

namespace Smart_Branch_Switch.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "LogItem" that represents each log printed by SBS run.
    */

    public partial class LogItem : UserControl
    {
        //Public variables
        public Window instantiatedByWindow = null;

        //Core methods

        public LogItem(Window instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
        }

        public void SetLogIcon(MainWindow.RunNotificationTier runNotificationTier)
        {
            //Disable all icons
            regular.Visibility = Visibility.Collapsed;
            warning.Visibility = Visibility.Collapsed;
            critical.Visibility = Visibility.Collapsed;

            //Enable the right icon
            if (runNotificationTier == MainWindow.RunNotificationTier.Regular)
                regular.Visibility = Visibility.Visible;
            if (runNotificationTier == MainWindow.RunNotificationTier.Warning)
                warning.Visibility = Visibility.Visible;
            if (runNotificationTier == MainWindow.RunNotificationTier.Critical)
                critical.Visibility = Visibility.Visible;
        }

        public void SetMessage(string message)
        {
            //Set the desired message
            this.message.Text = message;
        }
    }
}
