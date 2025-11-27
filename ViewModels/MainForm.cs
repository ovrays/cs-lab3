using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSLab3.Models;
using CSLab3.Interfaces;

namespace CSLab3.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<BlastFurnace> _furnaces;
        private ObservableCollection<Worker> _workers;
        private ObservableCollection<MaterialLoader> _loaders;
        private string _logText;
        
        public ObservableCollection<BlastFurnace> Furnaces
        {
            get => _furnaces;
            set
            {
                _furnaces = value;
                OnPropertyChanged(nameof(Furnaces));
            }
        }
        
        public ObservableCollection<Worker> Workers
        {
            get => _workers;
            set
            {
                _workers = value;
                OnPropertyChanged(nameof(Workers));
            }
        }
        
        public ObservableCollection<MaterialLoader> Loaders
        {
            get => _loaders;
            set
            {
                _loaders = value;
                OnPropertyChanged(nameof(Loaders));
            }
        }
        
        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }
        
        public MainViewModel()
        {
            _furnaces = new ObservableCollection<BlastFurnace>();
            _workers = new ObservableCollection<Worker>();
            _loaders = new ObservableCollection<MaterialLoader>();
            _logText = "";
            
            CreateInitialModels();
        }
        
        private void CreateInitialModels()
        {
            var furnace = new BlastFurnace("Печь #1");
            furnace.MaterialDepleted += Furnace_MaterialDepleted;
            furnace.Overheat += Furnace_Overheat;
            furnace.StatusChanged += Furnace_StatusChanged;
            furnace.TemperatureChanged += Furnace_TemperatureChanged;
            _furnaces.Add(furnace);
            
            var worker = new Worker("Иван Иванов", 5);
            worker.WorkPerformed += Worker_WorkPerformed;
            worker.StatusChanged += Worker_StatusChanged;
            _workers.Add(worker);
            
            var loader = new MaterialLoader("Загрузчик #1");
            loader.MaterialLoaded += Loader_MaterialLoaded;
            loader.LoadingStatusChanged += Loader_LoadingStatusChanged;
            _loaders.Add(loader);
            
            furnace.Start();
            worker.StartWork(loader);
        }
        
        private void Furnace_MaterialDepleted(object sender, EventArgs e)
        {
            var furnace = sender as BlastFurnace;
            LogMessage($"Внимание: {furnace?.Name} исчерпал материалы!");
            
            foreach (var worker in _workers)
            {
                worker.RespondToEvent("MaterialDepleted");
            }
        }
        
        private void Furnace_Overheat(object sender, EventArgs e)
        {
            var furnace = sender as BlastFurnace;
            LogMessage($"Внимание: {furnace?.Name} перегревается!");
            
            foreach (var worker in _workers)
            {
                worker.RespondToEvent("Overheat");
            }
        }
        
        private void Furnace_StatusChanged(object sender, string e)
        {
            var furnace = sender as BlastFurnace;
            LogMessage($"[{furnace?.Name}] {e}");
        }
        
        private void Furnace_TemperatureChanged(object sender, int e)
        {
        }
        
        private void Worker_WorkPerformed(object sender, string e)
        {
            var worker = sender as Worker;
            LogMessage($"[{worker?.Name}] Выполняет: {e}");
        }
        
        private void Worker_StatusChanged(object sender, string e)
        {
            var worker = sender as Worker;
            LogMessage($"[{worker?.Name}] {e}");
        }
        
        private void Loader_MaterialLoaded(object sender, EventArgs e)
        {
            var loader = sender as MaterialLoader;
            LogMessage($"[{loader?.Name}] Материалы успешно загружены");
            
            if (_furnaces.Any())
            {
                var random = new Random();
                var furnace = _furnaces[random.Next(_furnaces.Count)];
                furnace.AddMaterials(
                    random.Next(10, 50),
                    random.Next(10, 50),
                    random.Next(5, 25));
            }
        }
        
        private void Loader_LoadingStatusChanged(object sender, string e)
        {
            var loader = sender as MaterialLoader;
            LogMessage($"[{loader?.Name}] {e}");
        }
        
        private void LogMessage(string message)
        {
            LogText += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            OnPropertyChanged(nameof(LogText));
        }
        
        public void Cleanup()
        {
            foreach (var furnace in _furnaces)
            {
                furnace.Stop();
            }
            
            foreach (var worker in _workers)
            {
                worker.StopWork();
            }
            
            foreach (var loader in _loaders)
            {
                loader.StopLoading();
            }
        }
    }
    
    public partial class MainForm : Form
    {
        private MainViewModel _viewModel;
        private TextBox _logTextBox;
        private Button _addFurnaceButton;
        private Button _addWorkerButton;
        private Button _addLoaderButton;
        private System.Windows.Forms.Timer _animationTimer;
        private System.Windows.Forms.Timer _logUpdateTimer;
        private Panel _animationPanel;
        
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
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Text = "Симуляция доменной печи";
            this.Name = "MainForm";
            
            this.ResumeLayout(false);
        }
        
        private void InitializeUI()
        {
            _animationPanel = new Panel();
            _animationPanel.Location = new System.Drawing.Point(10, 10);
            _animationPanel.Size = new System.Drawing.Size(600, 400);
            _animationPanel.BorderStyle = BorderStyle.FixedSingle;
            _animationPanel.Paint += AnimationPanel_Paint;
            this.Controls.Add(_animationPanel);
            
            _logUpdateTimer = new System.Windows.Forms.Timer();
            _logUpdateTimer.Interval = 100;
            _logUpdateTimer.Tick += LogUpdateTimer_Tick;
            _logUpdateTimer.Start();
            
            _logTextBox = new TextBox();
            _logTextBox.Location = new System.Drawing.Point(10, 420);
            _logTextBox.Size = new System.Drawing.Size(980, 200);
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
            
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 50;
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
            
            BindToViewModel();
        }
        
        private void BindToViewModel()
        {
            UpdateLogDisplay();
        }
        
        private void UpdateLogDisplay()
        {
            _logTextBox.Text = _viewModel.LogText;
            _logTextBox.SelectionStart = _logTextBox.Text.Length;
            _logTextBox.ScrollToCaret();
        }
        
        private void AddFurnaceButton_Click(object sender, EventArgs e)
        {
            var furnaceNumber = _viewModel.Furnaces.Count + 1;
            var furnace = new BlastFurnace($"Печь #{furnaceNumber}");
            furnace.MaterialDepleted += _viewModel.Furnaces[0].MaterialDepleted;
            furnace.Overheat += _viewModel.Furnaces[0].Overheat;
            furnace.StatusChanged += _viewModel.Furnaces[0].StatusChanged;
            furnace.TemperatureChanged += _viewModel.Furnaces[0].TemperatureChanged;
            _viewModel.Furnaces.Add(furnace);
            furnace.Start();
            _viewModel.LogMessage($"Добавлена новая печь: {furnace.Name}");
        }
        
        private void AddWorkerButton_Click(object sender, EventArgs e)
        {
            var workerNumber = _viewModel.Workers.Count + 1;
            var random = new Random();
            var worker = new Worker($"Рабочий #{workerNumber}", random.Next(1, 10));
            worker.WorkPerformed += _viewModel.Workers[0].WorkPerformed;
            worker.StatusChanged += _viewModel.Workers[0].StatusChanged;
            _viewModel.Workers.Add(worker);
            worker.StartWork(null);
            _viewModel.LogMessage($"Добавлен новый рабочий: {worker.Name}");
        }
        
        private void AddLoaderButton_Click(object sender, EventArgs e)
        {
            var loaderNumber = _viewModel.Loaders.Count + 1;
            var loader = new MaterialLoader($"Загрузчик #{loaderNumber}");
            loader.MaterialLoaded += _viewModel.Loaders[0].MaterialLoaded;
            loader.LoadingStatusChanged += _viewModel.Loaders[0].LoadingStatusChanged;
            _viewModel.Loaders.Add(loader);
            _viewModel.LogMessage($"Добавлен новый загрузчик: {loader.Name}");
        }
        
        private void LogUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateLogDisplay();
        }
        
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
