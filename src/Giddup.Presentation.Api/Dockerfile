FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Giddup.ApplicationCore/Giddup.ApplicationCore.csproj", "src/Giddup.ApplicationCore/"]
COPY ["src/Giddup.Infrastructure/Giddup.Infrastructure.csproj", "src/Giddup.Infrastructure/"]
COPY ["src/Giddup.Presentation.Api/Giddup.Presentation.Api.csproj", "src/Giddup.Presentation.Api/"]
COPY ["src/SharedAssemblyInfo.targets", "src/"]
RUN dotnet restore "src/Giddup.Presentation.Api/Giddup.Presentation.Api.csproj"
COPY . .
WORKDIR "/src/src/Giddup.Presentation.Api"
RUN dotnet build "Giddup.Presentation.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Giddup.Presentation.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Giddup.Presentation.Api.dll"]
