namespace Cube.Protocol;

public readonly record struct FootswitchState(byte Slot, byte Mode)
{
    public int? PresetIndex => Mode == 1 && Slot <= 2 ? Slot : null;
}

public sealed partial class CubeSession
{
    private bool? footswitchSupported;

    // Experimental, read-only mapping from cubebaby_A_20220706.up.
    // Never assume that a device name alone establishes firmware compatibility.
    public async Task<FootswitchState?> ReadFootswitch(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            if (footswitchSupported == false) return null;
            if (footswitchSupported == null)
            {
                var signature = await ReadInternal(0xFFFFDDEC, 12, ct, 4);
                if (!signature.SequenceEqual(Convert.FromHexString("014B194436F4B6B3B8CB8000")))
                {
                    footswitchSupported = false;
                    Log?.Invoke("Sincronização dos footswitches indisponível: firmware não reconhecido.");
                    return null;
                }
                var bank = await ReadInternal(0, 48, ct);
                var alias = await ReadInternal(0xFFFD8558, 48, ct, 4);
                footswitchSupported = bank.SequenceEqual(alias);
                if (footswitchSupported == false) return null;
            }
            var state = await ReadInternal(0xFFFD84B2, 2, ct, 4);
            return new(state[0], state[1]);
        }
        finally { gate.Release(); }
    }
}
