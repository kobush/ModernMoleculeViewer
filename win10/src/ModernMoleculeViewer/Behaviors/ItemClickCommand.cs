using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace ModernMoleculeViewer.Behaviors
{
    public class ItemClickCommand : DependencyObject, IBehavior
    {
        private ListViewBase _control;

        public DependencyObject AssociatedObject => _control;

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand),
            typeof(ItemClickCommand), new PropertyMetadata(null));


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public void Attach(DependencyObject associatedObject)
        {
            _control = associatedObject as ListViewBase;
            if (_control != null)
            {
                _control.IsItemClickEnabled = true;
                _control.ItemClick += OnItemClick;
            }
        }

        public void Detach()
        {
            if (_control != null)
            {
                _control.ItemClick -= OnItemClick;
                _control = null;
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (Command != null && Command.CanExecute(e.ClickedItem))
                Command.Execute(e.ClickedItem);
        }
    }
}
