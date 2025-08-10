# usage: .\build.ps1 path\to\rimworld\mods\folder

param (
    [Parameter(Mandatory = $true)]
    [string]$pathToMods
)

$modName = "RimWorldGravshipRangeOnMapMod"
$outputPath = "$pathToMods\$modName"

echo "Outputting to: $outputPath"

Push-Location

Set-Location "Source"

dotnet build

Pop-Location

echo "Copying..."

$dllSourcePath = "Source\bin\Debug\net472\$modName.dll"
$dllOutputPath = "Assemblies"

echo "$dllSourcePath -> $dllOutputPath"

cp $dllSourcePath $dllOutputPath

cp -r -Force "About" "$outputPath"
cp -r -Force "Assemblies" "$outputPath"
cp "README.md" "$outputPath"
cp "LICENSE.md" "$outputPath"

echo "Done"

$normalizedPath = (Resolve-Path $outputPath).Path.ToLower()

# get list of currently open Explorer windows
$openFolders = (New-Object -ComObject Shell.Application).Windows() |
    Where-Object { $_.Name -eq "File Explorer" -and $_.LocationURL -like "file:///*" } |
    ForEach-Object { ([uri]$_.LocationURL).LocalPath.ToLower() }

if ($openFolders -notcontains $normalizedPath) {
    Start-Process explorer.exe $outputPath
}
