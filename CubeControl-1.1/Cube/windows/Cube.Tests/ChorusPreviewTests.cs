using System.Text.Json;
using Cube.Protocol;

internal static class ChorusPreviewTests
{
    public static async Task Run(Action<bool, string> check)
    {
        const string identity = "53494E434F2D437562654261627920202020200113141516171819";
        var initial = ChorusParameters.Default;
        var target = initial with { Depth = 0.2f, Mix = 0.425f, Feedback = 0.1f, Waveform = 1, WetGain = 0.5f, CombinedGain = 0.5f };
        check(ChorusPreset.Import(ChorusPreset.Export(identity, target)).Parameters == target, "chorus file roundtrip includes all seven controls");
        foreach (var p in new[] { initial with { Depth = float.NaN }, initial with { Depth = 0.41f }, initial with { Step = 0 },
            initial with { Step = float.PositiveInfinity }, initial with { Mix = 1 }, initial with { Feedback = 0.11f },
            initial with { Feedback = -0.1f }, initial with { Waveform = 2 }, initial with { WetGain = 0.49f }, initial with { CombinedGain = 1.01f } })
        {
            bool invalid = false; try { p.Validate(); } catch (ArgumentOutOfRangeException) { invalid = true; }
            check(invalid, "chorus rejects ungrounded control range");
        }
        foreach (var json in new[] { ChorusPreset.Export(identity, initial).Replace(ChorusParameters.Mapping, "other"),
            ChorusPreset.Export(identity, initial).Replace("\"Depth\":", "\"Address\": 1600, \"Depth\":"),
            ChorusPreset.Export(identity, initial).Replace("\"Depth\": 0.4,", "") })
        {
            bool invalid = false; try { ChorusPreset.Import(json); } catch (Exception e) when (e is FormatException or JsonException) { invalid = true; }
            check(invalid, "chorus rejects incompatible, arbitrary or incomplete file");
        }
        using var fake = new ChorusPreviewConnection(); using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) };
        await session.Identify(); ChorusRecovery? saved = null;
        Task Keep(ChorusRecovery r) { saved = r; return Task.CompletedTask; }
        var recovery = await session.BeginChorusPreview(0, Keep);
        check(saved != null && fake.Writes == 0, "chorus baseline preserved before any mutation");
        var original = fake.Dsp.ToArray();
        recovery = await session.ApplyChorusPreview(recovery, target, Keep);
        check(ChorusParameters.Read(fake.Dsp) == target && fake.Bank[8] == 3 && fake.Writes == 6, "chorus controls independent of MOD");
        check(ChorusParameters.StableEquals(original, fake.Dsp, true) && fake.Addresses[^1] == 0x64C, "chorus preserves state and installs feedback last");
        await session.RestoreChorusPreview(recovery);
        check(ChorusParameters.Read(fake.Dsp) == initial && fake.Addresses[6] == 0x64C, "chorus restores original with feedback reduced first");
        recovery = recovery with { Previous = initial, Target = initial };
        fake.IgnoreWrites = true; bool rejected = false;
        try { await session.ApplyChorusPreview(recovery, target, Keep); } catch (IOException) { rejected = true; }
        check(rejected && saved!.Target == target, "chorus ACK without mutation rejected with recovery retained");
        fake.IgnoreWrites = false; await session.RestoreChorusPreview(saved!);
        recovery = await session.ApplyChorusPreview(recovery, target, Keep);
        fake.SetParameters(initial); int writes = fake.Writes; rejected = false;
        try { await session.ApplyChorusPreview(recovery, target, Keep); } catch (IOException) { rejected = true; }
        check(rejected && fake.Writes == writes, "chorus detects external reset before writing");
        await session.RestoreChorusPreview(recovery);
        fake.State[0] = 1; rejected = false;
        try { await session.RestoreChorusPreview(recovery); } catch (IOException) { rejected = true; }
        check(rejected && fake.Writes == writes, "chorus rejects different physical preset during recovery");
        fake.State[0] = 0;
        recovery = await session.BeginChorusPreview(0, Keep);
        fake.Dsp[0x658] = 1; rejected = false;
        try { await session.ApplyChorusPreview(recovery, target, Keep); } catch (IOException) { rejected = true; }
        check(rejected && fake.Writes == writes, "chorus protects filter coefficient at phaser phase offset");
        fake.Dsp[0x658] = 0;
        fake.FailAfterWrites = fake.Writes + 2; rejected = false;
        try { await session.ApplyChorusPreview(recovery, target, Keep); } catch (TimeoutException) { rejected = true; }
        check(rejected && session.IsFaulted && saved!.Target == target, "chorus partial transaction survives disconnect");
        fake.FailAfterWrites = int.MaxValue; fake.Silent = false;
        using var reconnected = new CubeSession(fake); await reconnected.Identify();
        var durable = JsonSerializer.Deserialize<ChorusRecovery>(JsonSerializer.Serialize(saved))!;
        await reconnected.RestoreChorusPreview(durable);
        check(ChorusParameters.StableEquals(fake.Dsp, original), "chorus recovery from serialized partial transaction");
        fake.BadSignature = true; using var incompatible = new CubeSession(fake); await incompatible.Identify(); writes = fake.Writes; rejected = false;
        try { await incompatible.BeginChorusPreview(0, Keep); } catch (NotSupportedException) { rejected = true; }
        check(rejected && fake.Writes == writes, "chorus incompatible firmware rejects preview");
        fake.BadSignature = false; fake.Dsp[0x678] = 1;
        using var filters = new CubeSession(fake); await filters.Identify(); rejected = false;
        try { await filters.BeginChorusPreview(0, Keep); } catch (FormatException) { rejected = true; }
        check(rejected && fake.Writes == writes, "chorus refuses unknown recursive filter coefficients");
        fake.Dsp[0x678] = 0; fake.BadReadHeader = true;
        using var badHeader = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) }; await badHeader.Identify(); rejected = false;
        try { await badHeader.BeginChorusPreview(0, Keep); } catch (TimeoutException) { rejected = true; }
        check(rejected && badHeader.IsFaulted && fake.Writes == writes, "chorus rejects mismatched read address");
        using var moving = new ChorusPreviewConnection { ChangePhysicalAfterWrites = 1 };
        using var changing = new CubeSession(moving); await changing.Identify();
        var movingRecovery = await changing.BeginChorusPreview(0, _ => Task.CompletedTask); rejected = false;
        try { await changing.ApplyChorusPreview(movingRecovery, target, _ => Task.CompletedTask); } catch (IOException) { rejected = true; }
        check(rejected && moving.Writes == 1, "chorus stops multi-field update when physical preset changes between writes");
    }
}
