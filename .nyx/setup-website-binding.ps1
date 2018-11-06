param(
    [Parameter(mandatory=$true)]
    [string]$WebsiteName,

    [Parameter(mandatory=$true)]
    [string]$BindingDomainName,

    [string]$CertLocation = 'C:\Certs\certs_innov8tive\STAR.innov8tivoncline.com.pfx',

    [Parameter(mandatory=$true)]
    [string]$CertThumbprint,

    [Parameter(mandatory=$true)]
    [string]$CertPassword
    )

$cd = @{
       AllNodes = @(
       @{
           NodeName = 'localhost'

           PSDscAllowPlainTextPassword = $true
           CertPassword = $CertPassword
           CertThumbprint = $CertThumbprint
           CertLocation = $CertLocation
       })
}


Configuration EldersWebApp
{
    Import-DscResource -ModuleName PSDesiredStateConfiguration, xWebAdministration, xCertificate, xComputerManagement

    Node $AllNodes.NodeName
    {
        $password = $Node.CertPassword | ConvertTo-SecureString -asPlainText -Force
        [PSCredential] $certCred = New-Object System.Management.Automation.PSCredential('notused',$password)

        xPfxImport certMy
        {
            Ensure = 'Present'
            Path = $node.CertLocation
            Thumbprint = $node.CertThumbprint
            Location = 'LocalMachine'
            Store = 'My'
            Exportable = $true
            Credential = $certCred
        }

        xPfxImport certRoot
        {
            Ensure = 'Present'
            Path = $node.CertLocation
            Thumbprint = $node.CertThumbprint
            Location = 'LocalMachine'
            Store = 'Root'
            Exportable = $true
            Credential = $certCred
        }

        Script AllowNetworkServiceToReadCert
        {

            # Must return a hashtable with at least one key
            # named 'Result' of type String
            GetScript = {
                Return @{
                    Result = [string]$(netsh advfirewall show allprofiles)
                }
            }

            # Must return a boolean: $true or $false
            TestScript = {
                Write-Verbose "Should check for permissions. If present return $true"
                Return $false
            }

            # Returns nothing
            SetScript = {
                $keyName=(((Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Thumbprint -like $using:node.CertThumbprint}).PrivateKey).CspKeyContainerInfo).UniqueKeyContainerName
                $keyPath = "C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys\"
                $fullPath=$keyPath+$keyName
                $acl=Get-Acl -Path $fullPath
                $permission="NT AUTHORITY\NETWORK SERVICE","Read","Allow"
                $accessRule=new-object System.Security.AccessControl.FileSystemAccessRule $permission
                $acl.AddAccessRule($accessRule)
                Set-Acl $fullPath $acl
            }

           # DependsOn = '[xPfxImport]certMy'
        }


    }
}

Function Add-Binding {
    param(
        [Parameter(mandatory=$true)]
        [string]$WebsiteName,

        [Parameter(mandatory=$true)]
        [string]$DomainName,

        [Parameter(mandatory=$true)]
        [string]$CertThumbprint = '631516838b4ef1b208eade1ff29d80b6d55b6505'
        )

        #Add The Binding
        Write-Host 'Start Adding Binding...'
        $binding = (Get-WebBinding -Name $WebsiteName | where-object {$_.protocol -eq "https" -and  $_.bindingInformation -like ('*' + $DomainName +'*')}) 
        if($binding -ne $null) {
            Write-Host 'There is already a binding with the same hostname.' -ForegroundColor Yellow 
            Remove-WebBinding -Name $WebsiteName -Port 443 -Protocol "https" -HostHeader $DomainName
            Write-Host 'Existing binding removed!'
        } 

        New-WebBinding -Name $WebsiteName -Port 443 -Protocol https -HostHeader $DomainName -SSLFlags 1
        Write-Host 'New binding added!'

        (Get-WebBinding -Name $WebsiteName -Port 443 -Protocol "https" -HostHeader $DomainName).AddSslCertificate($CertThumbprint, "my")
        Write-Host 'Certificate added to the new binding!'

        Write-Host 'Successfuly added the new binding to the website!' -ForegroundColor Green
}

#IF RUN LOCALY AND HAVE PROLEMS WITH WINRM USE THIS
#Enable-PSRemoting -SkipNetworkProfileCheck -Force

EldersWebApp -ConfigurationData $cd
Start-DscConfiguration -Path EldersWebApp -Wait -Verbose -Force
Add-Binding -WebsiteName $WebsiteName -DomainName $BindingDomainName -CertThumbprint $CertThumbprint




