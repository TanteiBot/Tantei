# SPDX-License-Identifier: AGPL-3.0-or-later
# Copyright (C) 2021-2026 N0D4N

[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot 'upstream.openapi.json'),
    [string] $OutputDirectory = $PSScriptRoot,
    [switch] $SkipToolRestore
)

$ErrorActionPreference = 'Stop'
$stageDirectory = Join-Path $PSScriptRoot ".generate-$([Guid]::NewGuid().ToString('N'))"
$backupDirectory = Join-Path $PSScriptRoot ".generate-backup-$([Guid]::NewGuid().ToString('N'))"
$projectorPath = Join-Path $PSScriptRoot 'ProjectOpenApi.cs'
$projectedPath = Join-Path $stageDirectory 'tenrai.openapi.json'
$clientPath = Join-Path $stageDirectory 'TenraiClient.g.cs'
. (Join-Path $PSScriptRoot 'Generation.Common.ps1')

try {
    New-Item -ItemType Directory -Path $stageDirectory | Out-Null
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

    if (-not $SkipToolRestore) {
        & dotnet tool restore
        Assert-NativeCommandSucceeded 'dotnet tool restore'
    }

    & dotnet run $projectorPath -- ([IO.Path]::GetFullPath($UpstreamPath)) $projectedPath
    Assert-NativeCommandSucceeded 'Tenrai OpenAPI projection'

    $nswagArguments = @(
        'nswag'
        'openapi2csclient'
        "/Input:$projectedPath"
        "/Output:$clientPath"
        '/Namespace:PaperMalKing.MyAnimeList.Wrapper.Tenrai'
        '/ClassName:TenraiClient'
        '/JsonLibrary:SystemTextJson'
        '/GenerateClientInterfaces:false'
        '/ClientClassAccessModifier:internal'
        '/TypeAccessModifier:internal'
        '/GenerateNullableReferenceTypes:true'
        '/GenerateOptionalPropertiesAsNullable:true'
        '/UseBaseUrl:false'
        '/GenerateBaseUrlProperty:false'
        '/InjectHttpClient:true'
        '/DisposeHttpClient:false'
        '/GenerateExceptionClasses:true'
        '/ExceptionClass:TenraiApiException'
        '/GenerateResponseClasses:true'
        '/ResponseClass:TenraiResponse'
        '/WrapResponses:true'
        '/OperationGenerationMode:SingleClientFromOperationId'
        '/NewLineBehavior:LF'
    )
    & dotnet @nswagArguments
    Assert-NativeCommandSucceeded 'NSwag Tenrai client generation'

    $clientText = [IO.File]::ReadAllText($clientPath).Replace("`r`n", "`n").Replace("`r", "`n")
    $clientText = [Text.RegularExpressions.Regex]::Replace($clientText, '[\t ]+(?=\n|$)', '')
    [IO.File]::WriteAllText($clientPath, $clientText)
    if ((Get-Item -LiteralPath $projectedPath).Length -eq 0 -or (Get-Item -LiteralPath $clientPath).Length -eq 0) {
        throw 'Tenrai generation produced an empty output.'
    }

    Publish-GeneratedFiles @(
        [PSCustomObject]@{
            Source = $projectedPath
            Destination = Join-Path $OutputDirectory 'tenrai.openapi.json'
        },
        [PSCustomObject]@{
            Source = $clientPath
            Destination = Join-Path $OutputDirectory 'TenraiClient.g.cs'
        }
    ) $backupDirectory
}
finally {
    Remove-Item -LiteralPath $stageDirectory, $backupDirectory -Recurse -Force -ErrorAction SilentlyContinue
}
