FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Proyecto3Api.Domain/Proyecto3Api.Domain.csproj", "Proyecto3Api.Domain/"]
COPY ["Proyecto3Api.Infrastructure/Proyecto3Api.Infrastructure.csproj", "Proyecto3Api.Infrastructure/"]
COPY ["Proyecto3Api/Proyecto3Api.csproj", "Proyecto3Api/"]
RUN dotnet restore "Proyecto3Api/Proyecto3Api.csproj"
COPY . .
WORKDIR "/src/Proyecto3Api"
RUN dotnet publish "Proyecto3Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Proyecto3Api.dll"]
