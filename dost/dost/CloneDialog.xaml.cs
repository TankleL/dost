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
using System.Windows.Shapes;

namespace dost
{
    /// <summary>
    /// CloneDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CloneDialog : Window
    {
        public string RepoPath = "";

        public CloneDialog()
        {
            InitializeComponent();

            m_txtRemotePath.PreviewMouseDown += new MouseButtonEventHandler(m_txtRemotePath_PreviewMouseDown);
            m_txtRemotePath.GotFocus += new RoutedEventHandler(m_txtRemotePath_GotFocus);
            m_txtRemotePath.LostFocus += new RoutedEventHandler(m_txtRemotePath_LostFocus);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_txtLocalPath.Content = "请选择要放置仓库的目录";
            m_txtRemotePath.Text = "请在此填写远程仓库的URL路径(https)";
        }

        private void m_btnLocalPlace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_btnLocalPlace.Focus();

            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "选择仓库（文件夹）放置位置";
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_txtLocalPath.Content = dlg.SelectedPath;
            }
        }

        private void m_btnOkay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_btnOkay.Focus();

            string remoteUrl = m_txtRemotePath.Text;
            string localPath = (string)m_txtLocalPath.Content;
            string repoName = "err";

            try
            {
                repoName = remoteUrl.Substring(remoteUrl.LastIndexOf('/') + 1);
                repoName = repoName.Substring(0, repoName.LastIndexOf(".git"));
            }
            catch(Exception)
            {
                MessageBox.Show("远程仓库路径存在问题", "错误(Error)");
                return;
            }

            if (repoName.Length <= 0)
            {
                return;
            }

            try
            {
                LibGit2Sharp.Repository.Clone(remoteUrl, localPath + '/' + repoName);
            }
            catch(Exception ex)
            {
                MessageBox.Show("尝试从远程拷贝失败\r\n" + ex.Message, "错误(Error)");
                return;
            }

            RepoPath = localPath + '/' + repoName;
            this.DialogResult = true;
        }


        private void m_txtRemotePath_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            m_txtRemotePath.Focus();
            e.Handled = true;
        }

        private void m_txtRemotePath_GotFocus(object sender, RoutedEventArgs e)
        {
            m_txtRemotePath.SelectAll();
            m_txtRemotePath.PreviewMouseDown -= new MouseButtonEventHandler(m_txtRemotePath_PreviewMouseDown);
        }

        private void m_txtRemotePath_LostFocus(object sender, RoutedEventArgs e)
        {
            m_txtRemotePath.PreviewMouseDown += new MouseButtonEventHandler(m_txtRemotePath_PreviewMouseDown);
        }
    }
}
