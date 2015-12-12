using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace ModernMoleculeViewer.Behaviors
{
    public class ProgressBehavior : DependencyObject, IBehavior
    {

        public DependencyObject AssociatedObject { get; private set; }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string), typeof (ProgressBehavior),
            new PropertyMetadata(null, OnTextChanged));

        public bool IsVisible
        {

            get { return (bool)GetValue(IsVisibleProperty); }

            set { SetValue(IsVisibleProperty, value); }

        }

        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register("IsVisible", typeof(bool), typeof(ProgressBehavior),
            new PropertyMetadata(false, OnIsVisibleChanged));


        public double? Value
        {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
            typeof(object),
            typeof(ProgressBehavior),
            new PropertyMetadata(null, OnValueChanged));


        public void Attach(DependencyObject associatedObject)
        {

        }

        public void Detach()
        {

        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBehavior behavior = (ProgressBehavior)d;
            StatusBar.GetForCurrentView().ProgressIndicator.Text = behavior.Text;
        }


        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isvisible = (bool)e.NewValue;
            if (isvisible)
            {
                StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double? val = (double?)e.NewValue;
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = val;
        }
    }
}
