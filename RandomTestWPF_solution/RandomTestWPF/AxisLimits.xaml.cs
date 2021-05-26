using System;
using System.Windows;
using System.Windows.Controls;
//using LiveCharts;
using LiveCharts.Wpf;

namespace RandomTestWPF
{
    /// <summary>
    /// Interaction logic for AxisLimits.xaml
    /// </summary>
    public partial class AxisLimits : UserControl
    {
        Axis _axis;
        CartesianChart _chart;
        double _setMinVal;

        /// <summary>Binds the working chart to the control.</summary>
        /// <param name="chart">Chart to bind.</param>
        /// <param name="axIndaxDir">Axis direction: 0 - X, 1 - Y.</param>
        public void BindChart(CartesianChart chart, int axDir)
        {
            _chart = chart;
            _axis = axDir == 0 ? _chart.AxisX[0] : _chart.AxisY[0];
            _setMinVal = _axis.MinValue;
        }

        public AxisLimits()
        {
            InitializeComponent();
            SubGroupIsEnabled = chkBoxLim.IsChecked ?? false;
        }

        public string Title { set => lblLimits.Text = value; }

        /// <summary>
        /// Indicates whether the exchange of control and axis limits is to be ignored.
        /// </summary>
        /// <remarks>True while zooming, null while first drawing, false otherwise.</remarks>
        public bool? IsSkipped { get; set; }

        /// <summary>Indicates whether the control is ckecked.</summary>
        public bool IsChecked
        { 
            get => chkBoxLim.IsChecked ?? false;
            set => chkBoxLim.IsChecked = value;
        }

        public int? Increment { set => nmrLimMin.Increment = nmrLimMax.Increment = value; }

        public int? Minimum {
            //get => nmrLimMin.Minimum;
            set => nmrLimMin.Minimum = value; 
        }

        /// <summary>Initializes axis limits (range) by numeric limits.</summary>
        /// <param name="chartRefresh">If true then refresh the chart.</param>
        public void SetAxisActualRange(bool chartRefresh = false) 
        {
            if (chartRefresh || chkBoxLim.IsChecked == true)
            {
                _axis.MinValue = Convert.ToDouble(nmrLimMin.Value);
                _axis.MaxValue = Convert.ToDouble(nmrLimMax.Value);
            }
            if (chartRefresh) _chart.Refresh();
        }

        /// <summary>Initializes axis limits (range) by default min and max values.</summary>
        /// <param name="chartRefresh">If true then refresh the chart.</param>
        public void SetAxisDefRange(bool chartRefresh = false)
        {
            _axis.MinValue = _setMinVal;
            _axis.MaxValue = double.NaN;
            if (chartRefresh) _chart.Refresh();
        }

        /// <summary>Initializes numeric limits by axis default range.</summary>
        public void SetActualRange()
        {
            nmrLimMin.Value = Convert.ToInt32(_axis.ActualMinValue);
            nmrLimMax.Value = Convert.ToInt32(_axis.ActualMaxValue);
        }

        /// <summary>Sets a value indicating whether the subordinate controls group is enabled.</summary>
        private bool SubGroupIsEnabled { set => nmrLimMin.IsEnabled = nmrLimMax.IsEnabled = value; }

         private void nmrLimMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsSkipped != false)   return;
            _axis.MinValue = Convert.ToDouble(nmrLimMin.Value);
            _chart.Refresh();
        }

        private void nmrLimMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsSkipped != false) return;
            _axis.MaxValue = Convert.ToDouble(nmrLimMax.Value);
            _chart.Refresh();
        }

        private void chkBoxLim_Checked(object sender, RoutedEventArgs e)
        {
            SubGroupIsEnabled = true;
            SetAxisActualRange(true);
        }

        private void chkBoxLim_Unchecked(object sender, RoutedEventArgs e)
        {
            SubGroupIsEnabled = false;
            SetAxisDefRange(true);
        }
    }
}
