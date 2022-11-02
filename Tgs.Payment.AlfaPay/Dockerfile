#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Payment.AlfaPay/Tgs.Payment.AlfaPay.csproj", "Tgs.Payment.AlfaPay/"]
COPY ["AlfaPay.Core/AlfaPay.Core.csproj", "AlfaPay.Core/"]
COPY ["Tgs.Payment.AlfaPay.Client/Tgs.Payment.AlfaPay.Client.csproj", "Tgs.Payment.AlfaPay.Client/"]
RUN dotnet restore "Tgs.Payment.AlfaPay/Tgs.Payment.AlfaPay.csproj"
COPY . .
WORKDIR "/src/Tgs.Payment.AlfaPay"
RUN dotnet build "Tgs.Payment.AlfaPay.csproj" -c ${BuildConfiguration} -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Payment.AlfaPay.csproj" -c ${BuildConfiguration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Payment.AlfaPay.dll"]