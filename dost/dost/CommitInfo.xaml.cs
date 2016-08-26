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

using System.ComponentModel;
using System.Globalization;
using GitSharp;

namespace dost
{
    /// <summary>
    /// CommitInfo.xaml 的交互逻辑
    /// </summary>
    public partial class CommitInfo : INotifyPropertyChanged
    {
        public CommitInfo()
        {
            InitializeComponent();
            DataContext = this;
        }

        private Commit m_commit;

        public Commit Commit
        {
            get { return m_commit; }
            set
            {
                m_commit = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Commit"));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }

    [ValueConversion(typeof(string), typeof(Author))]
    public class AuthorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Error. Author shouldn't be null.";
            var author = value as Author;
            return author.Name + " <" + author.EmailAddress + ">";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
