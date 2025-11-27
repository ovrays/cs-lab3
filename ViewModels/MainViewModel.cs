using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        
        public void Furnace_MaterialDepleted(object sender, EventArgs e)
        {
            var furnace = sender as BlastFurnace;
            LogMessage($"Внимание: {furnace?.Name} исчерпал материалы!");
            
            foreach (var worker in _workers)
            {
                worker.RespondToEvent("MaterialDepleted");
            }
        }
        
        public void Furnace_Overheat(object sender, EventArgs e)
        {
            var furnace = sender as BlastFurnace;
            LogMessage($"Внимание: {furnace?.Name} перегревается!");
            
            foreach (var worker in _workers)
            {
                worker.RespondToEvent("Overheat");
            }
        }
        
        public void Furnace_StatusChanged(object sender, string e)
        {
            var furnace = sender as BlastFurnace;
            LogMessage($"[{furnace?.Name}] {e}");
        }
        
        public void Furnace_TemperatureChanged(object sender, int e)
        {
        }
        
        public void Worker_WorkPerformed(object sender, string e)
        {
            var worker = sender as Worker;
            LogMessage($"[{worker?.Name}] Выполняет: {e}");
        }
        
        public void Worker_StatusChanged(object sender, string e)
        {
            var worker = sender as Worker;
            LogMessage($"[{worker?.Name}] {e}");
        }
        
        public void Loader_MaterialLoaded(object sender, EventArgs e)
        {
            var loader = sender as MaterialLoader;
            LogMessage($"[{loader?.Name}] Материалы успешно загружены");
            
            // Load specific amounts to the most appropriate furnace
            if (_furnaces.Any())
            {
                // Find the furnace that needs this material the most
                var targetFurnace = SelectAppropriateFurnace(loader);
                
                if (targetFurnace != null)
                {
                    // Load specific amounts based on what the loader was set to load
                    if (loader != null)
                    {
                        switch (loader.CurrentMaterial)
                        {
                            case "Железная руда":
                                targetFurnace.AddMaterials(loader.Quantity, 0, 0);
                                break;
                            case "Кокс":
                                targetFurnace.AddMaterials(0, loader.Quantity, 0);
                                break;
                            case "Известняк":
                                targetFurnace.AddMaterials(0, 0, loader.Quantity);
                                break;
                            default:
                                // Load balanced amounts if material type is unknown
                                targetFurnace.AddMaterials(
                                    loader.Quantity / 3,
                                    loader.Quantity / 3,
                                    loader.Quantity / 3);
                                break;
                        }
                    }
                    else
                    {
                        // Fallback to random amounts if loader is null
                        targetFurnace.AddMaterials(
                            20,
                            20,
                            10);
                    }
                }
            }
        }
        
        private BlastFurnace SelectAppropriateFurnace(MaterialLoader loader)
        {
            if (loader == null || !_furnaces.Any())
                return null;
            
            // Find furnace with lowest level of the required material
            BlastFurnace targetFurnace = null;
            
            switch (loader.CurrentMaterial)
            {
                case "Железная руда":
                    targetFurnace = _furnaces.OrderBy(f => f.IronOre).FirstOrDefault();
                    break;
                case "Кокс":
                    targetFurnace = _furnaces.OrderBy(f => f.Coke).FirstOrDefault();
                    break;
                case "Известняк":
                    targetFurnace = _furnaces.OrderBy(f => f.Limestone).FirstOrDefault();
                    break;
                default:
                    // If material type is unknown, use the first furnace
                    targetFurnace = _furnaces.FirstOrDefault();
                    break;
            }
            
            return targetFurnace;
        }
        
        public void Loader_LoadingStatusChanged(object sender, string e)
        {
            var loader = sender as MaterialLoader;
            LogMessage($"[{loader?.Name}] {e}");
        }
        
        public void LogMessage(string message)
        {
            LogText += $"\r\n[{DateTime.Now:HH:mm:ss}] {message}";
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
}
