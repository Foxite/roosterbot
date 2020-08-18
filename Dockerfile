FROM mcr.microsoft.com/dotnet/core/runtime:3.1

COPY Config/ Config/

COPY bin/netcoreapp3.1/ App/

CMD ["App/dotnet", "NetCore.Docker.dll -- \"../Config\""]
