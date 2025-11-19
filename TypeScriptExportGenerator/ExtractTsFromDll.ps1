param([string]$dllPath, [string]$tsPath)

Add-Type -Path $dllPath
$ts = [TsExportGenerated.ExportedTs]::Content
Set-Content -Path $tsPath -Value $ts