Write-Host "Ok, lets see how we can update packages"
$packBuild = $env:APPVEYOR_BUILD_NUMBER
$pattern = "{BUILD}"
Write-Host "Build number is:",$packBuild
Write-Host "Pattern is :", $pattern
Write-Host "Discovering files...."
$files = Get-ChildItem -Path "" -Filter "project.json" -Recurse
foreach($file in $files) {
	 Write-Host $file.FullName
	 (gc $file.FullName) -replace $pattern,$packBuild | sc $file.FullName 
}
