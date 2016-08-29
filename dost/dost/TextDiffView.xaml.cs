/*
 * Copyright (C) 2010, Henon <meinrad.recheis@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the project nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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

using System.Windows.Xps.Packaging;
using System.Threading;
using System.Text.RegularExpressions;
using GitSharp;

namespace dost
{
    /// <summary>
    /// TextDiffView.xaml 的交互逻辑
    /// </summary>
    public partial class TextDiffView : UserControl
    {
        public TextDiffView()
        {
            InitializeComponent();
            DataContext = this;

            ListA.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.ListA.RaiseEvent(eventArg);
            };

            ListB.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.ListB.RaiseEvent(eventArg);
            };
        }

        public void Init(Diff diff)
        {
            Diff = diff;
            ListA.ItemsSource = diff.Sections;
            ListB.ItemsSource = diff.Sections;
        }

        public Diff Diff
        {
            get;
            private set;
        }


        private List<string> m_workedCommits = new List<string>();
        private string m_workedPath = String.Empty;

        internal void Show(Change change)
        {
            if (change == null)
            {
                return;
            }
            
            // check repeat operation
            bool bRepeated = false;
            if(change.Commits.Length == m_workedCommits.Count && m_workedPath == change.Path)
            {
                bRepeated = true;
                HashSet<string> ciHash = new HashSet<string>();
                
                foreach(var ci in change.Commits)
                    ciHash.Add(ci.Hash);

                foreach(var worked in m_workedCommits)
                {
                    if(ciHash.Contains(worked) == false)
                    {
                        bRepeated = false;
                        break;
                    }
                }
            }

            if (true == bRepeated)
                return;

            m_workedPath = change.Path;
            m_workedCommits.Clear();
            foreach(var ci in change.Commits)
            {
                m_workedCommits.Add(ci.Hash);
            }
            
            
            // start processing difference analysis.
            Clear();
            
            var a = (change.ReferenceObject != null ? (change.ReferenceObject as Blob).RawData : new byte[0]);
            var b = (change.ComparedObject != null ? (change.ComparedObject as Blob).RawData : new byte[0]);

            try
            {
                if (change.Path.LastIndexOf(".doc") == change.Path.Length - 4)
                {
                    new Thread(() =>
                    {
                        XpsDocument resDoc = null;
                        Application.Current.Dispatcher.Invoke(new Action(()=>{
                            ((MainWindow)Application.Current.MainWindow).SetStatus("正在分析文件差异...");
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(true);
                        }));

                        // this process may take a long time to execute.
                        resDoc =
                            DocOpt.CompareWordDocument(a, DocOpt.OfficeDocuFormat.doc, b, DocOpt.OfficeDocuFormat.doc);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            m_docViewer.Document = resDoc.GetFixedDocumentSequence();
                            ((MainWindow)Application.Current.MainWindow).SetStatusReady();
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(false);
                        }));
                    }).Start();

                    ShowCardDoc();
                }
                else if (change.Path.LastIndexOf(".docx") == change.Path.Length - 5)
                {                    
                    new Thread(() =>
                    {
                        XpsDocument resDoc = null;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            ((MainWindow)Application.Current.MainWindow).SetStatus("正在分析文件差异...");
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(true);
                        }));

                        // this process may take a long time to execute.
                        resDoc =
                            DocOpt.CompareWordDocument(a, DocOpt.OfficeDocuFormat.docx, b, DocOpt.OfficeDocuFormat.docx);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            m_docViewer.Document = resDoc.GetFixedDocumentSequence();
                            ((MainWindow)Application.Current.MainWindow).SetStatusReady();
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(false);
                        }));
                    }).Start();

                    ShowCardDoc();
                }
                else if(change.Path.LastIndexOf(".jpg") == change.Path.Length - 4 ||
                    change.Path.LastIndexOf(".jpeg") == change.Path.Length - 5)
                {
                    // TODO:
                    // Show image - jpeg difference here
                }
                else
                {
                    a = (Diff.IsBinary(a) == true ? Encoding.ASCII.GetBytes("Binary content\nFile size: " + a.Length) : a);
                    b = (Diff.IsBinary(b) == true ? Encoding.ASCII.GetBytes("Binary content\nFile size: " + b.Length) : b);
                    Init(new Diff(a, b));

                    ShowCardText();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        public void Show(byte[] a, byte[] b, string path)
        {
            // start processing difference analysis.
            Clear();

            try
            {
                if (path.LastIndexOf(".doc") == path.Length - 4)
                {
                    new Thread(() =>
                    {
                        XpsDocument resDoc = null;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            ((MainWindow)Application.Current.MainWindow).SetStatus("正在分析文件差异...");
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(true);
                        }));

                        // this process may take a long time to execute.
                        resDoc =
                            DocOpt.CompareWordDocument(a, DocOpt.OfficeDocuFormat.doc, b, DocOpt.OfficeDocuFormat.doc);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            m_docViewer.Document = resDoc.GetFixedDocumentSequence();
                            ((MainWindow)Application.Current.MainWindow).SetStatusReady();
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(false);
                        }));
                    }).Start();

                    ShowCardDoc();
                }
                else if (path.LastIndexOf(".docx") == path.Length - 5)
                {
                    new Thread(() =>
                    {
                        XpsDocument resDoc = null;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            ((MainWindow)Application.Current.MainWindow).SetStatus("正在分析文件差异...");
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(true);
                        }));

                        // this process may take a long time to execute.
                        resDoc =
                            DocOpt.CompareWordDocument(a, DocOpt.OfficeDocuFormat.docx, b, DocOpt.OfficeDocuFormat.docx);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            m_docViewer.Document = resDoc.GetFixedDocumentSequence();
                            ((MainWindow)Application.Current.MainWindow).SetStatusReady();
                            ((MainWindow)Application.Current.MainWindow).ShowWaitUI(false);
                        }));
                    }).Start();

                    ShowCardDoc();
                }
                else if (path.LastIndexOf(".jpg") == path.Length - 4 ||
                    path.LastIndexOf(".jpeg") == path.Length - 5)
                {
                    // TODO:
                    // Show image - jpeg difference here
                }
                else
                {
                    a = (Diff.IsBinary(a) == true ? Encoding.ASCII.GetBytes("Binary content\nFile size: " + a.Length) : a);
                    b = (Diff.IsBinary(b) == true ? Encoding.ASCII.GetBytes("Binary content\nFile size: " + b.Length) : b);
                    Init(new Diff(a, b));

                    ShowCardText();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowCardDoc()
        {
            m_gridText.Visibility = System.Windows.Visibility.Hidden;
            m_gridImg.Visibility = System.Windows.Visibility.Hidden;
            m_gridDoc.Visibility = System.Windows.Visibility.Visible;
        }

        private void ShowCardText()
        {
            m_gridDoc.Visibility = System.Windows.Visibility.Hidden;
            m_gridImg.Visibility = System.Windows.Visibility.Hidden;
            m_gridText.Visibility = System.Windows.Visibility.Visible;            
        }

        private void ShowCardImg()
        {
            m_gridDoc.Visibility = System.Windows.Visibility.Hidden;
            m_gridText.Visibility = System.Windows.Visibility.Hidden;
            m_gridImg.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender == m_scrollview_A)
            {
                m_scrollview_B.ScrollToVerticalOffset(m_scrollview_A.VerticalOffset);
                m_scrollview_B.ScrollToHorizontalOffset(m_scrollview_A.HorizontalOffset);
                return;
            }
            if (sender == m_scrollview_B)
            {
                m_scrollview_A.ScrollToVerticalOffset(m_scrollview_B.VerticalOffset);
                m_scrollview_A.ScrollToHorizontalOffset(m_scrollview_B.HorizontalOffset);
                return;
            }
        }


        /// <summary>
        /// Work around a limitation in the framework that does not allow to stretch ListBoxItemTemplates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StretchDataTemplate(object sender, RoutedEventArgs e)
        {
            // found this method at: http://silverlight.net/forums/p/18918/70469.aspx#70469
            var t = sender as FrameworkElement;
            if (t == null)
                return;
            var p = VisualTreeHelper.GetParent(t) as ContentPresenter;
            if (p == null)
                return;
            p.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        internal void Clear()
        {
            ListA.ItemsSource = null;
            ListB.ItemsSource = null;
        }
    }


    /// <summary>
    /// Adjust text block sizes to correspond to each other by adding lines
    /// </summary>
    public class BlockTextConverterA : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var section = value as Diff.Section;
            if (section == null)
                return 0;
            var a_lines = section.EndA - section.BeginA;
            var b_lines = section.EndB - section.BeginB;
            var line_difference = Math.Max(a_lines, b_lines) - a_lines;
            var s = new StringBuilder(Regex.Replace(section.TextA, "\r?\n$", ""));
            if (a_lines == 0)
                line_difference -= 1;
            for (var i = 0; i < line_difference; i++)
                s.AppendLine();
            return s.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Adjust text block sizes to correspond to each other by adding lines
    /// </summary>
    public class BlockTextConverterB : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var section = value as Diff.Section;
            if (section == null)
                return 0;
            var a_lines = section.EndA - section.BeginA;
            var b_lines = section.EndB - section.BeginB;
            var line_difference = Math.Max(a_lines, b_lines) - b_lines;
            var s = new StringBuilder(Regex.Replace(section.TextB, "\r?\n$", ""));
            if (b_lines == 0)
                line_difference -= 1;
            for (var i = 0; i < line_difference; i++)
                s.AppendLine();
            return s.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Calculate block background color
    /// </summary>
    public class BlockColorConverterA : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var section = value as Diff.Section;
            if (section == null)
                return Brushes.Pink; // <-- this shouldn't happen anyway
            switch (section.EditWithRespectToA)
            {
                case Diff.EditType.Deleted:
                    return Brushes.LightSkyBlue;
                case Diff.EditType.Replaced:
                    return Brushes.LightSalmon;
                case Diff.EditType.Inserted:
                    return Brushes.DarkGray;
                case Diff.EditType.Unchanged:
                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Calculate block background color
    /// </summary>
    public class BlockColorConverterB : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var section = value as Diff.Section;
            if (section == null)
                return Brushes.Pink; // <-- this shouldn't happen anyway
            switch (section.EditWithRespectToA)
            {
                case Diff.EditType.Deleted:
                    return Brushes.DarkGray;
                case Diff.EditType.Replaced:
                    return Brushes.LightSalmon;
                case Diff.EditType.Inserted:
                    return Brushes.LightGreen;
                case Diff.EditType.Unchanged:
                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
