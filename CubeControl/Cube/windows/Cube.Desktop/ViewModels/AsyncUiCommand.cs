using System.Windows.Input;

namespace Cube.Desktop.ViewModels;

public sealed class AsyncUiCommand(Func<Task> execute, Func<bool> canExecute, Action<Exception> failed) : ICommand
{
    private bool running;
    public bool CanExecute(object? parameter) => !running && canExecute();
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public async void Execute(object? parameter) => await ExecuteAsync();

    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;
        running = true;
        CommandManager.InvalidateRequerySuggested();
        try { await execute(); }
        catch (Exception ex) { failed(ex); }
        finally
        {
            running = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
