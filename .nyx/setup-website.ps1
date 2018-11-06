param(
    [Parameter(mandatory=$true)]
    [string]$Company,

    [Parameter(mandatory=$true)]
    [string]$App,

    [Parameter(mandatory=$true)]
    [string]$Tenant,

    [Parameter(mandatory=$true)]
    [string]$AppHost,

    [Parameter(mandatory=$true)]
    [string]$Website,

    [string]$AppPool = $Website,

    [Parameter(mandatory=$true)]
    [string]$WebsiteSource,

    [string]$CertLocation = 'c:\certificate.pfx',

    [Parameter(mandatory=$true)]
    [string]$CertThumbprint,

    [Parameter(mandatory=$true)]
    [string]$CertPassword
    )

$cd = @{
       AllNodes = @(
       @{
           NodeName = 'localhost'

           Company = $Company
           App = $App
           Tenant = $Tenant
           Host = $AppHost

           PSDscAllowPlainTextPassword = $true
           CertPassword = $CertPassword
           CertThumbprint = $CertThumbprint
           CertLocation = $CertLocation

           WebsiteSource = $WebsiteSource
           Website = $Website
           AppPool = $AppPool
       })
}


Configuration EldersWebApp
{
    Import-DscResource -ModuleName PSDesiredStateConfiguration, xWebAdministration, xCertificate, xComputerManagement

    Node $AllNodes.NodeName
    {
        # Configure the server to automatically corret configuration drift including reboots if needed.
        LocalConfigurationManager
        {
            ConfigurationMode = 'ApplyAndAutoCorrect'
            RebootNodeifNeeded = $node.RebootNodeifNeeded
        }

        Script CleanWebsiteDirectory
        {
            GetScript =
            {
                #needs to return hashtable.
            }
            SetScript =
            {
                $appState = Get-WebAppPoolState -Name $node.AppPool
                if ($appState.Value -eq "started") {
                    Stop-WebAppPool -Name $node.AppPool
                }
                Stop-Website -Name $node.Website
                Sleep 10
                $dest = [io.path]::combine([environment]::getfolderpath('CommonApplicationData'), $using:node.Company, $using:node.App, $using:node.Tenant, $using:node.Host)
                Remove-Item -Path $dest -Recurse -Force -Verbose
                Start-Website -Name $node.Website
            }
            TestScript =
            {
                $dest = [io.path]::combine([environment]::getfolderpath('CommonApplicationData'), $using:node.Company, $using:node.App, $using:node.Tenant, $using:node.Host)
                -Not(Test-Path -Path $dest)
            }
        }

        File WebsiteDirectory
        {
            SourcePath = $node.WebsiteSource
            DestinationPath = [io.path]::combine([environment]::getfolderpath('CommonApplicationData'), $node.Company, $node.App, $node.Tenant, $node.Host)
            Ensure = 'Present'
            Recurse = $true
            Type = 'Directory'
            Checksum = "SHA-256"
            Force = $true
            MatchSource = $true
            DependsOn = '[Script]CleanWebsiteDirectory'
        }

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

        xWebAppPool pool
        {
            Ensure                  = 'Present'
            Name                    = $node.AppPool
            State                   = 'Started'
            autoStart               = $true
            startMode               = 'AlwaysRunning'
            managedRuntimeVersion   = 'v4.0'
            managedPipelineMode     = 'Integrated'
            identityType            = 'NetworkService'
            DependsOn = '[Script]CleanWebsiteDirectory'
        }

        xWebSite app
        {
            Ensure          = 'Present'
            Name            = $node.Website
            ApplicationPool = $node.AppPool
            State           = 'Started'
            PhysicalPath    = [io.path]::combine([environment]::getfolderpath('CommonApplicationData'), $node.Company, $node.App, $node.Tenant, $node.Host)
            BindingInfo     = @(
                MSFT_xWebBindingInformation
                {
                    Protocol              = 'HTTPS'
                    Port                  = 443
                    CertificateThumbprint = $node.CertThumbprint
                    CertificateStoreName  = 'My'
                    HostName              = $node.Website
                    SslFlags              = 1
                }
            )
            DependsOn = '[Script]AllowNetworkServiceToReadCert', '[xWebAppPool]pool'
        }
    }
}


EldersWebApp -ConfigurationData $cd
Start-DscConfiguration -Path EldersWebApp -Wait -Verbose -Force
