using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CSLab3.Models;
using CSLab3.Interfaces;

namespace CSLab3.ViewModels
{
    public partial class MainForm : Form
    {
        private MainViewModel _viewModel;
        private TextBox _logTextBox;
        private Button _addFurnaceButton;
        private Button _addWorkerButton;
        private Button _addLoaderButton;
        private Button _startStopButton;
        private bool _isSimulationRunning = false;
        private System.Windows.Forms.Timer _animationTimer;
        private System.Windows.Forms.Timer _logUpdateTimer;
        private Panel _animationPanel;
        
        // Event handlers for furnace
        private void Furnace_MaterialDepleted(object sender, EventArgs e)
        {
            _viewModel.Furnace_MaterialDepleted(sender, e);
        }
        
        private void Furnace_Overheat(object sender, EventArgs e)
        {
            _viewModel.Furnace_Overheat(sender, e);
        }
        
        private void Furnace_StatusChanged(object sender, string e)
        {
            _viewModel.Furnace_StatusChanged(sender, e);
        }
        
        private void Furnace_TemperatureChanged(object sender, int e)
        {
            _viewModel.Furnace_TemperatureChanged(sender, e);
        }
        
        private void Worker_WorkPerformed(object sender, string e)
        {
            _viewModel.Worker_WorkPerformed(sender, e);
        }
        
        private void Worker_StatusChanged(object sender, string e)
        {
            _viewModel.Worker_StatusChanged(sender, e);
        }
        
        private void Loader_MaterialLoaded(object sender, EventArgs e)
        {
            _viewModel.Loader_MaterialLoaded(sender, e);
        }
        
        private void Loader_LoadingStatusChanged(object sender, string e)
        {
            _viewModel.Loader_LoadingStatusChanged(sender, e);
        }
        
        public MainForm()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeUI();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 800);
            this.Text = "Симуляция доменной печи";
            this.Name = "MainForm";
            
            this.ResumeLayout(false);
        }
        
        private void InitializeUI()
        {
            _animationPanel = new Panel();
            _animationPanel.Location = new System.Drawing.Point(10, 10);
            _animationPanel.Size = new System.Drawing.Size(600, 450);
            _animationPanel.BorderStyle = BorderStyle.FixedSingle;
            _animationPanel.Paint += AnimationPanel_Paint;
            this.Controls.Add(_animationPanel);
            
            _logUpdateTimer = new System.Windows.Forms.Timer();
            _logUpdateTimer.Interval = 100;
            _logUpdateTimer.Tick += LogUpdateTimer_Tick;
            _logUpdateTimer.Start();
            
            _logTextBox = new TextBox();
            _logTextBox.Location = new System.Drawing.Point(10, 470);
            _logTextBox.Size = new System.Drawing.Size(980, 300);
            _logTextBox.Multiline = true;
            _logTextBox.ScrollBars = ScrollBars.Vertical;
            _logTextBox.ReadOnly = true;
            this.Controls.Add(_logTextBox);
            
            _addFurnaceButton = new Button();
            _addFurnaceButton.Location = new System.Drawing.Point(620, 10);
            _addFurnaceButton.Size = new System.Drawing.Size(150, 30);
            _addFurnaceButton.Text = "Добавить печь";
            _addFurnaceButton.Click += AddFurnaceButton_Click;
            this.Controls.Add(_addFurnaceButton);
            
            _addWorkerButton = new Button();
            _addWorkerButton.Location = new System.Drawing.Point(620, 50);
            _addWorkerButton.Size = new System.Drawing.Size(150, 30);
            _addWorkerButton.Text = "Добавить рабочего";
            _addWorkerButton.Click += AddWorkerButton_Click;
            this.Controls.Add(_addWorkerButton);
            
            _addLoaderButton = new Button();
            _addLoaderButton.Location = new System.Drawing.Point(620, 90);
            _addLoaderButton.Size = new System.Drawing.Size(150, 30);
            _addLoaderButton.Text = "Добавить загрузчик";
            _addLoaderButton.Click += AddLoaderButton_Click;
            this.Controls.Add(_addLoaderButton);
            
            _startStopButton = new Button();
            _startStopButton.Location = new System.Drawing.Point(620, 130);
            _startStopButton.Size = new System.Drawing.Size(150, 30);
            _startStopButton.Text = "Запустить симуляцию";
            _startStopButton.Click += StartStopButton_Click;
            this.Controls.Add(_startStopButton);
            
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 50;
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Stop(); // Start with animation stopped
            
            BindToViewModel();
        }
        
        private void BindToViewModel()
        {
            UpdateLogDisplay();
        }
        
        private void UpdateLogDisplay()
        {
            _logTextBox.Text = _viewModel.LogText;
            
            // Only auto-scroll to bottom when simulation is running
            if (_isSimulationRunning)
            {
                _logTextBox.SelectionStart = _logTextBox.Text.Length;
                _logTextBox.ScrollToCaret();
            }
        }
        
        private void AddFurnaceButton_Click(object sender, EventArgs e)
        {
            var furnaceNumber = _viewModel.Furnaces.Count + 1;
            var furnace = new BlastFurnace($"Печь #{furnaceNumber}");
            furnace.MaterialDepleted += Furnace_MaterialDepleted;
            furnace.Overheat += Furnace_Overheat;
            furnace.StatusChanged += Furnace_StatusChanged;
            furnace.TemperatureChanged += Furnace_TemperatureChanged;
            _viewModel.Furnaces.Add(furnace);
            furnace.Start();
            _viewModel.LogMessage($"Добавлена новая печь: {furnace.Name}");
        }
        
        private void AddWorkerButton_Click(object sender, EventArgs e)
        {
            var workerNumber = _viewModel.Workers.Count + 1;
            var random = new Random();
            var worker = new Worker($"Рабочий #{workerNumber}", random.Next(1, 10));
            worker.WorkPerformed += Worker_WorkPerformed;
            worker.StatusChanged += Worker_StatusChanged;
            _viewModel.Workers.Add(worker);
            worker.StartWork(_viewModel.Loaders.Count > 0 ? _viewModel.Loaders[0] : null);
            _viewModel.LogMessage($"Добавлен новый рабочий: {worker.Name}");
        }
        
        private void AddLoaderButton_Click(object sender, EventArgs e)
        {
            var loaderNumber = _viewModel.Loaders.Count + 1;
            var loader = new MaterialLoader($"Загрузчик #{loaderNumber}");
            loader.MaterialLoaded += Loader_MaterialLoaded;
            loader.LoadingStatusChanged += Loader_LoadingStatusChanged;
            _viewModel.Loaders.Add(loader);
            _viewModel.LogMessage($"Добавлен новый загрузчик: {loader.Name}");
        }
        
        private void StartStopButton_Click(object sender, EventArgs e)
        {
            _isSimulationRunning = !_isSimulationRunning;
            
            // Toggle all furnaces
            foreach (var furnace in _viewModel.Furnaces)
            {
                if (_isSimulationRunning)
                {
                    // Only start if not already running
                    if (!furnace.IsRunning)
                    {
                        furnace.Start();
                    }
                }
                else
                {
                    furnace.Stop();
                }
            }
            
            // Toggle all workers
            foreach (var worker in _viewModel.Workers)
            {
                if (_isSimulationRunning)
                {
                    // Only start if not already working
                    if (!worker.IsWorking)
                    {
                        worker.StartWork(_viewModel.Loaders.Count > 0 ? _viewModel.Loaders[0] : null);
                    }
                }
                else
                {
                    worker.StopWork();
                }
            }
            
            // Toggle all loaders
            foreach (var loader in _viewModel.Loaders)
            {
                if (_isSimulationRunning)
                {
                    // Only start if not already loading
                    if (!loader.IsLoading)
                    {
                        loader.StartLoading();
                    }
                }
                else
                {
                    loader.StopLoading();
                }
            }
            
            // Control animation timer based on simulation state
            if (_isSimulationRunning)
            {
                _animationTimer.Start();
            }
            else
            {
                _animationTimer.Stop();
            }
            
            _startStopButton.Text = _isSimulationRunning ? "Остановить симуляцию" : "Запустить симуляцию";
            _viewModel.LogMessage(_isSimulationRunning ? "Симуляция запущена" : "Симуляция остановлена");
        }
        
        private void LogUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateLogDisplay();
        }
        
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.LogText))
            {
                UpdateLogDisplay();
            }
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _animationPanel.Invalidate();
        }
        
        private void AnimationPanel_Paint(object sender, PaintEventArgs e)
        {
            DrawAnimatedElements(e.Graphics);
        }
        
        private void DrawAnimatedElements(Graphics g)
        {
            for (int i = 0; i < _viewModel.Furnaces.Count; i++)
            {
                var furnace = _viewModel.Furnaces[i];
                var x = 50 + (i % 3) * 150;
                var y = 50 + (i / 3) * 100;
                var width = 100;
                var height = 80;
                
                Color color = Color.Blue;
                if (furnace.Temperature > 1000) color = Color.Red;
                else if (furnace.Temperature > 800) color = Color.Orange;
                else if (furnace.Temperature > 500) color = Color.Yellow;
                
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x, y, width, height);
                }
                
                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawRectangle(pen, x, y, width, height);
                }
                
                g.DrawString($"{furnace.Name}", Font, Brushes.Black, x, y - 20);
                g.DrawString($"Темп: {furnace.Temperature}°C", Font, Brushes.Black, x, y + height + 5);
            }
            
            for (int i = 0; i < _viewModel.Workers.Count; i++)
            {
                var worker = _viewModel.Workers[i];
                var x = 50 + (i % 4) * 120;
                var y = 250 + (i / 4) * 80;
                var radius = 20;
                
                Color color = worker.IsWorking ? Color.Green : Color.Gray;
                
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, x, y, radius * 2, radius * 2);
                }
                
                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawEllipse(pen, x, y, radius * 2, radius * 2);
                }
                
                g.DrawString($"{worker.Name}", Font, Brushes.Black, x, y + radius * 2 + 5);
            }
            
            for (int i = 0; i < _viewModel.Loaders.Count; i++)
            {
                var loader = _viewModel.Loaders[i];
                var x = 50 + (i % 3) * 180;
                var y = 350;
                var size = 30;
                
                Color color = loader.IsLoading ? Color.Purple : Color.LightGray;
                
                Point[] points = {
                    new Point(x, y + size),
                    new Point(x + size, y + size),
                    new Point(x + size / 2, y)
                };
                
                using (var brush = new SolidBrush(color))
                {
                    g.FillPolygon(brush, points);
                }
                
                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawPolygon(pen, points);
                }
                
                g.DrawString($"{loader.Name}", Font, Brushes.Black, x, y + size + 10);
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
                
                _logUpdateTimer?.Stop();
                _logUpdateTimer?.Dispose();
                
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    _viewModel.Cleanup();
                }
            }
            
            base.Dispose(disposing);
        }
    }
}
