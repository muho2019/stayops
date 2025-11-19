# 개발 전용 .NET 8 Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0

# 작업 디렉토리
WORKDIR /src

# 먼저 csproj 파일들만 복사해서 restore → 캐시 활용
COPY StayOps.sln ./
COPY StayOps.Api/StayOps.Api.csproj ./StayOps.Api/
COPY StayOps.Application/StayOps.Application.csproj ./StayOps.Application/
COPY StayOps.Domain/StayOps.Domain.csproj ./StayOps.Domain/
COPY StayOps.Infrastructure/StayOps.Infrastructure.csproj ./StayOps.Infrastructure/

RUN dotnet restore ./StayOps.Api/StayOps.Api.csproj

# ⚠️ 소스 전체 복사는 하지 않음
# docker-compose의 volume mount가 소스코드를 덮어씌우기 때문

# API 포트
EXPOSE 8080

# dotnet watch 실행
CMD ["dotnet", "watch", "run", "--project", "StayOps.Api/StayOps.Api.csproj", "--urls=http://0.0.0.0:8080"]
