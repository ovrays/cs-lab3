using System;

namespace CSLab3.Interfaces
{
    public interface ILoader
    {
        event EventHandler MaterialLoaded;
        string Name { get; }
        bool IsLoading { get; }
        void LoadMaterial(string materialType, int quantity);
        void StartLoading();
        void StopLoading();
    }
}
