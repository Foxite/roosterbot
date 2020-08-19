FROM mcr.microsoft.com/dotnet/core/sdk:3.1

COPY Output/Config/ Config/

COPY Output/RoosterBot/ App/

WORKDIR App/
ENTRYPOINT dotnet RoosterBot.dll /Config
