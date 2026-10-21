from pathlib import Path
p=Path('CubeControl-1.1/Cube/windows/Cube.Desktop.Tests/BackingDawTests.cs')
s=p.read_text(encoding='utf-8-sig')
a=s.index('            window.GetType().GetMethod("DetachSessionMixer"');b=s.index('            await VerifyLayout(window, reports);',a)
s=s[:a]+s[b:]
a=s.index('    private static async Task VerifyLayout(');b=s.index('    private static async Task CapturePackets(',a)
s=s[:a]+'''    private static async Task VerifyLayout(MainWindow window, string reports)
    {
        await PlayerLayoutTests.VerifyStudio(window, reports);
    }
'''+s[b:]
p.write_text(s,encoding='utf-8')
