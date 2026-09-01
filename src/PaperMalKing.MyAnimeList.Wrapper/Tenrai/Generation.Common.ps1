# SPDX-License-Identifier: AGPL-3.0-or-later
# Copyright (C) 2021-2026 N0D4N

function Assert-NativeCommandSucceeded([string] $commandName) {
    if ($LASTEXITCODE -ne 0) {
        throw "$commandName failed with exit code $LASTEXITCODE."
    }
}

function Publish-GeneratedFiles([object[]] $files, [string] $backupDirectory) {
    New-Item -ItemType Directory -Path $backupDirectory | Out-Null
    $published = [Collections.Generic.List[object]]::new()
    try {
        foreach ($file in $files) {
            $destination = [IO.Path]::GetFullPath($file.Destination)
            $backup = Join-Path $backupDirectory ([IO.Path]::GetFileName($destination))
            $existed = [IO.File]::Exists($destination)
            if ($existed) {
                [IO.File]::Copy($destination, $backup, $true)
            }

            [IO.File]::Move([IO.Path]::GetFullPath($file.Source), $destination, $true)
            $published.Add([PSCustomObject]@{
                Destination = $destination
                Backup = $backup
                Existed = $existed
            })
        }
    }
    catch {
        for ($index = $published.Count - 1; $index -ge 0; $index--) {
            $file = $published[$index]
            if ($file.Existed) {
                [IO.File]::Move($file.Backup, $file.Destination, $true)
            }
            else {
                [IO.File]::Delete($file.Destination)
            }
        }

        throw
    }
}
