namespace Cinturon360.Web.Services.State;

/// <summary>
/// Scoped service for queuing and dismissing toast notifications within a Blazor circuit.
/// </summary>
public sealed class ToastService
{
    private readonly List<ToastMessage> _toasts = new();
    private int _nextId;

    public IReadOnlyList<ToastMessage> ActiveToasts => _toasts;

    public event Action? OnChange;

    public void Show(
        string message,
        string? title = null,
        ToastType type = ToastType.Info,
        string? actionLabel = null,
        EventCallback onAction = default,
        int autoDismissSeconds = 5)
    {
        var id = Interlocked.Increment(ref _nextId);
        var toast = new ToastMessage(id, message, title, type, actionLabel, onAction);
        _toasts.Add(toast);
        OnChange?.Invoke();

        if (autoDismissSeconds > 0)
        {
            _ = Task.Delay(TimeSpan.FromSeconds(autoDismissSeconds))
                    .ContinueWith(_ => Dismiss(id));
        }
    }

    public void Success(string message, string? title = null)
        => Show(message, title, ToastType.Success);

    public void Error(string message, string? title = null, int autoDismissSeconds = 0)
        => Show(message, title, ToastType.Error, autoDismissSeconds: autoDismissSeconds);

    public void Warning(string message, string? title = null)
        => Show(message, title, ToastType.Warning);

    public void Info(string message, string? title = null)
        => Show(message, title, ToastType.Info);

    public void Dismiss(int id)
    {
        var existing = _toasts.FirstOrDefault(t => t.Id == id);
        if (existing is null) return;
        _toasts.Remove(existing);
        OnChange?.Invoke();
    }
}

public sealed record ToastMessage(
    int Id,
    string Message,
    string? Title,
    ToastType Type,
    string? ActionLabel,
    EventCallback OnAction);

public enum ToastType { Info, Success, Warning, Error }
