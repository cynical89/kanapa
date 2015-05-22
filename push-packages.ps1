Write-Host "Ok, lets see what to publish"
$env:APPVEYOR_BUILD_NUMBER = "{BUILD}"
$packBuild = $env:APPVEYOR_BUILD_NUMBER
$pattern = "*-$($packbuild).nupkg"
Write-Host "Build number is:",$packBuild
Write-Host "Pattern is :", $pattern
Write-Host "Discovering files...."
$files = Get-ChildItem -Path "" -Filter "$($pattern)" -Recurse
foreach($file in $files) {
	 Write-Host $file.FullName
}
