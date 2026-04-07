FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5033

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AlboV2.DiscordBot/AlboV2.DiscordBot.csproj", "AlboV2.DiscordBot/"]
COPY ["AlboV2.Features/AlboV2.Features.csproj", "AlboV2.Features/"]
COPY ["AlboV2.Shared/AlboV2.Shared.csproj", "AlboV2.Shared/"]
RUN dotnet restore "AlboV2.DiscordBot/AlboV2.DiscordBot.csproj"
COPY . .
WORKDIR "/src/AlboV2.DiscordBot"
RUN dotnet build "./AlboV2.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AlboV2.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlboV2.DiscordBot.dll"]
