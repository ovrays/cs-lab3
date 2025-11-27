using CSLab3.Interfaces;

namespace CSLab3.Models
{
    public class MaterialLoader : ILoader
    {
        private readonly object _lockObject = new object();
        private bool _isLoading;
        private string _currentMaterial;
        private int _quantity;
        private Random _random;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler MaterialLoaded;
        public event EventHandler<string> LoadingStatusChanged;

        public string Name { get; set; }
        public bool IsLoading 
        { 
            get => _isLoading;
            private set 
            { 
                _isLoading = value;
                OnLoadingStatusChanged(_isLoading ? $"{Name} начал загрузку" : $"{Name} остановил загрузку");
            }
        }
        public string CurrentMaterial => _currentMaterial;
        public int Quantity => _quantity;

        public MaterialLoader(string name)
        {
            Name = name;
            _isLoading = false;
            _currentMaterial = "";
            _quantity = 0;
            _random = new Random();
        }

        public void LoadMaterial(string materialType, int quantity)
        {
            _currentMaterial = materialType;
            _quantity = quantity;
            OnLoadingStatusChanged($"{Name} готов к загрузке {quantity} единиц {materialType}");
        }

        public void StartLoading()
        {
            if (IsLoading) return;
            
            IsLoading = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(() => LoadAsync(_cancellationTokenSource.Token));
        }

        public void StopLoading()
        {
            if (!IsLoading) return;
            
            _cancellationTokenSource?.Cancel();
            IsLoading = false;
        }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            try
            {
                OnLoadingStatusChanged($"{Name} загружает {_quantity} единиц {_currentMaterial}");
                
                int loadingTime = _random.Next(3000, 8000);
                await Task.Delay(loadingTime, cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    OnMaterialLoaded();
                    OnLoadingStatusChanged($"{Name} завершил загрузку {_quantity} единиц {_currentMaterial}");
                }
            }
            catch (OperationCanceledException)
            {
                OnLoadingStatusChanged($"{Name} загрузка отменена");
            }
            catch (Exception ex)
            {
                OnLoadingStatusChanged($"Ошибка во время загрузки: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual void OnMaterialLoaded()
        {
            MaterialLoaded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLoadingStatusChanged(string message)
        {
            LoadingStatusChanged?.Invoke(this, message);
        }

        public override string ToString()
        {
            return $"{Name} - {(IsLoading ? $"Загружает {_currentMaterial}" : "Свободен")}";
        }
    }
}
