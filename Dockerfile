
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "ExerciseCSolution/Api/Api.csproj"
RUN dotnet publish "ExerciseCSolution/Api/Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]