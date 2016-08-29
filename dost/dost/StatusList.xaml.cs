using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Globalization;
using GitSharp;
using dost.util;

namespace dost
{
        /// <summary>
    /// StatusList.xaml 的交互逻辑
    /// </summary>
    public partial class StatusList : IRepositoryView
    {
        public StatusList()
        {
            InitializeComponent();
        }

        public event Action<IEnumerable<PathStatus>> SelectionChanged;
        ObservableCollection<PathStatus> _status_paths = new ObservableCollection<PathStatus>();

        public Repository Repository
        {
            get;
            private set;
        }

        private void Reload()
        {
            _status_paths.Clear();
            m_list.ItemsSource = null;
            m_list.ItemsSource = _status_paths;
            ThreadPool.QueueUserWorkItem(o => new RepositoryStatus(Repository, new RepositoryStatusOptions { ForceContentCheck = false, PerPathNotificationCallback = OnUpdateStatus }));
            m_list.SelectionChanged += (o, args) =>
            {
                if (SelectionChanged != null)
                    SelectionChanged(m_list.SelectedItems.OfType<PathStatus>());
            };
        }
                

        public void Update(Repository repository)
        {
            Repository = repository;
            Reload();
        }

        private void OnUpdateStatus(PathStatus status)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _status_paths.Add(status)));
        }

        public static IValueConverter StatusToColorConverter
        {
            get
            {
                return new GenericConverter { ConvertFunc = ConvertStatusToColor };
            }
        }

        private static object ConvertStatusToColor(object arg, Type t, object parameter, CultureInfo culture)
        {
            var status = arg as PathStatus;
            if (status == null)
                return Brushes.Black;
            if (status.IndexPathStatus == IndexPathStatus.MergeConflict)
                return Brushes.Red;
            if (status.IndexPathStatus == IndexPathStatus.Added || status.IndexPathStatus == IndexPathStatus.Staged)
                return Brushes.Chartreuse;
            if (status.IndexPathStatus == IndexPathStatus.Removed)
                return Brushes.Orange;
            if (status.WorkingPathStatus == WorkingPathStatus.Modified || status.WorkingPathStatus == WorkingPathStatus.Missing)
                return Brushes.RoyalBlue;
            if (status.WorkingPathStatus == WorkingPathStatus.Untracked)
                return Brushes.Black;
            return Brushes.Black;
        }

        private void OnStage(object sender, RoutedEventArgs e)
        {
            var status_paths = m_list.SelectedItems.OfType<PathStatus>();
            if (status_paths == null)
                return;
            try
            {
                Repository.Index.Stage(status_paths.Select(sp => sp.Path).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reload();
        }

        private void OnUnstage(object sender, RoutedEventArgs e)
        {
            var status_paths = m_list.SelectedItems.OfType<PathStatus>();
            if (status_paths == null)
                return;
            try
            {
                Repository.Index.Unstage(status_paths.Select(sp => sp.Path).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reload();
        }

        private void OnCheckout(object sender, RoutedEventArgs e)
        {
            var status_paths = m_list.SelectedItems.OfType<PathStatus>();
            if (status_paths == null)
                return;
            try
            {
                Repository.Head.CurrentCommit.Checkout(status_paths.Select(sp => sp.Path).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reload();
        }

        private void OnCheckoutIndex(object sender, RoutedEventArgs e)
        {
            var status_paths = m_list.SelectedItems.OfType<PathStatus>();
            if (status_paths == null)
                return; try
            {
                Repository.Index.Checkout(status_paths.Select(sp => sp.Path).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reload();
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            var status_paths = m_list.SelectedItems.OfType<PathStatus>();
            if (status_paths == null)
                return;
            try
            {
                Repository.Index.Delete(status_paths.Select(sp => sp.Path).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reload();
        }

        private void OnRemove(object sender, RoutedEventArgs e)
        {
            var status_paths = m_list.SelectedItems.OfType<PathStatus>();
            if (status_paths == null)
                return;
            try
            {
                Repository.Index.Remove(status_paths.Select(sp => sp.Path).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reload();
        }

        private void OnCommitIndex(object sender, RoutedEventArgs e)
        {
            StartCommitDialog();
        }

        public void StartCommitDialog()
        {
            var dlg = new CommitDialog { Repository = Repository };
            dlg.Init(_status_paths.Where(p => p.IndexPathStatus != IndexPathStatus.Unchanged));
            dlg.ShowDialog();
            Update(Repository);
        }
    }
}
