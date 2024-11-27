using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

using LiveCharts;
using LiveCharts.Wpf;
//using Xceed.Wpf.Toolkit;

namespace RandomTestWPF
{
    public static class ExtensionMethods
    {
        private static readonly Action EmptyDelegate = delegate { };

        /// <summary>Forces a control external update.</summary>
        /// <param name="uiElement"></param>
        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Numbers numbers;
        /// <summary>Series counter for indexing the series name.</summary>
        int seriesCounter = 0;
        /// <summary>Width of graphics and splitter controls.</summary>
        /// <remarks>Used when hiding-visualizing the chart.</remarks>
        double extChartWidth;
        /// <summary>Line smoothness: 0 - straight lines, 1 smooth lines.</summary>
        double lnSmoothness;

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        /// <summary>Chart series collection.</summary>
        public SeriesCollection Values { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            cmbBoxRNG.SelectedIndex = 3;
            cmbBoxNormGen.SelectedIndex = 2;

            Values = new SeriesCollection();
            Chart.DataTooltip = chkBoxToolTip.IsChecked == true ? new DefaultTooltip() : null;

            LimitsX.BindChart(Chart, 0);
            LimitsY.BindChart(Chart, 1);
            NormParams.DepControls = new List<UIElement> { pnlNormMethod }; // auto-enabling Norm panel
            LNormParams.DepControls = new List<UIElement> {
                chkBoxStdLnorm, // auto-enabling Cstd checkbox
                SSParams        // auto-enabling Size selection panel
            };
            LimitsIsEnabled = !(chkBoxAutoLim.IsChecked ?? false);
            lnSmoothness = chkBoxSpline.IsChecked == true ? 1 : 0;

            PrintCalcLnParams();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            #region Clear and Froze controls
            AllControlsEnabled = false;
            Cursor = Cursors.AppStarting;
            if (chkBoxChart.IsChecked == true)
            {
                ChartInfo.Text = "generate...";
                if (chkBoxAccum.IsChecked != true)
                {
                    Values.Clear();
                    Chart.InputBindings.Clear();
                    Chart.Visibility = Visibility.Hidden;
                }
                ChartPanel.Refresh();
            }
            lsViewResult.ItemsSource = null;
            lsViewResult.Items.Clear();
            lsViewResult.Refresh();
            seriesCounter = 0;
            #endregion
            #region Initialize Random Generator
            {
                bool isExp = LNormParams.IsChecked ?? false;
                float[] distVars = { 
                    isExp ? LNormParams.Mean : NormParams.Mean, 
                    isExp ? LNormParams.Sigma : NormParams.Sigma };
                int[] szSelVars = { 
                    Convert.ToInt32(SSParams.Mean),
                    Convert.ToInt32(SSParams.Sigma)
                };

                if (!RandomGen.Init(
                    cmbBoxRNG.SelectedIndex,
                    NormParams.IsChecked == true ? cmbBoxNormGen.SelectedIndex : -1,
                    isExp,
                    chkBoxStdLnorm.IsChecked ?? false,
                    distVars,
                    SSParams.IsChecked == true ? szSelVars : null
                    ))
                {
                    System.Windows.MessageBox.Show(String.Format(
                        "Extern C-library {0} is not found.\nYou should use C# random generator only.", CRandom.DllFile),
                        "Missing " + CRandom.DllFile);
                    //goto end;
                    return;
                }
            }
            #endregion
            #region Generate random numbers

            int cntNumb = (int)(nmrCountBase.Value * (int)Math.Pow(10, nmrCountPower.Value ?? 0));

            try { numbers = new Numbers(cntNumb, SSParams.IsChecked ?? false); }
            catch (OutOfMemoryException)
            {
                System.Windows.MessageBox.Show(String.Format(
                    "Not enough memory to visualize {0:0.###E0} cycles.\nTurn off Graphics and try again.", cntNumb),
                    "Out of memory");
                goto end;
            }

            int cntRNGcall = cntNumb;
            stopwatch.Reset();
            stopwatch.Start();
            float average = RandomGen.GetDistrib(numbers, ref cntRNGcall);
            stopwatch.Stop();
            #endregion
            #region TextInfo output

            lsViewResult.ItemsSource = numbers.Freqs;   // fill listView
            txtRngCalls.Text = cntRNGcall > 0 ?
                ((float)cntRNGcall / cntNumb).ToString("0.##") : "undef";
            TimeSpan ts = stopwatch.Elapsed;
            txtTickerVal.Text = ((float)ts.Ticks / cntNumb).ToString("0.##");
            txtTimerVal.Text = ts.ToString(@"mm\:ss\.ff");
            txtMode.Text = numbers.Freqs[numbers.SummitInd].X.ToString();
            txtMean.Text = average.ToString("0.##");
            txtXmax.Text = numbers.Freqs[numbers.Length - 1].X.ToString();

            #endregion
            #region Chart output
            if (chkBoxChart.IsChecked == true)
            {
                if (chkBoxAccum.IsChecked == false)
                    chkBoxAutoLim.IsChecked = true;
                DrawGraph(numbers.Freqs);
            }
            end:
            AllControlsEnabled = true;
            #endregion
        }

        /// <summary>Draws a graph./// </summary>
        /// <param name="pts">Series of plotted points</param>
        private void DrawGraph(Point[] pts)
        {
            Chart.Visibility = Visibility.Visible;

            var ls = new LineSeries
            {
                Configuration = new LiveCharts.Configurations.CartesianMapper<Point>()
                    .X(point => point.X)    // Define a function that returns a value that should map to the x-axis
                    .Y(point => point.Y)    // Define a function that returns a value that should map to the y-axis
                    //.Stroke(point => Brushes.Transparent) // Define a function that returns a Brush based on the current data item
                    //.Fill(point => Brushes.Transparent)
                    ,
                Title = "Series" + (++seriesCounter).ToString(),
                LineSmoothness = lnSmoothness,
                Values = new ChartValues<Point>(pts),
                PointGeometry = null,       //  to improve the overall plotting performance
                Fill = Brushes.Transparent
            };
            Values.Add(ls);
            ChartInfo.Text = string.Empty;
            LimitsIsSkipped = null;
            Cursor = null;
            DataContext = this;
        }

        private bool AllControlsEnabled
        {
            set => MainButtonPanel.IsEnabled = MainPanel.IsEnabled = value;
        }

        #region Limits control

        /// <summary>Raises when the range of an axis is changed by zooming.</summary>
        /// <param name="eventArgs"></param>
        private void Axis_RangeChanged(LiveCharts.Events.RangeChangedEventArgs eventArgs)
        {
            Chart.Cursor = Cursors.AppStarting;
            LimitsIsSkipped = true;
        }

        /// <summary>Raises when the chart is updated</summary>
        /// <param name="sender"></param>
        private void Chart_UpdaterTick(object sender)
        {
            if (LimitsX.IsSkipped != false)             // zooming|panning OR first drawing
            {
                if (LimitsX.IsSkipped == true)          // zooming|panning
                {
                    Chart.Cursor = null;                // back to default cursos
                    LimitsX.IsChecked = true;
                    chkBoxAutoLim.IsChecked = false;    // LimitsIsSkipped is true!
                }
                LimitsX.SetActualRange();
                LimitsY.SetActualRange();
                LimitsIsSkipped = false;
            }
        }

        private bool? LimitsIsSkipped
        {
            get => LimitsX.IsSkipped;
            set => LimitsX.IsSkipped = LimitsY.IsSkipped = value;
        }

        private bool LimitsIsEnabled
        {
            set => LimitsX.IsEnabled = LimitsY.IsEnabled = value;
        }

        private void chkBoxAutoLim_Checked(object sender, RoutedEventArgs e)
        {
            LimitsIsEnabled = false;
            if (LimitsX.IsSkipped != false) return;
            LimitsX.SetAxisDefRange();
            LimitsY.SetAxisDefRange();
        }

        private void chkBoxAutoLim_Unchecked(object sender, RoutedEventArgs e)
        {
            LimitsIsEnabled = true;
            if (LimitsX.IsSkipped != false) return;
            LimitsX.SetAxisActualRange();
            LimitsY.SetAxisActualRange();
        }

        #endregion
        #region View control
        private void PrintCalcLnParams()
        {
            float sigmaPower = LNormParams.Sigma * LNormParams.Sigma;

            txtCalcMode.Text = ((float)Math.Exp(LNormParams.Mean - sigmaPower)).ToString("0.#");
            txtCalcMean.Text = ((float)Math.Exp(LNormParams.Mean + sigmaPower / 2)).ToString("0.#");
            txtCalcMedian.Text = ((float)Math.Exp(LNormParams.Mean)).ToString("0.#");
        }

        private void chkBoxGrid_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                Chart.AxisX[0].Separator.StrokeThickness =
                Chart.AxisY[0].Separator.StrokeThickness =
                Convert.ToDouble((sender as CheckBox).IsChecked ?? false);
        }

        private void chkBoxToolTip_Checked(object sender, RoutedEventArgs e)
        {
            Chart.DataTooltip = new DefaultTooltip();
        }

        private void chkBoxToolTip_Unchecked(object sender, RoutedEventArgs e)
        {
            Chart.DataTooltip = null;
        }

        private void chkBoxSpline_CheckedChanged(object sender, RoutedEventArgs e)
        {
            lnSmoothness = Convert.ToDouble((sender as CheckBox).IsChecked ?? false);
            foreach (LineSeries ls in Values)
                ls.LineSmoothness = lnSmoothness;
        }

        private void chkBoxChart_Checked(object sender, RoutedEventArgs e)
        {
            //splitter.IsEnabled = true;
            this.Width += extChartWidth;
            this.ResizeMode = ResizeMode.CanResize;
        }

        private void chkBoxChart_Unchecked(object sender, RoutedEventArgs e)
        {
            //splitter.IsEnabled = false;
            extChartWidth = splitter.ActualWidth + ChartPanel.ActualWidth;
            this.Width -= extChartWidth;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void LNormParams_ValueChanged(object sender, EventArgs e)
        {
            if (txtCalcMode != null)    // during initialization TextBlocks 'txtCalc...' have not yet been created
                PrintCalcLnParams();
        }

        #endregion
    }
}
