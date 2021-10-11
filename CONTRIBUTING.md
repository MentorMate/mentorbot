## Contribute with project management

1. Identify tasks needed for the next milestone.
2. Create milestones.
3. Create issues
	1. Add business requirements.
	2. Set label enhancement or bug.
    3. Set label new.
    4. Add task list. For example (design,writing unit tests, develop, verified)
4. Revisit verified stories and add labels closed if accepted. 

## Contribute with design

1. Replace label "new" with "design".
2. Create MockUp and/or design.
3. Replace label "design" with "open"

## Contribute with development

1. Download the project from this repository
2. Assign to yourself feature or bug with label "open"
3. Write tests and code
4. Commit to feature branch and create pull request to the master branch.
5. Replace label "open" with "resolved"
6. Associate the pull request
0. Make reviews to other developers "pull request"

## Contribute with testing

1. Get a feature or bug with label "resolved" and verify it
2. Return it to "open" with a comment or replace the label with "verified"

## Run in linux docker as production
1. In top folder build the image
   ```
   docker build -t mentorbot-functions -f src\MentorBot.Functions\Dockerfile .
   ```
2. Run
   ```
   docker run -it --rm --name mentorbot-local mentorbot-functions
   ```

## Build with docker under linux
```bash
docker run -it --rm --name build-bot -v %cd%:/app -w /app  mcr.microsoft.com/dotnet/sdk:6.0

# REM Install sdk 3.1
# wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && dpkg -i packages-microsoft-prod.deb && rm packages-microsoft-prod.deb
# apt-get update && apt-get install -y dotnet-sdk-3.1
# dotnet publish -c Release src/MentorBot.Functions/MentorBot.Functions.csproj -o dist/
```

## Create test az-function
```
az functionapp create --resource-group mentorbot --consumption-plan-location westeurope --runtime dotnet-isolated --functions-version 3 --name mentorbot-application --storage-account mentorbotstorage --os-type Linux
az functionapp config appsettings set --settings FUNCTIONS_EXTENSION_VERSION=~4 -n mentorbot-application -g mentorbot
az functionapp deployment source config-zip -n mentorbot-application -g mentorbot --src app.zip --subscription mentorbot
```