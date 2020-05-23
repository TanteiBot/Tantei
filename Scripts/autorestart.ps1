# Name of bot's dll;
$MyBotName = "PaperMalKing.dll";
# As in after compiling this script is stored in bot directory's child we need to get bot directory
$BotDllDir = (get-item $PSScriptRoot);
# Path to current directory + bot.dll's name
$BotDllPath = Join-Path $BotDllDir $MyBotName;


while ($true)
{
    $process = Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList $BotDllPath -PassThru -WorkingDirectory $BotDllDir -NoNewWindow -Wait

    if ($process.ExitCode -eq 0)
	{
        Write-Host -BackgroundColor Black -ForegroundColor Green -Object $(Get-Date)
        Write-Host "Bot exited clearly";
        break;
    }
    Write-Host -BackgroundColor Black -ForegroundColor Red -Object $(Get-Date)
    Write-Host "Bot exited non-clear. Sleeping for 10 seconds then restarting";
    Start-Sleep -Seconds 10;
}

Read-Host -Prompt "Press Enter to exit"