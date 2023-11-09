FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Giddup.ApplicationCore/Giddup.ApplicationCore.csproj", "src/Giddup.ApplicationCore/"]
COPY ["src/Giddup.Infrastructure/Giddup.Infrastructure.csproj", "src/Giddup.Infrastructure/"]
COPY ["src/Giddup.Presentation.Projections/Giddup.Presentation.Projections.csproj", "src/Giddup.Presentation.Projections/"]
COPY ["src/SharedAssemblyInfo.targets", "src/"]
RUN dotnet restore "src/Giddup.Presentation.Projections/Giddup.Presentation.Projections.csproj"
COPY . .
WORKDIR "/src/src/Giddup.Presentation.Projections"
RUN dotnet build "Giddup.Presentation.Projections.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Giddup.Presentation.Projections.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Giddup.Presentation.Projections.dll"]
