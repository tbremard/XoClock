# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies?view=powershell-7.2
# Get-ExecutionPolicy -List
# Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser

Param (
    [string]$rootFolder = $(get-location).Path,
    [string]$filePattern,
    [string]$assemblyVersion, 
    [string]$FileVersion, 
    [string]$assemblyProduct,
    [string]$assemblyDescription,
    [string]$assemblyCopyright,
    [string]$assemblyCompany,
    [string]$assemblyTrademark,
    [string]$assemblyConfiguration,
    [string]$assemblyInformationalVersion,
    [string]$customAttributes
)

Write-Host ([Environment]::NewLine)
Write-Host ("Parameters:")
Write-Host ("----------")
Write-Host ("Root folder: " + $rootFolder)
Write-Host ("File pattern: " + $filePattern)
Write-Host ("Version: " + $assemblyVersion)
Write-Host ("File version: " + $FileVersion)

function UpdateAssemblyInfo()
{
    Write-Host([Environment]::NewLine)
    Write-Host("Searching for files: $filePattern")
    foreach ($file in $input) 
    {       
        Write-Host("============================================================")
        Write-Host ($file.FullName)
        $tmpFile = $file.FullName + ".tmp"
        $fileContent = Get-Content $file.FullName -encoding utf8
        Write-Host ([Environment]::NewLine)
        Write-Host("Updating attributes")
        Write-Host("-------------------")
        $fileContent = TryReplace "AssemblyVersion" $assemblyVersion;
        $fileContent = TryReplace "FileVersion" $FileVersion;        
        Write-Host ([Environment]::NewLine)
        Write-Host("Saving changes...")
        Set-Content $tmpFile -value $fileContent -encoding utf8   
        Move-Item $tmpFile $file.FullName -force
    }
    Write-Host("============================================================")
    Write-Host("Done!")
}

function TryReplace($attributeKey, $value)
{
    if($value)
    {
        $containsAttributeKey = $fileContent | %{$_ -match $attributeKey}

        If($containsAttributeKey -contains $true)
        {
            Write-Host("Updating '$attributeKey'...")

            if($file.Extension -eq ".vb")
            {
                $attribute = $attributeKey + '("' + $value + '")';
            }
            else
            {
                $attribute = $attributeKey + '(@"' + $value + '")';
            }

            $fileContent = $fileContent -replace ($attributeKey +'\(@{0,1}".*"\)'), $attribute
        }
        else
        {
            Write-Host("Skipped '$attributeKey' (not found in file)")
        }
    }
    else
    {
        Write-Host("Skipped '$attributeKey' (no value defined)")
    }
    return $fileContent
}

function ValidateVersionString($versionString)
{
    $versionStringRegex = [System.Text.RegularExpressions.Regex]::Match($versionString, "^[0-9]+(\.[0-9]+){1,3}$");
    return $versionStringRegex.Success;
}

function ValidateParams()
{
    if($assemblyVersion -and (-not (ValidateVersionString $assemblyVersion)))
    {
        Write-Host ("'$assemblyVersion' is not a valid parameter for attribute 'AssemblyVersion'")
        return $false
    }
    if($FileVersion -and (-not (ValidateVersionString $FileVersion)))
    {
        Write-Host ("'$FileVersion' is not a valid parameter for attribute 'FileVersion'")
        return $false
    }
    return $true
 }

function WriteCustomAttributes($customAttributes)
{
    foreach($customAttribute in ($customAttributes -split ';'))
    {
        $customAttributeKey, $customAttributeValue = $customAttribute.Split('=')
      
        $fileContent = TryReplace $customAttributeKey $customAttributeValue
    }
    return $fileContent
}

if(ValidateParams)
{
    Get-Childitem -Path $rootFolder -recurse |? {$_.Name -like $filePattern} | UpdateAssemblyInfo; 
}