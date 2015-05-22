Write-Host "Ok, lets see what to publish"
$myget = $env:MYGET_TOKEN
$packBuild = $env:APPVEYOR_BUILD_NUMBER
$pattern = "*-$($packbuild).nupkg"
Write-Host "Build number is:",$packBuild
Write-Host "Pattern is :", $pattern
Write-Host "Discovering files...."
$files = Get-ChildItem -Path "" -Filter "$($pattern)" -Recurse
foreach($file in $files) {
	 Write-Host $file.FullName
	 $package = '{0} push "{1}" {2} -s https://www.myget.org/F/l0nley' -f $env:CACHED_NUGET,$file.FullName,$myget
     Invoke-Expression $package
}
