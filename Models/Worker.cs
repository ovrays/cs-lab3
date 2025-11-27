using CSLab3.Interfaces;

namespace CSLab3.Models
{
    public class Worker
    {
        private readonly object _lockObject = new object();
        private bool _isWorking;
        private Random _random;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<string> WorkPerformed;
        public event EventHandler WorkCompleted;
        public event EventHandler<string> StatusChanged;

        public string Name { get; set; }
        public int ExperienceLevel { get; set; }
        public bool IsWorking 
        { 
            get => _isWorking;
            private set 
            { 
                _isWorking = value;
                OnStatusChanged(_isWorking ? $"{Name} начал работу" : $"{Name} закончил работу");
            }
        }

        public Worker(string name, int experienceLevel)
        {
            Name = name;
            ExperienceLevel = experienceLevel;
            _isWorking = false;
            _random = new Random();
        }

        public void StartWork(ILoader loader)
        {
            if (IsWorking) return;
            
            IsWorking = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(() => WorkAsync(loader, _cancellationTokenSource.Token));
        }

        public void StopWork()
        {
            if (!IsWorking) return;
            
            _cancellationTokenSource?.Cancel();
            IsWorking = false;
        }

        private async Task WorkAsync(ILoader loader, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsWorking)
            {
                try
                {
                    string workDescription = PerformWork();
                    OnWorkPerformed(workDescription);
                    
                    if (loader != null && !loader.IsLoading)
                    {
                        string[] materials = { "Железная руда", "Кокс", "Известняк" };
                        string materialType = materials[_random.Next(materials.Length)];
                        int quantity = _random.Next(10, 50);
                        loader.LoadMaterial(materialType, quantity);
                        loader.StartLoading();
                        OnStatusChanged($"{Name} запустил загрузчик {loader.Name} для загрузки {quantity} единиц {materialType}");
                    }
                    
                    int workDuration = Math.Max(2000, 5000 - (ExperienceLevel * 200));
                    await Task.Delay(workDuration, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"Ошибка во время работы: {ex.Message}");
                }
            }
            
            OnWorkCompleted();
        }

        private string PerformWork()
        {
            string[] workTypes = {
                "Проверка температуры печи",
                "Проверка уровня материалов",
                "Регулировка воздушного потока",
                "Очистка шлакового отверстия",
                "Контроль качества продукции",
                "Калибровка приборов",
                "Проверка оборудования безопасности"
            };
            
            int workIndex = _random.Next(workTypes.Length);
            return workTypes[workIndex];
        }

        public void RespondToEvent(string eventName)
        {
            string response = "";
            switch (eventName.ToLower())
            {
                case "materialdepleted":
                    response = $"{Name} реагирует на исчерпание материалов - запуск аварийной остановки";
                    break;
                case "overheat":
                    response = $"{Name} реагирует на перегрев - активация систем охлаждения";
                    break;
                default:
                    response = $"{Name} подтверждает событие: {eventName}";
                    break;
            }
            
            OnStatusChanged(response);
        }

        protected virtual void OnWorkPerformed(string description)
        {
            WorkPerformed?.Invoke(this, description);
        }

        protected virtual void OnWorkCompleted()
        {
            IsWorking = false;
            WorkCompleted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }

        public override string ToString()
        {
            return $"{Name} (Опыт: {ExperienceLevel}) - {(IsWorking ? "Работает" : "Свободен")}";
        }
    }
}
