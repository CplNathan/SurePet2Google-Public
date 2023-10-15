#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-arm64v8 AS base
USER app
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SurePet2Google.Blazor/Server/SurePet2Google.Blazor.Server.csproj", "SurePet2Google.Blazor/Server/"]
COPY ["GoogleHelper/GoogleHelper.csproj", "GoogleHelper/"]
COPY ["SurePet2Google.Blazor/Client/SurePet2Google.Blazor.Client.csproj", "SurePet2Google.Blazor/Client/"]
COPY ["SurePet2Google.Blazor/Shared/SurePet2Google.Blazor.Shared.csproj", "SurePet2Google.Blazor/Shared/"]
RUN dotnet restore "SurePet2Google.Blazor/Server/SurePet2Google.Blazor.Server.csproj"
COPY . .
WORKDIR "/src/SurePet2Google.Blazor/Server"
RUN dotnet build "SurePet2Google.Blazor.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "SurePet2Google.Blazor.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SurePet2Google.Blazor.Server.dll"]