FROM microsoft/dotnet:2.0-sdk

# Separate dotnet restore to take advantage of cached builds


WORKDIR /app
COPY src/GOC.ApiGateway ./
RUN dotnet restore && \dotnet build && \dotnet publish -o out

ENTRYPOINT ["dotnet", "/app/out/GOC.ApiGateway.dll"]