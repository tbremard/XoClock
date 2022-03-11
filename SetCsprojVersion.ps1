$csprojPath = $args[0] # path to the file i.e. 'C:\Users\ben\Code\csproj powershell\MySmallLibrary.csproj'
$newVersion = $args[1] # the build version, from VSTS build i.e. "1.1.20170323.1"

Write-Host "Patch csproj with version number"
$x=Get-Location
$csprojPath = $x.Path+"\"+$csprojPath
Write-Host("CurrentDirectory: "+$x.Path)
Write-Host("FilePath        : " + $csprojPath)
Write-Host("Version         : " + $newVersion)
$splitNumber = $newVersion.Split(".")
if( $splitNumber.Count -eq 4 )
{
	$xml=New-Object XML
	$xml.Load($csprojPath)
	$xml.Project.PropertyGroup[0].FileVersion = $newVersion
	$xml.Project.PropertyGroup[0].AssemblyVersion = $newVersion
	$xml.Project.PropertyGroup[0].Version = $newVersion
	$xml.Save($csprojPath)
	Write-Host "Updated "$csprojPath" with version: "$newVersion
}
else
{
	Write-Host "ERROR: Something was wrong with the build number"
}