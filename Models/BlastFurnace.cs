using System;
using System.Threading;
using System.Threading.Tasks;
using CSLab3.Interfaces;

namespace CSLab3.Models
{
    public class BlastFurnace
    {
        private readonly object _lockObject = new object();
        private int _ironOre;
        private int _coke;
        private int _limestone;
        private int _temperature;
        private bool _isRunning;
        private Random _random;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler MaterialDepleted;
        public event EventHandler Overheat;
        public event EventHandler<string> StatusChanged;
        public event EventHandler<int> TemperatureChanged;

        public string Name { get; set; }
        public int IronOre 
        { 
            get => _ironOre;
            private set 
            { 
                _ironOre = value;
                OnStatusChanged($"Железная руда: {_ironOre}");
            }
        }
        public int Coke 
        { 
            get => _coke;
            private set 
            { 
                _coke = value;
                OnStatusChanged($"Кокс: {_coke}");
            }
        }
        public int Limestone 
        { 
            get => _limestone;
            private set 
            { 
                _limestone = value;
                OnStatusChanged($"Известняк: {_limestone}");
            }
        }
        public int Temperature 
        { 
            get => _temperature;
            private set 
            {
                _temperature = value;
                OnTemperatureChanged(_temperature);
                OnStatusChanged($"Температура: {_temperature}°C");
            }
        }
        public bool IsRunning 
        { 
            get => _isRunning;
            private set 
            { 
                _isRunning = value;
                OnStatusChanged(_isRunning ? "Печь запущена" : "Печь остановлена");
            }
        }

        public BlastFurnace(string name)
        {
            Name = name;
            _ironOre = 100;
            _coke = 100;
            _limestone = 50;
            _temperature = 20;
            _isRunning = false;
            _random = new Random();
        }

        public void Start()
        {
            if (IsRunning) return;
            
            IsRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(() => RunSimulation(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (!IsRunning) return;
            
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
        }

        private async Task RunSimulation(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsRunning)
            {
                try
                {
                    lock (_lockObject)
                    {
                        if (IronOre > 0) IronOre--;
                        if (Coke > 0) Coke--;
                        if (Limestone > 0) Limestone--;

                        if (Coke > 0)
                        {
                            Temperature += _random.Next(1, 3);
                        }
                        else
                        {
                            Temperature = Math.Max(20, Temperature - 1);
                        }
                    }

                    CheckMaterialLevels();

                    CheckOverheat();

                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"Ошибка в симуляции: {ex.Message}");
                }
            }
        }

        private void CheckMaterialLevels()
        {
            lock (_lockObject)
            {
                if (IronOre <= 0 || Coke <= 0 || Limestone <= 0)
                {
                    OnMaterialDepleted();
                }
            }
        }

        private void CheckOverheat()
        {
            if (Temperature > 1200 && _random.NextDouble() < 0.05)
            {
                OnOverheat();
            }
        }

        public void AddMaterials(int ironOre, int coke, int limestone)
        {
            lock (_lockObject)
            {
                IronOre += ironOre;
                Coke += coke;
                Limestone += limestone;
            }
            
            OnStatusChanged($"Добавлены материалы: Железная руда={ironOre}, Кокс={coke}, Известняк={limestone}");
        }

        protected virtual void OnMaterialDepleted()
        {
            IsRunning = false;
            MaterialDepleted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnOverheat()
        {
            Overheat?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }

        protected virtual void OnTemperatureChanged(int temperature)
        {
            TemperatureChanged?.Invoke(this, temperature);
        }

        public override string ToString()
        {
            return $"{Name}: Железная руда={IronOre}, Кокс={Coke}, Известняк={Limestone}, Темп={Temperature}°C";
        }
    }
}
