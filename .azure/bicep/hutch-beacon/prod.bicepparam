using '../main.bicep'

param serviceName = 'hutch-beacon'

param env = 'prod'
param beaconHostnames = ['beacon.nottingham.ac.uk']
param beaconAppName = 'hutch-beacon'
param aspName = 'linuxapps'
