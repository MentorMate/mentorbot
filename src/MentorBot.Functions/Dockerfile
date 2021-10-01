FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated6.0 AS base
 COPY --from=mcr.microsoft.com/dotnet/aspnet:6.0 /usr/share/dotnet /usr/share/dotnet
 WORKDIR /home/site/wwwroot
 EXPOSE 80

 FROM mcr.microsoft.com/dotnet/runtime:3.1 as runtime3_1
 FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
 # Copy .NET Core 3.1 runtime from the 3.1 image
 COPY --from=runtime3_1 /usr/share/dotnet/host /usr/share/dotnet/host
 COPY --from=runtime3_1 /usr/share/dotnet/shared /usr/share/dotnet/shared
 WORKDIR /app
 COPY ["src/MentorBot.Functions/MentorBot.Functions.csproj", "src/MentorBot.Functions/"]
 RUN dotnet restore "src/MentorBot.Functions/MentorBot.Functions.csproj"
 COPY src/ src/
 COPY tests/ tests/
 COPY tools/ tools/
 WORKDIR "/app/src/MentorBot.Functions"
 RUN dotnet build "MentorBot.Functions.csproj" -c Release -o /app/build
    
 FROM build AS publish
 RUN dotnet publish "MentorBot.Functions.csproj" -c Release -o /app/publish
    
 FROM base AS final
 WORKDIR /home/site/wwwroot
 COPY --from=publish /app/publish .
 ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
     AzureFunctionsJobHost__Logging__Console__IsEnabled=true