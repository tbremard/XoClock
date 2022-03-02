$csprojPath = $args[0] # path to the file i.e. 'C:\Users\ben\Code\csproj powershell\MySmallLibrary.csproj'
$newVersion = $args[1] # the build version, from VSTS build i.e. "1.1.20170323.1"

Write-Host "Starting process of generating new version number for the csproj"
$x=Get-Location
Write-Host "CurrentDirectory: "$x.Path
$csprojPath = $x.Path+"\"+$csprojPath
Write-Host ("filePath: " + $csprojPath)

$splitNumber = $newVersion.Split(".")
if( $splitNumber.Count -eq 4 )
{
	$majorNumber = $splitNumber[0]
	$minorNumber = $splitNumber[1]
	$revisionNumber = $splitNumber[3]
	$myBuildNumber = (Get-Date).Year + ((Get-Date).Month * 31) + (Get-Date).Day
	$myBuildNumber = $majorNumber + "." + $minorNumber + "." + $myBuildNumber + "." + $revisionNumber
	$filePath = $csprojPath
	$xml=New-Object XML
	$xml.Load($filePath)
	$FileVersionNode = $xml.Project.PropertyGroup.FileVersion
	if ($FileVersionNode -eq $null) {
		# If you have a new project and have not changed the FileVersion number the FileVersion tag may not exist
		$FileVersionNode = $xml.CreateElement("FileVersion")
		$xml.Project.PropertyGroup.AppendChild($FileVersionNode)
		Write-Host "FileVersion XML tag added to the csproj"
	}
	$xml.Project.PropertyGroup.FileVersion = $myBuildNumber
	$xml.Save($filePath)

	Write-Host "Updated csproj "$csprojPath" and set to FileVersion "$myBuildNumber
}
else
{
	Write-Host "ERROR: Something was wrong with the build number"
}