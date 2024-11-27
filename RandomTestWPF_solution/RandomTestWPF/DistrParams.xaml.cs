using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace RandomTestWPF
{
    /// <summary>
    /// Interaction logic for DistrParams.xaml
    /// </summary>
    public partial class DistrParams : UserControl
    {
        public event EventHandler SomeValueChanged;

        public DistrParams()
        {
            InitializeComponent();
            nmrMean.IsEnabled = nmrSD.IsEnabled = chkBoxActive.IsChecked ?? false;
            nmrMean.ValueChanged += HandleSomeValueChanged;
            nmrSD.ValueChanged += HandleSomeValueChanged;
        }

        public string Title {
            get => chkBoxActive.Content as string;
            set => chkBoxActive.Content = value;
        }

        public float Mean { get => nmrMean.Value ?? 0; set => nmrMean.Value = value; }

        public float IncrementMean { get => nmrMean.Increment ?? 0; set => nmrMean.Increment = value; }

        public float Sigma { get => nmrSD.Value ?? 0; set => nmrSD.Value = value; }

        public float IncrementSigma { get => nmrSD.Increment ?? 0; set => nmrSD.Increment = value; }

        public bool? IsChecked { get => chkBoxActive.IsChecked; set => chkBoxActive.IsChecked = value; }

        /// <summary>Get/set dependend controls to manage their enabling.</summary>
        public List<UIElement> DepControls { get; set; }

        protected virtual void OnSomeValueChanged(EventArgs e)
        {
            this.SomeValueChanged?.Invoke(this, e);
        }

        private void HandleSomeValueChanged(object sender, EventArgs e)
        {
            OnSomeValueChanged(EventArgs.Empty);
        }

        private void chkBoxActive_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool newVal = nmrMean.IsEnabled = nmrSD.IsEnabled = 
                (sender as CheckBox).IsChecked ?? false;
            if (DepControls != null)
                foreach (UIElement c in DepControls)
                    c.IsEnabled = newVal;
        }
    }
}
