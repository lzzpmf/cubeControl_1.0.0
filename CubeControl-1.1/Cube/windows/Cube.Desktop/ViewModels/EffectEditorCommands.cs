namespace Cube.Desktop.ViewModels;

public sealed class EffectEditorCommands
{
    public AsyncUiCommand Read { get; }
    public AsyncUiCommand Restore { get; }
    public AsyncUiCommand Import { get; }
    public AsyncUiCommand Export { get; }
    public AsyncUiCommand? Listen { get; }

    internal EffectEditorCommands(Func<Task> read, Func<Task> restore, Func<Task> import, Func<Task> export,
        Func<bool> canRead, Func<bool> canRestore, Func<bool> canImport, Func<bool> canExport,
        Action<Exception> failed, Func<Task>? listen = null, Func<bool>? canListen = null)
    {
        Read = new(read, canRead, failed);
        Restore = new(restore, canRestore, failed);
        Import = new(import, canImport, failed);
        Export = new(export, canExport, failed);
        if (listen != null && canListen != null) Listen = new(listen, canListen, failed);
    }
}
