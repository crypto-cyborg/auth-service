FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./AuthService.sln ./AuthService.sln
COPY ./AuthService.API/AuthService.API.csproj ./AuthService.API/
COPY ./AuthService.Core/AuthService.Core.csproj ./AuthService.Core/
COPY ./AuthService.Application/AuthService.Application.csproj ./AuthService.Application/
COPY ./AuthService.Persistence/AuthService.Persistence.csproj ./AuthService.Persistence/

RUN dotnet restore

COPY ./AuthService.API/ ./AuthService.API/
COPY ./AuthService.Core/ ./AuthService.Core/
COPY ./AuthService.Application/ ./AuthService.Application/
COPY ./AuthService.Persistence/ ./AuthService.Persistence/

RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT [ "dotnet", "AuthService.API.dll" ]

