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
	 $env:CACHED_NUGET push "$($file.FullName)" 63d8f776-2dd7-4d06-bd64-3a6c9b31e7eb -s https://www.myget.org/F/l0nley
}
