using '../main.bicep'

import { referenceSecret } from 'br/DrsUtils:functions:v1'

param serviceName = 'hutch-beacon'

param env = 'prod'
param frontendHostnames = ['beacon.nottingham.ac.uk']
param frontendAppName = 'hutch-beacon'
param aspName = 'linuxapps'

param keyVaultName = '${serviceName}-${env}-kv'

param backendAppSettings = {
    BeaconInfo__BeaconId: 'uk.unott.demo'
    BeaconInfo__ApiVersion: 'v2.0.0'
    BeaconInfo__Granularity: 'boolean'
    BeaconInfo__Name: 'University of Nottingham Demo Beacon'
    BeaconInfo__Environment: 'development'
    BeaconInfo__Description: 'This Beacon is based on the GA4GH Beacon standard v2.0 https://github.com/ga4gh-beacon/beacon-v2/'
    BeaconInfo__Version: 'v2.0.0'
    BeaconInfo__WelcomeUrl: 'https://beacon.nottingham.ac.uk/api'

    Organisation__Id: 'UNOTT'
    Organisation__Name: 'University of Nottingham - Centre for Health Informatics'
    Organisation__Description: 'University of Nottingham is a public research university in Nottingham, England. It is a member of the ELIXIR-UK Node'
    Organisation__Address: 'Centre for Health Informatics, University of Nottingham, University Park, Nottingham, UK'
    Organisation__WelcomeUrl: 'https://www.nottingham.ac.uk/'
    Organisation__ContactUrl: referenceSecret(keyVaultName, 'mailto-address')
    Organisation__LogoUrl: 'https://www.nottingham.ac.uk/SiteElements/Images/Base/logo.png'

    ServiceInfo__Type__Group: 'uk.unott.demo'
    ServiceInfo__Type__Artifact: 'beacon'
    ServiceInfo__Type__Version: '1.0'
    ServiceInfo__DocumentationUrl: 'https://github.com/Health-Informatics-UoN/beacon-tools'
}
