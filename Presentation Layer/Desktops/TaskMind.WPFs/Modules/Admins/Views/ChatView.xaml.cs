using System.Collections.Specialized;
using System.Windows.Controls;
using TaskMind.WPFs.Modules.Admins.ViewModels;

namespace TaskMind.WPFs.Modules.Admins.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
            DataContextChanged += (_, __) =>
            {
                if (DataContext is ChatVM vm)
                {
                    vm.PropertyChanged += (_, e) =>
                    {
                        if (e.PropertyName == nameof(ChatVM.SelectedConversation))
                            HookMessages(vm);
                    };
                    HookMessages(vm);
                }
            };
        }

        private void HookMessages(ChatVM vm)
        {
            if (vm.SelectedConversation == null) return;

            vm.SelectedConversation.Messages.CollectionChanged -= Messages_CollectionChanged;
            vm.SelectedConversation.Messages.CollectionChanged += Messages_CollectionChanged;
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() => MessagesScroll.ScrollToEnd());
        }
    }
}