# If you switch to alpine, command localization will break because they don't have locales installed
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY RoosterBot RoosterBot
COPY Component.targets .
RUN dotnet build "RoosterBot/RoosterBot.csproj" -c Release

# build all components in separate images. consider using buildkit to do it in parallel.
FROM build AS build-meta
WORKDIR /src/RoosterBot.Meta
COPY RoosterBot.Meta .
RUN dotnet build "RoosterBot.Meta.csproj" -c Release

FROM build AS build-datetimeutils
WORKDIR /src/RoosterBot.DateTimeUtils
COPY RoosterBot.DateTimeUtils .
RUN dotnet build "RoosterBot.DateTimeUtils.csproj" -c Release

FROM build AS build-discordnet
WORKDIR /src/RoosterBot.DiscordNet
COPY RoosterBot.DiscordNet .
RUN dotnet build "RoosterBot.DiscordNet.csproj" -c Release

FROM build-datetimeutils AS build-schedule
WORKDIR /src/RoosterBot.Schedule
COPY RoosterBot.Schedule .
RUN dotnet build "RoosterBot.Schedule.csproj" -c Release

FROM build-schedule AS build-glu
WORKDIR /src/RoosterBot.GLU
COPY RoosterBot.GLU .
RUN dotnet build "RoosterBot.GLU.csproj" -c Release

FROM build-glu AS build-gludiscord
COPY --from=build-discordnet /src/RoosterBot.DiscordNet /src/RoosterBot.DiscordNet
COPY --from=build-discordnet /src/Output/RoosterBot/Components/DiscordNet /src/Output/RoosterBot/Components/DiscordNet
WORKDIR /src/RoosterBot.GLU.Discord
COPY RoosterBot.GLU.Discord .
RUN dotnet build "RoosterBot.GLU.Discord.csproj" -c Release

FROM build AS build-tools
WORKDIR /src/RoosterBot.Tools
COPY RoosterBot.Tools .
RUN dotnet build "RoosterBot.Tools.csproj" -c Release

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
COPY --from=build /src/Output/RoosterBot /App
COPY --from=build-datetimeutils /src/Output/RoosterBot/Components/DateTimeUtils /App/Components/DateTimeUtils
COPY --from=build-discordnet /src/Output/RoosterBot/Components/DiscordNet /App/Components/DiscordNet
COPY --from=build-meta /src/Output/RoosterBot/Components/Meta /App/Components/Meta
COPY --from=build-glu /src/Output/RoosterBot/Components/GLU /App/Components/GLU
COPY --from=build-gludiscord /src/Output/RoosterBot/Components/GLU.Discord /App/Components/GLU.Discord
COPY --from=build-schedule /src/Output/RoosterBot/Components/Schedule /App/Components/Schedule
COPY --from=build-tools /src/Output/RoosterBot/Components/Tools /App/Components/Tools

WORKDIR /App
CMD [ "dotnet", "RoosterBot.dll", "/Config" ]
