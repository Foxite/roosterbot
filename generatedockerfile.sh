#!/bin/sh

# NOTE: this only does most of the work. After generating the dockerfile, some modifications are still necessary:
# - All image names need to be lowercased and dots removed
# - GLU.Discord needs to copy the binaries from GLU and DiscordNet
# - GLU needs to copy from Schedule
# - Schedule and Weather from DateTimeUtils

cat << EOF
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY RoosterBot RoosterBot
COPY Component.targets .
RUN dotnet build "RoosterBot/RoosterBot.csproj" -c Release

EOF

# Add/remove any components to be included in the build here
components=(Meta DiscordNet DateTimeUtils Schedule GLU GLU.Discord DiscordNet Tools)
for component in "${components[@]}"
do
    cat << EOF
FROM build AS build-$component
WORKDIR /src/RoosterBot.$component
COPY RoosterBot.$component .
RUN dotnet build "RoosterBot.$component.csproj" -c Release

EOF
done

cat << EOF
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine AS runtime
COPY --from=build /src/Output/RoosterBot /App
EOF


for component in "${components[@]}"
do
    cat << EOF
COPY --from=build-$component /src/Output/RoosterBot/Components/$component /App/Components/$component
EOF
done

cat << EOF

WORKDIR /App
CMD [ "dotnet", "RoosterBot.dll", "/Config" ]
EOF
