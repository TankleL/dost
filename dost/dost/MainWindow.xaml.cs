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

using System.IO;
using Microsoft.Win32;
using GitSharp;

namespace dost
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Repository  m_repo;
        private String[]    m_startArgs;

        public MainWindow(String[] startArgs)
        {
            InitializeComponent();
            m_historyGraph.CommitClicked += SelectCommit;
            m_commitDiffView.SelectionChanged += SelectionChanged;
           
            m_fileTree.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.m_fileTree.RaiseEvent(eventArg);
            };

            m_startArgs = startArgs;
            if (m_startArgs != null && m_startArgs.Length > 0)
            {
                if(LoadRepo(m_startArgs[0]))
                {
                    m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                    m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
                    m_cards.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("无法打开仓库", "错误");
                }
            }
            else
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Visible;

                m_girdBrowse.Visibility = System.Windows.Visibility.Hidden;
                m_cards.Visibility = System.Windows.Visibility.Hidden;                
            }

            DocOpt.ClearSpace();

            m_msgStatText.DataContext = m_statText;
        }


        public void SetStatusReady()
        {
            m_statText.Content = "就绪";
        }

        public void SetStatus(string status)
        {
            m_statText.Content = status;
        }

        public void ShowWaitUI(bool onoff)
        {
            if (onoff == true)
            {
                m_waitFrame.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            m_waitFrame.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SelectionChanged(Change change)
        {
            m_textDiff.Show(change);
        }

        private void SelectCommit(Commit commit)
        {
            if (commit == null || commit.Tree == null)
                return;

            try
            {
                SetStatus("正在读取信息...");
                m_commitInfo.Commit = commit;
                m_fileTree.ItemsSource = commit.Tree.Children;
                m_commitDiffView.Init(commit.Parent, commit);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
            finally
            {
                SetStatusReady();
            }
        }

        private bool NewRepo()
        {
            bool res = false;
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "选择仓库（文件夹）所在位置";
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetStatus("创建中...");

                try
                {
                    GitSharp.Git.Init(dlg.SelectedPath);

                    if (null != m_repo)
                    {
                        m_repo.Close();
                        m_repo = null;
                    }

                    m_repo = new Repository(dlg.SelectedPath);
                    m_historyGraph.Update(m_repo);
                    m_repoPathText.Text = dlg.SelectedPath;
                    SelectCommit(m_repo.Head.CurrentCommit);

                    res = true;
                }
                catch (Exception ex)
                {
                    SetStatus("发生错误...");
                    MessageBox.Show(ex.Message, "创建过程错误");
                }
                finally
                {
                    SetStatusReady();
                }
            }

            return res;
        }

        private bool LoadRepo()
        {
            bool res = false;

            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "选择仓库（文件夹）所在位置";
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetStatus("加载中...");

                try
                {
                    m_repo = new Repository(dlg.SelectedPath);
                    m_historyGraph.Update(m_repo);
                    m_statusView.Update(m_repo);
                    m_repoPathText.Text = dlg.SelectedPath;
                    SelectCommit(m_repo.Head.CurrentCommit);

                    res = true;
                }
                catch (Exception ex)
                {
                    SetStatus("发生错误...");
                    MessageBox.Show(ex.Message, "加载过程错误");
                }
                finally
                {
                    SetStatusReady();                    
                }
            }

            return res;
        }

        private bool LoadRepo(string repoPath)
        {
            bool res = false;
            try
            {
                SetStatus("加载中...");

                if (null != m_repo)
                {
                    m_repo.Close();
                    m_repo = null;
                }

                m_repo = new Repository(repoPath);
                m_historyGraph.Update(m_repo);
                m_statusView.Update(m_repo);
                SelectCommit(m_repo.Head.CurrentCommit);

                res = true;
            }
            catch (Exception ex)
            {
                SetStatus("发生错误...");
                MessageBox.Show(ex.Message, "加载过程错误");
            }
            finally
            {
                SetStatusReady();
            }

            return res;
        }

        private bool ReLoadRepo(string repoPath)
        {
            bool res = false;
            try
            {
                SetStatus("加载中...");

                if (null != m_repo)
                {
                    m_repo.Close();
                    m_repo = null;
                }

                m_repo = new Repository(repoPath);
                m_historyGraph.Update(m_repo);
                m_statusView.Update(m_repo);
                SelectCommit(m_repo.Head.CurrentCommit);

                res = true;
            }
            catch (Exception ex)
            {
                SetStatus("发生错误...");
                MessageBox.Show(ex.Message, "加载过程错误");
            }
            finally
            {
                SetStatusReady();
            }

            return res;
        }

        private bool CloneRepoAndStart()
        {
            CloneDialog cloneDlg = new CloneDialog();
            cloneDlg.Owner = this;
            if(cloneDlg.ShowDialog() == true)
            {
                if (null != m_repo)
                {
                    m_repo.Close();
                    m_repo = null;
                }

                try
                {
                    m_repo = new Repository(cloneDlg.RepoPath);
                    m_historyGraph.Update(m_repo);
                    m_statusView.Update(m_repo);
                    SelectCommit(m_repo.Head.CurrentCommit);
                }
                catch(Exception ex)
                {
                    SetStatus("发生错误...");
                    MessageBox.Show(ex.Message, "加载过程错误");
                }
                finally
                {
                    SetStatusReady();
                }

                return true;
            }
            else
                return false;
        }

        private void LeaveRepo()
        {
            if(m_repo != null)
                m_repo.Close();
        }

        private void OnLoadRepo(object sender, RoutedEventArgs e)
        {
            if(LoadRepo())
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
                m_mi_mode_browse.IsChecked = true;
                m_mi_mode_edit.IsChecked = false;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnReloadRepo(object sender, RoutedEventArgs e)
        {
            if(ReLoadRepo(m_repoPathText.Text))
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
                m_mi_mode_browse.IsChecked = true;
                m_mi_mode_edit.IsChecked = false;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnLeaveRepo(object sender, RoutedEventArgs e)
        {
            LeaveRepo();

            m_gridWelcome.Visibility = System.Windows.Visibility.Visible;

            m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
            m_mi_mode_browse.IsChecked = true;
            m_mi_mode_edit.IsChecked = false;

            m_cards.Visibility = System.Windows.Visibility.Hidden;
            m_girdBrowse.Visibility = System.Windows.Visibility.Hidden;
        }

        private void m_btnLoadRepo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LoadRepo())
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
                m_mi_mode_browse.IsChecked = true;
                m_mi_mode_edit.IsChecked = false;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;

                m_textDiff.Clear();
            }
        }

        private void m_btnNewRepo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(NewRepo())
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
                m_mi_mode_browse.IsChecked = true;
                m_mi_mode_edit.IsChecked = false;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void m_btnCloneRepo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CloneRepoAndStart())
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
                m_mi_mode_browse.IsChecked = true;
                m_mi_mode_edit.IsChecked = false;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnNewRepo(object sender, RoutedEventArgs e)
        {
            if (NewRepo())
            {
                m_gridWelcome.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
                m_mi_mode_browse.IsChecked = true;
                m_mi_mode_edit.IsChecked = false;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnBrowseMode(object sender, RoutedEventArgs e)
        {
            m_mi_mode_browse.IsChecked = true;
            m_mi_mode_edit.IsChecked = false;

            if (null != m_repo)
            {
                m_girdBrowse.Visibility = System.Windows.Visibility.Visible;
                m_gridEdit.Visibility = System.Windows.Visibility.Hidden;
            }

        }

        private void OnEditMode(object sender, RoutedEventArgs e)
        {
            m_mi_mode_edit.IsChecked = true;
            m_mi_mode_browse.IsChecked = false;

            if (null != m_repo)
            {
                m_girdBrowse.Visibility = System.Windows.Visibility.Hidden;

                m_gridEdit.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
