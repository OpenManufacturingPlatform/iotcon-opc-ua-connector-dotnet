FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OMP_HOS_UI/OmpHandsOnUi.csproj", "OMP_HOS_UI/"]
RUN dotnet restore "OMP_HOS_UI/OmpHandsOnUi.csproj"
COPY . .
WORKDIR "/src/OMP_HOS_UI"
RUN dotnet build "OmpHandsOnUi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OmpHandsOnUi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OmpHandsOnUi.dll"]