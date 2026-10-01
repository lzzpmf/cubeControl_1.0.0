namespace Cube.Protocol;

public readonly record struct ParameterKey(int Slot, int Offset);

// The worker retains the caller's synchronization context; pending values are thread-safe.
public sealed class LiveEditQueue(Func<ParameterKey, int, Task> send)
{
    private readonly Dictionary<string, Func<Task>> pending = [];
    private readonly LinkedList<string> order = [];
    private readonly object sync = new();
    private Task worker = Task.CompletedTask;
    private bool stopped, running;
    private Exception? failure;
    public event Action<Exception>? Failed;
    public bool IsRunning { get { lock (sync) return running; } }

    public void Enqueue(ParameterKey key, int value)
        => EnqueueAction($"parameter:{key.Slot}:{key.Offset}", () => send(key, value));

    public void EnqueueAction(string key, Func<Task> action)
    {
        lock (sync)
        {
            if (stopped) throw new InvalidOperationException("Fila encerrada. Reconecte o aparelho.", failure);
            if (!pending.ContainsKey(key)) order.AddLast(key);
            pending[key] = action;
            if (!running) { running = true; worker = Pump(); }
        }
    }

    private async Task Pump()
    {
        // Yield once so the worker is assigned before processing synchronous replies.
        await Task.Yield();
        while (true)
        {
            KeyValuePair<string, Func<Task>> item;
            lock (sync)
            {
                if (stopped || pending.Count == 0) { running = false; return; }
                string key = order.First!.Value; order.RemoveFirst();
                item = new(key, pending[key]); pending.Remove(key);
            }
            try { await item.Value(); }
            catch (Exception ex)
            {
                lock (sync) { failure = ex; stopped = true; pending.Clear(); order.Clear(); running = false; }
                Failed?.Invoke(ex); return;
            }
        }
    }

    public async Task Flush()
    {
        while (true)
        {
            Task current; lock (sync) current = worker;
            await current;
            lock (sync)
            {
                if (failure != null) throw new IOException("Envio interrompido. A edição local foi preservada.", failure);
                if (!running) return;
            }
        }
    }
    public void Stop() { lock (sync) { stopped = true; pending.Clear(); order.Clear(); } }
}
