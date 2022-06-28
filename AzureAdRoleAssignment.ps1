# Install the module. (You need admin on the machine.)
Import-Module AzureAD

# Your tenant ID (in the Azure portal, under Azure Active Directory > Overview).
$tenantID = 'e56b135d-b0e0-4ad8-8faa-1ca3915fe4b2'

# The name of your web app, which has a managed identity that should be assigned to the server app's app role.
#$webAppName = 'hqy-test-client'
#$resourceGroupName = 'HQY-Test-APIM'

# The name of the server app that exposes the app role.
$serverApplicationName = 'HQY-Azure-Server' # For example, MyApi

Connect-AzureAD -TenantId $tenantID

# Look up the web app's managed identity's object ID.
#$managedIdentityObjectId = (Get-AzWebApp -ResourceGroupName $resourceGroupName -Name $webAppName).identity.principalid
$managedIdentityObjectId = '54add5a2-1515-48ab-be22-2dca6156add8'
$userObjectId = 'c95ff74e-b892-4aae-a551-2984a786d506'
$groupObjectId = 'f5002490-7cca-4f70-a4f4-d95323067ca4'

# Look up the details about the server app's service principal and app role.
$serverServicePrincipal = (Get-AzureADServicePrincipal -Filter "DisplayName eq '$serverApplicationName'")
$serverServicePrincipalObjectId = $serverServicePrincipal.ObjectId
#$appRoleId = 'aa38686c-fa18-44a3-988e-3cd9c4007d82' #Test.Role
$appRoleId = 'cc7f81f8-4d2d-4269-a28c-f36e46fa276a' #Another.Test.Role

# Assign the managed identity access to the app role.
#New-AzureADServiceAppRoleAssignment `
#    -ObjectId $managedIdentityObjectId `
#    -Id $appRoleId `
#    -PrincipalId $managedIdentityObjectId `
#    -ResourceId $serverServicePrincipalObjectId

# Assign a user access to the app role
#New-AzureADUserAppRoleAssignment `
#    -ObjectId $userObjectId `
#    -PrincipalId $userObjectId `
#    -ResourceId $serverServicePrincipalObjectId `
#    -Id $appRoleId

New-AzureADGroupAppRoleAssignment `
    -ObjectId $groupObjectId `
    -PrincipalId $groupObjectId `
    -ResourceId $serverServicePrincipalObjectId `
    -Id $appRoleId