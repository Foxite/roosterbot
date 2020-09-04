FROM mcr.microsoft.com/dotnet/core/sdk:3.1

COPY Output/RoosterBot/ App/

WORKDIR App/
CMD [ "dotnet", "RoosterBot.dll", "/Config" ]
