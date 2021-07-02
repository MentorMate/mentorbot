targetScope = 'subscription'

param mentorbot_name string = 'mentorbot'
param storage_name string = '${mentorbot_name}storage'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: mentorbot_name
  location: deployment().location
}

module functionsDeployPlan 'functions.bicep' = {
  name: 'functionsDeploy'
  scope: rg
  params: {
    app_name: mentorbot_name
    storage_name: storage_name
  }
}
