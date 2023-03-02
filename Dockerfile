#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SurePet2Google.Blazor/Server/SurePet2Google.Blazor.Server.csproj", "SurePet2Google.Blazor/Server/SurePet2Google.Blazor.Server.csproj"]
COPY ["SurePet2Google.Blazor/Client/SurePet2Google.Blazor.Client.csproj", "SurePet2Google.Blazor/Client/SurePet2Google.Blazor.Client.csproj"]
COPY ["SurePet2Google.Blazor/Shared/SurePet2Google.Blazor.Shared.csproj", "SurePet2Google.Blazor/Shared/SurePet2Google.Blazor.Shared.csproj"]
COPY ["GoogleHelper/GoogleHelper.csproj", "GoogleHelper/GoogleHelper.csproj"]
RUN dotnet restore "SurePet2Google.Blazor/Server/SurePet2Google.Blazor.Server.csproj"
COPY . .
WORKDIR "/src/SurePet2Google.Blazor/Server"
RUN dotnet build "SurePet2Google.Blazor.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SurePet2Google.Blazor.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SurePet2Google.Blazor.Server.dll"]
