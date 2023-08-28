FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /App
COPY ./Microservices/RestAPI .

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /App
EXPOSE 8080
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "RestAPI.dll"]