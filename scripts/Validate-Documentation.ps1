param([string]$PackageRoot)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$guides = Join-Path $repo 'CubeControl-1.1\Cube\docs\usuario'
$manuals = Join-Path $repo 'CubeControl-1.1\Cube\windows\Cube.Desktop\Assets\Documents'
$catalogPath = Join-Path $guides 'catalogo.v1.json'
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($catalog.version -ne 1 -or -not $catalog.documents) { throw 'Catalogo documental invalido.' }
$ids = @{}
$resources = @{ 'Assets\Documents\catalogo.v1.json' = $catalogPath }
$paths = @{}
$categories = 'comecar', 'timbres', 'criadores', 'praticar', 'ajuda', 'manuais'
foreach ($entry in $catalog.documents) {
    if (-not $entry.id -or $ids.ContainsKey($entry.id) -or $entry.category -notin $categories) { throw 'ID/categoria documental invalido.' }
    $ids[$entry.id] = $true
    if (-not $entry.versions) { throw "Documento sem versoes: $($entry.id)" }
    $languages = @{}
    foreach ($item in $entry.versions) {
        if (-not $item.language -or $languages.ContainsKey($item.language) -or -not $item.title -or -not $item.summary -or
            $item.format -notin 'markdown', 'pdf' -or [IO.Path]::IsPathRooted($item.path)) { throw "Versao invalida: $($entry.id)" }
        $languages[$item.language] = $true
        if ($item.format -eq 'markdown') {
            if (-not $item.path.StartsWith('guias/') -or -not $item.path.EndsWith('.md')) { throw 'Caminho de guia invalido.' }
            $source = [IO.Path]::GetFullPath((Join-Path $guides $item.path.Substring(6)))
            $allowed = [IO.Path]::GetFullPath($guides) + '\'
        } else {
            if (-not $item.path.EndsWith('.pdf')) { throw 'Caminho de PDF invalido.' }
            $source = [IO.Path]::GetFullPath((Join-Path $manuals $item.path))
            $allowed = [IO.Path]::GetFullPath($manuals) + '\'
        }
        if (-not $source.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path -LiteralPath $source -PathType Leaf)) {
            throw "Documento ausente ou fora da raiz: $($item.path)"
        }
        $relative = 'Assets\Documents\' + $item.path.Replace('/', '\')
        if ($resources.ContainsKey($relative)) { throw 'Caminho documental duplicado.' }
        $resources[$relative] = $source
        $paths[$source] = $item
        if ($item.format -eq 'pdf') {
            $stream = [IO.File]::OpenRead($source)
            try { $header = New-Object byte[] 5; $null = $stream.Read($header, 0, 5) }
            finally { $stream.Dispose() }
            if ([Text.Encoding]::ASCII.GetString($header) -ne '%PDF-') { throw "PDF invalido: $source" }
        }
    }
    if ($entry.category -ne 'manuais' -and (-not $languages.ContainsKey('pt-BR') -or -not $languages.ContainsKey('en-US'))) {
        throw "Guia sem traducao: $($entry.id)"
    }
}
foreach ($source in $paths.Keys) {
    if ($paths[$source].format -ne 'markdown') { continue }
    $text = Get-Content -LiteralPath $source -Raw -Encoding UTF8
    foreach ($match in [regex]::Matches($text, '\]\(([^)]+)\)')) {
        $target = $match.Groups[1].Value.Trim('<', '>')
        if ($target -match '^(https?://|mailto:)') { continue }
        $parts = $target.Split('#', 2)
        $dest = if ($parts[0]) { [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $source) ([Uri]::UnescapeDataString($parts[0])))) } else { $source }
        if (-not $paths.ContainsKey($dest)) { throw "Link nao publicado: $source -> $target" }
        if ($parts.Length -eq 2 -and $parts[1]) {
            $anchors = @(); $seen = @{}
            foreach ($heading in [regex]::Matches((Get-Content -LiteralPath $dest -Raw -Encoding UTF8), '(?m)^#{1,6}\s+(.+?)\s*#*\r?$')) {
                $anchor = [regex]::Replace($heading.Groups[1].Value.ToLowerInvariant(), '[^\p{L}\p{N}_\-\s]', '').Replace(' ', '-')
                $count = if ($seen.ContainsKey($anchor)) { $seen[$anchor] } else { 0 }
                $seen[$anchor] = $count + 1
                if ($count -gt 0) { $anchor += "-$count" }
                $anchors += $anchor
            }
            if ([Uri]::UnescapeDataString($parts[1]) -notin $anchors) { throw "Ancora inexistente: $source -> $target" }
        }
    }
}
$manualRoot = Join-Path $repo 'CubeControl-1.1\Cube\docs\manual'
$manifest = Get-Content -LiteralPath (Join-Path $manualRoot 'sources.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($inputFile in $manifest.inputs.PSObject.Properties) {
    if ((Get-FileHash -LiteralPath (Join-Path $repo $inputFile.Name) -Algorithm SHA256).Hash -ne $inputFile.Value) {
        throw "Manual HTML desatualizado. Execute node scripts/Build-Manual.mjs: $($inputFile.Name)"
    }
}
[xml]$desktopProject = Get-Content -LiteralPath (Join-Path $repo 'CubeControl-1.1\Cube\windows\Cube.Desktop\Cube.Desktop.csproj') -Raw
if ($manifest.version -ne $desktopProject.SelectSingleNode('/Project/PropertyGroup/Version').InnerText) { throw 'Versao do manual desatualizada. Execute node scripts/Build-Manual.mjs.' }
foreach ($file in Get-ChildItem -LiteralPath $manualRoot -File -Recurse) {
    $resources['Assets\Documents\manual\' + $file.FullName.Substring($manualRoot.Length + 1)] = $file.FullName
}
if ($PackageRoot) {
    foreach ($relative in $resources.Keys) {
        $destination = Join-Path $PackageRoot $relative
        if (-not (Test-Path -LiteralPath $destination -PathType Leaf) -or
            (Get-FileHash -LiteralPath $resources[$relative]).Hash -ne (Get-FileHash -LiteralPath $destination).Hash) {
            throw "Documento ausente/diferente no pacote: $relative"
        }
    }
}
Write-Host "Documentacao validada: $($catalog.documents.Count) entradas, $($resources.Count) arquivos publicados."
