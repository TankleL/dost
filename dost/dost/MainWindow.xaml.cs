﻿using System;
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

using Microsoft.Win32;
using GitSharp;

namespace dost
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Repository m_repo;

        public MainWindow()
        {
            InitializeComponent();
            m_historyGraph.CommitClicked += SelectCommit;
            m_commitDiffView.SelectionChanged += change => m_textDiff.Show(change);
           
            m_fileTree.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.m_fileTree.RaiseEvent(eventArg);
            };

            m_bgkLoadRepo.Visibility = System.Windows.Visibility.Visible;
            m_btnLoadRepo.Visibility = System.Windows.Visibility.Visible;

            m_bgkNewRepo.Visibility = System.Windows.Visibility.Visible;
            m_btnNewRepo.Visibility = System.Windows.Visibility.Visible;

            m_cards.Visibility = System.Windows.Visibility.Hidden;
            m_gridDetailInfo.Visibility = System.Windows.Visibility.Hidden;
            m_commitInfo.Visibility = System.Windows.Visibility.Hidden;
        }


        private void SetStatusReady()
        {
            m_statText.Content = "就绪";
        }

        private void SelectCommit(Commit commit)
        {
            if (commit == null || commit.Tree == null)
                return;

            try
            {
                m_statText.Content = "正在读取信息...";
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

        private bool LoadRepo()
        {
            bool res = false;
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "选择仓库（文件夹）所在位置";
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_statText.Content = "加载中...";

                try
                {
                    m_repo = new Repository(dlg.SelectedPath);
                    m_historyGraph.Update(m_repo);
                    m_repoPathText.Text = dlg.SelectedPath;
                    SelectCommit(m_repo.Head.CurrentCommit);

                    res = true;
                }
                catch (Exception ex)
                {
                    m_statText.Content = "发生错误...";
                    MessageBox.Show(ex.Message, "加载过程错误");
                }
                finally
                {
                    SetStatusReady();                    
                }
            }

            return res;
        }

        private bool ReLoadRepo(string repoPath)
        {
            bool res = false;
            try
            {
                m_statText.Content = "加载中...";

                if (null != m_repo)
                    m_repo.Close();
                m_repo = new Repository(repoPath);
                m_historyGraph.Update(m_repo);
                SelectCommit(m_repo.Head.CurrentCommit);

                res = true;
            }
            catch (Exception ex)
            {
                m_statText.Content = "发生错误...";
                MessageBox.Show(ex.Message, "加载过程错误");
            }
            finally
            {
                SetStatusReady();
            }

            return res;
        }

        private void LeaveRepo()
        {
            m_repo.Close();
        }

        private void OnLoadRepo(object sender, RoutedEventArgs e)
        {
            if(LoadRepo())
            {
                m_bgkLoadRepo.Visibility = System.Windows.Visibility.Hidden;
                m_btnLoadRepo.Visibility = System.Windows.Visibility.Hidden;

                m_bgkNewRepo.Visibility = System.Windows.Visibility.Hidden;
                m_btnNewRepo.Visibility = System.Windows.Visibility.Hidden;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_commitDiffView.Visibility = System.Windows.Visibility.Visible;
                m_commitInfo.Visibility = System.Windows.Visibility.Visible;
                m_gridDetailInfo.Visibility = System.Windows.Visibility.Visible;
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
                m_bgkLoadRepo.Visibility = System.Windows.Visibility.Hidden;
                m_btnLoadRepo.Visibility = System.Windows.Visibility.Hidden;

                m_bgkNewRepo.Visibility = System.Windows.Visibility.Hidden;
                m_btnNewRepo.Visibility = System.Windows.Visibility.Hidden;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_commitDiffView.Visibility = System.Windows.Visibility.Visible;
                m_commitInfo.Visibility = System.Windows.Visibility.Visible;
                m_gridDetailInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnLeaveRepo(object sender, RoutedEventArgs e)
        {
            LeaveRepo();

            m_bgkLoadRepo.Visibility = System.Windows.Visibility.Visible;
            m_btnLoadRepo.Visibility = System.Windows.Visibility.Visible;

            m_bgkNewRepo.Visibility = System.Windows.Visibility.Visible;
            m_btnNewRepo.Visibility = System.Windows.Visibility.Visible;

            m_cards.Visibility = System.Windows.Visibility.Hidden;
            m_commitDiffView.Visibility = System.Windows.Visibility.Hidden;
            m_commitInfo.Visibility = System.Windows.Visibility.Hidden;
            m_gridDetailInfo.Visibility = System.Windows.Visibility.Hidden;
        }

        private void m_btnLoadRepo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LoadRepo())
            {
                m_bgkLoadRepo.Visibility = System.Windows.Visibility.Hidden;
                m_btnLoadRepo.Visibility = System.Windows.Visibility.Hidden;

                m_bgkNewRepo.Visibility = System.Windows.Visibility.Hidden;
                m_btnNewRepo.Visibility = System.Windows.Visibility.Hidden;

                m_cards.Visibility = System.Windows.Visibility.Visible;
                m_commitDiffView.Visibility = System.Windows.Visibility.Visible;
                m_commitInfo.Visibility = System.Windows.Visibility.Visible;
                m_gridDetailInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void m_btnNewRepo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

    }
}
