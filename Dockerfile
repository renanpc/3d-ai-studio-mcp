FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["global.json", "./"]
COPY ["ThreeDAiStudioMcp.csproj", "./"]
RUN dotnet restore "./ThreeDAiStudioMcp.csproj"

COPY . .
RUN dotnet publish "./ThreeDAiStudioMcp.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV HTTP_PORT=8080
ENV THREE_D_AI_STUDIO_BASE_URL=https://api.3daistudio.com
EXPOSE 8080

ENTRYPOINT ["./ThreeDAiStudioMcp"]
