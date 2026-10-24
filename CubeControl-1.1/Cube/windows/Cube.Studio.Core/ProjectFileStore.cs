using System.Text.Json;

namespace Cube.Studio.Core;

/// <summary>Portable project metadata. Audio bytes are never embedded or modified.</summary>
public static class ProjectFileStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const string Format = "cube-studio";
    public const string FileExtension = ".cubestudio.json";

    public static void Save(ProjectModel project, string path, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, "." + Path.GetFileName(fullPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        var relocated = new Dictionary<Guid, string>();
        try
        {
            // Copy owned media before replacing the manifest. Never delete sources: history may still refer to them.
            var sources = Sources(project).ToArray();
            var mediaDirectory = Path.Combine(directory, Path.GetFileNameWithoutExtension(fullPath) + ".media");
            foreach (var source in sources.Where(source => source.OwnsSource && !source.IsMissing).DistinctBy(source => source.MediaId))
            {
                cancellation.ThrowIfCancellationRequested();
                var extension = Path.GetExtension(source.SourcePath!);
                if (extension.Length is 0 or > 12) extension = ".wav";
                var destination = Path.Combine(mediaDirectory, source.MediaId.ToString("N") + extension);
                if (!Path.GetFullPath(source.SourcePath!).Equals(destination, StringComparison.OrdinalIgnoreCase))
                {
                    Directory.CreateDirectory(mediaDirectory);
                    var copying = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
                    try { CopyFile(source.SourcePath!, copying, cancellation); File.Move(copying, destination, true); }
                    finally { if (File.Exists(copying)) File.Delete(copying); }
                }
                relocated[source.MediaId] = destination;
            }
            var portable = Deserialize(Serialize(project));
            foreach (var source in Sources(portable))
                if (relocated.TryGetValue(source.MediaId, out var destination)) source.SourcePath = Path.GetRelativePath(directory, destination);
            File.WriteAllText(temporaryPath, Serialize(portable, false));
            cancellation.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullPath, true);
            foreach (var source in sources)
                if (relocated.TryGetValue(source.MediaId, out var destination)) source.SourcePath = destination;
        }
        finally { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
    }

    private static void CopyFile(string source, string destination, CancellationToken cancellation)
    {
        using var input = File.OpenRead(source);
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(65536);
        try
        {
            int count;
            while ((count = input.Read(buffer)) > 0)
            {
                cancellation.ThrowIfCancellationRequested();
                output.Write(buffer, 0, count);
            }
            output.Flush(true);
        }
        finally { System.Buffers.ArrayPool<byte>.Shared.Return(buffer); }
    }

    public static ProjectModel Load(string path)
    {
        if (new FileInfo(path).Length > 10 * 1024 * 1024) throw new InvalidDataException("O arquivo de projeto excede 10 MB.");
        return Deserialize(File.ReadAllText(path), Path.GetDirectoryName(Path.GetFullPath(path)));
    }

    public static string Serialize(ProjectModel project, bool forHistory = false)
    {
        var data = new ProjectData
        {
            Name = project.Name, Bpm = project.Bpm, ReferenceBpm = project.ReferenceBpm, ReferenceLocked = project.ReferenceLocked,
            Semitones = project.Semitones, Meter = project.Meter,
            MetronomeEnabled = project.MetronomeEnabled, MetronomeVolumeDb = project.MetronomeVolumeDb,
            MetronomeSound = project.MetronomeSound, LoopEnabled = project.LoopEnabled, SnapEnabled = project.SnapEnabled,
            LoopStart = project.LoopStart, LoopEnd = project.LoopEnd, Duration = project.Duration,
            Position = forHistory ? 0 : project.Position,
            SelectedTrackId = forHistory ? null : project.SelectedTrackId, SelectedClipId = forHistory ? null : project.SelectedClipId,
            Tracks = project.Tracks.Select(track => new TrackData
            {
                Id = track.Id, Name = track.Name, IsSolo = track.IsSolo, IsMuted = track.IsMuted, IsArmed = track.IsArmed,
                VolumeDb = track.VolumeDb, Pan = track.Pan, InputChannel = track.InputChannel, HeightWeight = track.HeightWeight,
                Clips = track.Clips.Select(clip =>
                {
                    var data = new ClipData {
                    Id = clip.Id, Name = clip.Name, Start = clip.Start, Duration = clip.Duration, SourceOffset = clip.SourceOffset,
                    SourceDuration = clip.SourceDuration, Seed = clip.Seed, SourcePath = clip.SourcePath, ActiveTakeIndex = clip.ActiveTakeIndex,
                    Takes = clip.Takes.Select(take => { var td = new TakeData { Id = take.Id, Name = take.Name, Seed = take.Seed, Duration = take.Duration }; CopyToData(take, td); return td; }).ToList()
                    };
                    CopyToData(clip, data); return data;
                }).ToList()
            }).ToList()
        };
        return JsonSerializer.Serialize(new ProjectFile { Format = Format, Version = 1, Project = data }, Options);
    }

    public static ProjectModel Deserialize(string json, string? baseDirectory = null)
    {
        var file = JsonSerializer.Deserialize<ProjectFile>(json, Options) ?? throw new InvalidDataException("Projeto vazio.");
        var legacy = file.Format == "mini-studio-shell";
        if ((file.Format != Format && !legacy) || file.Version != 1 || file.Project is null)
            throw new InvalidDataException("Formato ou versão de projeto incompatível.");
        var data = file.Project;
        if (data.Tracks is null || data.Tracks.Count > 4) throw new InvalidDataException("O projeto aceita até quatro tracks.");
        Range(data.Bpm, 30, 300, "BPM"); Range(data.ReferenceBpm, 30, 300, "BPM de referência");
        Range(data.Duration, 1, 86400, "Duração"); Range(data.Position, 0, data.Duration, "Posição");
        Range(data.LoopStart, 0, data.Duration, "Início do loop"); Range(data.LoopEnd, data.LoopStart + ClipModel.MinimumDuration, data.Duration, "Fim do loop");
        Range(data.MetronomeVolumeDb, -61, 6, "Volume do metrônomo");
        if (!ProjectModel.AvailableMeters.Contains(data.Meter) || !ProjectModel.AvailableMetronomeSounds.Contains(data.MetronomeSound))
            throw new InvalidDataException("Configuração de metrônomo inválida.");
        if (data.Semitones is < -12 or > 12) throw new InvalidDataException("Transposição inválida.");
        var project = new ProjectModel
        {
            Name = data.Name, Bpm = data.Bpm, ReferenceBpm = data.ReferenceBpm, Semitones = data.Semitones,
            Meter = data.Meter, MetronomeEnabled = data.MetronomeEnabled, MetronomeVolumeDb = data.MetronomeVolumeDb,
            MetronomeSound = data.MetronomeSound, LoopEnabled = data.LoopEnabled, SnapEnabled = data.SnapEnabled,
            Duration = data.Duration, LoopStart = data.LoopStart, LoopEnd = data.LoopEnd, Position = data.Position,
            SelectedTrackId = data.SelectedTrackId, SelectedClipId = data.SelectedClipId
        };
        var ids = new HashSet<Guid>();
        var armed = false;
        foreach (var td in data.Tracks)
        {
            if (td is null) throw new InvalidDataException("Track vazia.");
            Unique(td.Id, ids); Range(td.VolumeDb, -61, 6, "Volume"); Range(td.Pan, -100, 100, "Pan"); Range(td.HeightWeight, 0.5, 2, "Altura");
            if (string.IsNullOrWhiteSpace(td.InputChannel) || td.InputChannel.Length > 128) throw new InvalidDataException("Canal de entrada inválido.");
            if (td.IsArmed && armed) throw new InvalidDataException("Somente uma track pode estar armada.");
            armed |= td.IsArmed;
            if (td.Clips is null || td.Clips.Count > 10000) throw new InvalidDataException("Lista de clipes inválida.");
            var track = new TrackModel { Id = td.Id, Name = td.Name, IsSolo = td.IsSolo, IsMuted = td.IsMuted, IsArmed = td.IsArmed, VolumeDb = td.VolumeDb, Pan = td.Pan, InputChannel = td.InputChannel, HeightWeight = td.HeightWeight };
            foreach (var cd in td.Clips)
            {
                if (cd is null) throw new InvalidDataException("Clipe vazio.");
                Unique(cd.Id, ids); Range(cd.Start, 0, data.Duration, "Início do clipe");
                Range(cd.Duration, ClipModel.MinimumDuration, data.Duration, "Duração do clipe");
                Range(cd.SourceDuration, ClipModel.MinimumDuration, 86400, "Duração da fonte");
                Range(cd.SourceOffset, 0, cd.SourceDuration - ClipModel.MinimumDuration, "Offset da fonte");
                if (cd.Start + cd.Duration > data.Duration + 0.001 || cd.SourceOffset + cd.Duration > cd.SourceDuration + 0.001)
                    throw new InvalidDataException("Clipe fora dos limites da fonte ou da sessão.");
                if (cd.Takes is null || cd.Takes.Count > 1000 || cd.ActiveTakeIndex < 0 || cd.ActiveTakeIndex >= Math.Max(1, cd.Takes.Count))
                    throw new InvalidDataException("Lista de takes inválida.");
                var clip = new ClipModel { Id = cd.Id, Name = cd.Name, Start = cd.Start, Duration = cd.Duration, SourceOffset = cd.SourceOffset, SourceDuration = cd.SourceDuration, Seed = cd.Seed, SourcePath = cd.SourcePath };
                CopyFromData(cd, clip, baseDirectory, legacy);
                foreach (var take in cd.Takes)
                {
                    if (take is null) throw new InvalidDataException("Take vazio.");
                    Unique(take.Id, ids); Range(take.Duration, ClipModel.MinimumDuration, 86400, "Duração do take");
                    var model = new TakeModel { Id = take.Id, Name = take.Name, Seed = take.Seed, Duration = take.Duration };
                    CopyFromData(take, model, baseDirectory, legacy);
                    ValidateSourceDuration(model, take.Duration);
                    clip.Takes.Add(model);
                }
                clip.ActiveTakeIndex = cd.ActiveTakeIndex;
                ValidateSourceDuration(clip.ActiveSource, clip.SourceDuration);
                if (clip.ActiveTake is { } active && clip.SourceDuration > active.Duration + .001)
                    throw new InvalidDataException("Clipe excede a fonte do take ativo.");
                track.Clips.Add(clip);
            }
            project.Tracks.Add(track);
        }
        while (project.Tracks.Count < 4) project.Tracks.Add(new TrackModel { Name = "Track " + (project.Tracks.Count + 1) });
        var selectedTrack = project.Tracks.FirstOrDefault(t => t.Id == project.SelectedTrackId);
        if (selectedTrack is null) { project.SelectedTrackId = project.Tracks[0].Id; project.SelectedClipId = null; }
        else if (selectedTrack.Clips.All(c => c.Id != project.SelectedClipId)) project.SelectedClipId = null;
        foreach (var clip in project.Tracks.SelectMany(t => t.Clips)) clip.IsSelected = clip.Id == project.SelectedClipId;
        project.ReferenceLocked = data.ReferenceLocked || project.Tracks.Any(track => track.Clips.Count > 0);
        return project;
    }

    private static IEnumerable<AudioSourceModel> Sources(ProjectModel project) => project.Tracks.SelectMany(track => track.Clips)
        .SelectMany(clip => new AudioSourceModel[] { clip }.Concat(clip.Takes));

    private static void CopyToData(AudioSourceModel source, SourceData data)
    {
        data.MediaId = source.MediaId; data.SourcePath = source.SourcePath;
        data.SourceSampleRate = source.SourceSampleRate; data.SourceChannels = source.SourceChannels;
        data.SourceFrameCount = source.SourceFrameCount; data.OwnsSource = source.OwnsSource;
        data.RecordedSpeed = source.RecordedSpeed; data.RecordedSemitones = source.RecordedSemitones; data.ClockRatio = source.ClockRatio;
    }

    private static void CopyFromData(SourceData data, AudioSourceModel source, string? directory, bool legacy)
    {
        if (data.MediaId == Guid.Empty && !legacy) throw new InvalidDataException("Identificador de mídia vazio.");
        Range(data.RecordedSpeed, .1, 10, "Velocidade de captura");
        Range(data.ClockRatio, .5, 2, "Razão dos relógios de áudio");
        if (data.RecordedSemitones is < -12 or > 12 || data.SourceSampleRate is < 0 or > 384000
            || data.SourceChannels is < 0 or > 64 || data.SourceFrameCount < 0
            || (data.SourceFrameCount > 0 && (data.SourceSampleRate < 8000 || data.SourceChannels == 0)))
            throw new InvalidDataException("Metadados de mídia inválidos.");
        source.MediaId = data.MediaId == Guid.Empty ? Guid.NewGuid() : data.MediaId;
        source.SourcePath = data.SourcePath;
        if (!string.IsNullOrWhiteSpace(source.SourcePath) && directory is not null && !Path.IsPathRooted(source.SourcePath))
            source.SourcePath = Path.GetFullPath(Path.Combine(directory, source.SourcePath));
        source.SourceSampleRate = data.SourceSampleRate; source.SourceChannels = data.SourceChannels;
        source.SourceFrameCount = data.SourceFrameCount; source.OwnsSource = data.OwnsSource;
        source.RecordedSpeed = data.RecordedSpeed; source.RecordedSemitones = data.RecordedSemitones; source.ClockRatio = data.ClockRatio;
    }

    private static void Range(double value, double min, double max, string name)
    {
        if (!double.IsFinite(value) || value < min - 0.000001 || value > max + 0.000001)
            throw new InvalidDataException(name + " fora dos limites permitidos.");
    }
    private static void ValidateSourceDuration(AudioSourceModel source, double duration)
    {
        if (source.SourceFrameCount > 0
            && duration > source.AudioDuration * source.RecordedSpeed * source.ClockRatio + .001)
            throw new InvalidDataException("Duração excede os frames da fonte de áudio.");
    }
    private static void Unique(Guid id, HashSet<Guid> ids)
    {
        if (id == Guid.Empty || !ids.Add(id)) throw new InvalidDataException("Identificador duplicado ou vazio.");
    }

    private sealed class ProjectFile
    {
        public ProjectFile() { }
        public string Format { get; set; } = "";
        public int Version { get; set; }
        public ProjectData? Project { get; set; }
    }
    private sealed class ProjectData
    {
        public ProjectData() { }
        public string Name { get; set; } = "Sem título";
        public double Bpm { get; set; } = 100;
        public double ReferenceBpm { get; set; } = 100;
        public bool ReferenceLocked { get; set; }
        public int Semitones { get; set; }
        public string Meter { get; set; } = "4/4";
        public bool MetronomeEnabled { get; set; }
        public double MetronomeVolumeDb { get; set; } = -12;
        public string MetronomeSound { get; set; } = "Click";
        public bool LoopEnabled { get; set; }
        public bool SnapEnabled { get; set; } = true;
        public double LoopStart { get; set; }
        public double LoopEnd { get; set; } = 80;
        public double Duration { get; set; } = 80;
        public double Position { get; set; }
        public Guid? SelectedTrackId { get; set; }
        public Guid? SelectedClipId { get; set; }
        public List<TrackData> Tracks { get; set; } = [];
    }
    private sealed class TrackData
    {
        public TrackData() { }
        public Guid Id { get; set; }
        public string Name { get; set; } = "Track";
        public bool IsSolo { get; set; }
        public bool IsMuted { get; set; }
        public bool IsArmed { get; set; }
        public double VolumeDb { get; set; }
        public double Pan { get; set; }
        public string InputChannel { get; set; } = "Nenhuma";
        public double HeightWeight { get; set; } = 1;
        public List<ClipData> Clips { get; set; } = [];
    }
    private class SourceData
    {
        public Guid MediaId { get; set; }
        public string? SourcePath { get; set; }
        public int SourceSampleRate { get; set; }
        public int SourceChannels { get; set; }
        public long SourceFrameCount { get; set; }
        public bool OwnsSource { get; set; }
        public double RecordedSpeed { get; set; } = 1;
        public double ClockRatio { get; set; } = 1;
        public int RecordedSemitones { get; set; }
    }
    private sealed class ClipData : SourceData
    {
        public ClipData() { }
        public Guid Id { get; set; }
        public string Name { get; set; } = "Clipe";
        public double Start { get; set; }
        public double Duration { get; set; } = 10;
        public double SourceOffset { get; set; }
        public double SourceDuration { get; set; } = 80;
        public int Seed { get; set; } = 1;
        public int ActiveTakeIndex { get; set; }
        public List<TakeData> Takes { get; set; } = [];
    }
    private sealed class TakeData : SourceData
    {
        public TakeData() { }
        public Guid Id { get; set; }
        public string Name { get; set; } = "Take";
        public int Seed { get; set; } = 1;
        public double Duration { get; set; } = 10;
    }
}
