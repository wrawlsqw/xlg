using MetX.Standard.Pipelines;

namespace MetX.Standard.Interfaces
{
    public interface IMessageBox
    {
        IGenerationHost Host { get; }
        MessageBoxResult Show(string message);
        MessageBoxResult Show(string message, string title);
        MessageBoxResult Show(string message, string title, MessageBoxChoices choices);
        MessageBoxResult Show(string message, string title, MessageBoxChoices choices, MessageBoxStatus status, MessageBoxDefault @default);
    }
}