using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

namespace User_Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Manages most basic UI elements
    /// </summary>
    public partial class MainWindow : Window
    {
        Backend b { get; set; } // class that manages backend tasks
        string outFilePath { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            Mouse.OverrideCursor = Cursors.Wait;
            RunButton.IsEnabled = false;
            ComboBoxB.IsEnabled = false;
            CutButton.IsEnabled = false;

            b = new Backend();
            ComboBoxA.ItemsSource = b.GetStationList();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            b.DisplayMap(TubeMap);
            Mouse.OverrideCursor = null;
        }

        // Background Workers
        #region
        //Presorting worker
        private void BgwSorting_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker thisBgw = sender as BackgroundWorker;
            bool invalid = true;

            do
            {
                try
                {
                    LoadAndSort(thisBgw);
                    invalid = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid File / File Already in Use. Please Try Again.");
                }
            } while (invalid);
        }

        private void LoadAndSort(BackgroundWorker bgw)
        {
            string path;

            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "csv files (*.csv)|*.csv|txt files (*.txt)|*.txt";
            OFD.FilterIndex = 0;
            OFD.Title = "Select journey data file .csv";
            OFD.ShowDialog();

            path = OFD.FileName;
            b.LoadFile(bgw, path); // throws an error if file is invalid
        }

        private void BgwSorting_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusSoFar.Value = e.ProgressPercentage;
        }

        private void BgwSorting_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(b.NumberOfRecords.ToString("N0") + " records successfully loaded");
            RunButton.IsEnabled = true;
            RunButton.Content = "Run";
            RunButton.FontWeight = FontWeights.Bold;
        }

        //Analysis worker
        private void BgwAnalysis_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arguments = e.Argument as object[];
            string path = (string)arguments[0];
            List<string> cutEdges = (List<string>)arguments[1];

            bool invalid = true;

            do
            {
                try
                {
                    b.RunAnalysis(path, sender as BackgroundWorker, cutEdges);
                    invalid = false;
                }
                catch
                {
                    MessageBox.Show("File is already open. Please close and try again");
                }
            } while (invalid);
        }

        private void BgwAnalysis_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusSoFar.Value = e.ProgressPercentage; // update progress bar
        }

        private void BgwAnalysis_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Analysis Complete: " + b.GetDroppedRecords().ToString("N0") + " records dropped");
            sideBarA.Visibility = Visibility.Hidden; // hides old sidebar and reveals new one
            SideBarB.Visibility = Visibility.Visible;
            TimeSlider.Value = 72; // loads on midday
            DrawLegend();
        }
        #endregion
        // SideBar One
        #region
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadButton.IsEnabled = false;
            RunButton.Content = "Reading...";

            BackgroundWorker BgwSorting = new BackgroundWorker();
            BgwSorting.WorkerReportsProgress = true;
            BgwSorting.DoWork += BgwSorting_DoWork;
            BgwSorting.ProgressChanged += BgwSorting_ProgressChanged;
            BgwSorting.RunWorkerCompleted += BgwSorting_RunWorkerCompleted;

            BgwSorting.RunWorkerAsync();
        }

        private void ComboBoxA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = ComboBoxA.SelectedIndex;

            ComboBoxB.IsEnabled = true;
            ComboBoxB.ItemsSource = b.GetNeighbourNames(selection);
        }

        private void ComboBoxB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CutButton.IsEnabled = true;
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = ComboBoxB.SelectedIndex;
            string rec;
            rec = b.CutEdge(selectedIndex);
            CutStationsListBox.Items.Add(rec);

            ComboBoxA_SelectionChanged(null, null); // refires selection changed event

            CutButton.IsEnabled = false;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Title = "Save Loadings File as";
            SFD.Filter = "csv files (*.csv)|*.csv";
            SFD.FilterIndex = 0;
            SFD.ShowDialog();

            string path = SFD.FileName;

            if (path != "")
            {
                RunButton.IsEnabled = false;
                RunButton.Content = "Working...";
                //passing parameters to bgw
                object[] arguments = new object[2];

                List<string> cutEdges = new List<string>();
                foreach (var cutEdge in CutStationsListBox.Items)
                {
                    cutEdges.Add((string)cutEdge);
                }

                arguments[0] = path;
                arguments[1] = cutEdges;

                //bgw setup
                BackgroundWorker BgwAnalysis = new BackgroundWorker();
                BgwAnalysis.WorkerReportsProgress = true;
                BgwAnalysis.DoWork += BgwAnalysis_DoWork;
                BgwAnalysis.ProgressChanged += BgwAnalysis_ProgressChanged;
                BgwAnalysis.RunWorkerCompleted += BgwAnalysis_RunWorkerCompleted;

                BgwAnalysis.RunWorkerAsync(arguments);
            }
            else
            {
                MessageBox.Show("No file name input. Please try again.");
            }
        }
        #endregion
        // SideBar Two
        #region
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            // code ripped from: http://stackoverflow.com/questions/4773632/how-do-i-restart-a-wpf-application

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void SaveImages_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Title = "Choose directory and name of (144) images to save";
            SFD.Filter = "png files (*.png)|*.png";
            SFD.FilterIndex = 0;
            SFD.ShowDialog();

            string path = SFD.FileName;

            if (path != "")
            {
                Mouse.OverrideCursor = Cursors.Wait;

                path = path.Remove(path.IndexOf('.')); // removes .png extension , will be added later
                try
                {
                    SaveImagesToFile(TubeMap, path);
                }
                catch
                {
                    MessageBox.Show("File already open. Please close and try again.");
                    Mouse.OverrideCursor = null;
                    return;
                }

                SaveImages.Content = "Images Saved";
                SaveImages.IsEnabled = false;
                CanvasLabel.Content = "Station";
                Mouse.OverrideCursor = null;
            }
            else
            {
                MessageBox.Show("No file name input. Please try again.");
            }
        }

        private void SaveImagesToFile(Canvas c, string path)
        {
            // code ripped from: http://stackoverflow.com/questions/5959217/wpf-forcing-redraw-of-canvas

            Action emptyDelegate = delegate { };

            for (int i = 0; i < 144; i++)
            {
                b.TimeChanged(i);
                CanvasLabel.Content = GetTimeString(i);
                c.UpdateLayout();
                c.Dispatcher.Invoke(emptyDelegate, DispatcherPriority.Render);
                Save(c, path + i + ".png");
            }

            MessageBox.Show("Images saved");
        }

        private void Save(Canvas c, string fileName)
        {
            // code ripped from: https://jasonkemp.ca/blog/how-to-save-xaml-as-an-image/ 

            Rect rect = new Rect(c.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            rtb.Render(c);
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            using (Stream fileStream = File.Create(fileName))
            {
                pngEncoder.Save(fileStream);
            }
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)TimeSlider.Value;
            b.TimeChanged(val);
            TimeBox.Text = GetTimeString(val);
        }

        private string GetTimeString(int tensOfMins)
        {
            int h = tensOfMins / 6;
            int m = (tensOfMins % 6) * 10;
            string time = h + ":" + m;

            if (h < 10)
            {
                time = "0" + time;
            }

            if (m == 0)
            {
                time = time + "0";
            }

            return time;
        }
        #endregion
        // Legend
        #region
        private void DrawLegend()
        {
            int divisions = 100;
            int width = 200;
            int height = 20;
            int padding = 10;

            for (int i = 0; i <= divisions; i++)
            {
                AddRectangle(i, divisions, width, height, padding, TubeMap);
            }

            int maxValue = b.GetMaxLoading();
            int powOfTen;

            for (int i = 0; i >= 0; i++)
            {
                powOfTen = (int)Math.Pow(10, i);

                if (powOfTen < maxValue)
                {
                    AddMarker(powOfTen, Math.Log(powOfTen) / Math.Log(maxValue), width, height, padding, TubeMap);
                }

                else
                {
                    break;
                }
            }

        }

        private void AddRectangle(int progress, int max, double totalWidth, double totalHeight, double padding, Canvas c)
        {

            Rectangle rect = new Rectangle();
            c.Children.Add(rect);

            Canvas.SetLeft(rect, padding + progress * totalWidth / max);
            Canvas.SetBottom(rect, padding);

            rect.Fill = new SolidColorBrush(HSVtoRGB.Convert((float)progress / (float)max, 1.0f, 1.0f, 1.0f));
            rect.Width = totalWidth / max;
            rect.Height = totalHeight;
        }

        private void AddMarker(int content, double along, int totalWidth, int totalHeight, int padding, Canvas C)
        {
            Label lab = new Label();
            C.Children.Add(lab);

            Canvas.SetLeft(lab, along * totalWidth);
            Canvas.SetBottom(lab, 1.1 * padding + totalHeight);

            lab.Content = content;
        }
        #endregion
    }
}
