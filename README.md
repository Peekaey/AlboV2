
## AlboV2 - New and improved   

![albo](https://content.api.news/v3/images/bin/c1379c4a5c951e0c1116ca406e5cc68c)

Hi there, PM here. This is your reminder that the right to disconnect is now law.   
Because if you're not being paid 24 hours a day, you shouldn't be on call 24 hours a day either.

[Right to disconnect](https://www.fairwork.gov.au/employment-conditions/hours-of-work-breaks-and-rosters/right-to-disconnect)


## About
Self hostable discord bot that reminds members of a guild about the right to disconnect under Australian law.

![example](/RepoAssets/example.png)

## Features
- Automated alerts at 5:30PM AEST weekdays that reminds members about the right to disconnect.
  - Automatically handles timezone conversions for different timezones within Australia
  - Handles public holidays for different states and doesn't post notifications for country-wide & state-wide holidays that fall on weekdays
- Slash commands that ping specific members with
    - A reminder about the right to disconnect
    - Check in to see if they have disconnected for the day
- Randomised attachment of specific clips of Albo mentioning the right to disconnect
- Support for logging to a Grafana Loki endpoint

### Requirements
Parameter requirements (see example.appsettings.json)
- Discord Bot Token [Required]
- Discord Channel Id for the reminder alerts to be sent to [Required]
- TimezoneId (IANA) -Australia/Sydney as an example  [Required]
- EnableCaching - Caching of the response returned from Nager API via HybridCache (in memory only) [Required]
- EnableRemoteLogging - Toggles logging to a remote Grafana Loki endpoint (Optional)
- LokiUrl (Optional if EnableRemoteLogging disabled)
- LokiUsername (Optional if EnableRemoteLogging disabled)
- LokiApiToken (Optional if EnableRemoteLogging disabled)

### Running/Compiling
1.  Run ```dotnet restore``` to pull dependencies
2.  Run ```dotnet build``` to compile project
3.  ```dotnet run``` to run

#### Building for docker
1.  ```docker build -t albov2:latest .```
- Sample Run 
```
docker run \
  -e DISCORD_BOT_TOKEN="" \
  -e DISCORD_REMINDER_CHANNELID="" \
  -e TimezoneId="Australia/Sydney" \
  -e EnableCaching="true" \
  -e EnableRemoteLogging="true" \
  -e LokiUrl="" \
  -e LokiUsername="" \
  -e LokiApiToken="" \
  --name albov2-bot \
  albov2
```
- Can also pull the latest image directly via ```ghcr.io/peekaey/albov2:latest```

Made with Netcord & Nager