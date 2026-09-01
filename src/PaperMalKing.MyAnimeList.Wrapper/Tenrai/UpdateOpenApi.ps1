# SPDX-License-Identifier: AGPL-3.0-or-later
# Copyright (C) 2021-2026 N0D4N

$ErrorActionPreference = 'Stop'
$stageDirectory = Join-Path $PSScriptRoot ".update-$([Guid]::NewGuid().ToString('N'))"
$backupDirectory = Join-Path $PSScriptRoot ".update-backup-$([Guid]::NewGuid().ToString('N'))"
$upstreamPath = Join-Path $stageDirectory 'upstream.openapi.json'
. (Join-Path $PSScriptRoot 'Generation.Common.ps1')

try {
    New-Item -ItemType Directory -Path $stageDirectory | Out-Null

    & dotnet tool restore --configfile (Join-Path $PSScriptRoot 'NuGet.Offline.config')
    Assert-NativeCommandSucceeded 'dotnet tool restore'

    Invoke-WebRequest -Uri 'https://api.tenrai.org/documentation/openapi.json' -OutFile $upstreamPath
    if ((Get-Item -LiteralPath $upstreamPath).Length -eq 0) {
        throw 'The downloaded Tenrai OpenAPI document is empty.'
    }

    & (Join-Path $PSScriptRoot 'Generate.ps1') -UpstreamPath $upstreamPath -OutputDirectory $stageDirectory -SkipToolRestore

    Publish-GeneratedFiles @(
        [PSCustomObject]@{
            Source = $upstreamPath
            Destination = Join-Path $PSScriptRoot 'upstream.openapi.json'
        },
        [PSCustomObject]@{
            Source = Join-Path $stageDirectory 'tenrai.openapi.json'
            Destination = Join-Path $PSScriptRoot 'tenrai.openapi.json'
        },
        [PSCustomObject]@{
            Source = Join-Path $stageDirectory 'TenraiClient.g.cs'
            Destination = Join-Path $PSScriptRoot 'TenraiClient.g.cs'
        }
    ) $backupDirectory
}
finally {
    Remove-Item -LiteralPath $stageDirectory, $backupDirectory -Recurse -Force -ErrorAction SilentlyContinue
}
