# Repository Guidelines

## 프로젝트 구조

- `backend/StayOps.sln`: `StayOps.Api`(HTTP), `StayOps.Application`(유즈케이스), `StayOps.Domain`(엔티티/규칙), `StayOps.Infrastructure`(DB·외부 연동)로 계층화되며 각각 `*.Tests`가 있습니다.
- `frontend/stayops-admin`: Next.js 15 + TypeScript. 정적 자산은 `public`, 전역 스타일은 `src/app/globals.css`.
- `docker-compose.yml`: MSSQL + API + Web을 로컬용으로 묶고 소스를 바인드 마운트해 핫 리로드를 지원합니다.

## 주요 기술 스택

- 백엔드: .NET 10
- 프런트엔드: Next.js 15.5, React 19, TypeScript 5, Tailwind CSS 4.
- 인프라: Docker Compose 3.9, Azure SQL Edge(로컬용 SQL Server 호환).

## 빌드 · 테스트 · 로컬 실행

- 백엔드 복원/빌드: `dotnet restore backend/StayOps.sln` → `dotnet build backend/StayOps.sln`.
- API 실행: `dotnet run --project backend/StayOps.Api` (Development).
- 백엔드 테스트+커버리지: `dotnet test backend/StayOps.sln /p:CollectCoverage=true` (xUnit, coverlet.collector).
- 프런트엔드: `cd frontend/stayops-admin && npm install` 이후 `npm run dev`(Turbopack) 또는 `npm run build && npm start`.
- 풀스택 컨테이너: `docker compose up --build` (web:3000, api:5000→8080, db:1433). ARM 호스트에서는 DB 컨테이너가 root로 실행되도록 설정되어 있습니다(권한 오류 회피용). API 헬스체크는 기본 템플릿 엔드포인트(`/weatherforecast`)를 ping하므로 별도 `/health`가 생기면 compose 경로를 함께 바꿔주세요.

## 코드 스타일 & 네이밍

- C#: 4-스페이스 들여쓰기, nullable 활성, `var`는 타입이 명확할 때만 사용. 공개 타입/메서드는 PascalCase, 로컬/파라미터는 camelCase, 비동기 메서드는 `Async` 접미사. 인터페이스는 `I` 접두사, DTO/명령/쿼리는 역할 명시, 예외는 구체적 타입 사용.
- .NET/DDD & OOP: 의존성은 DI로 주입 후 생성자 null 가드. 규칙은 엔티티/값 객체에 두고 Application 서비스는 트랜잭션·흐름만 조율. 애그리게이트 루트가 불변 조건을 유지하고 도메인 이벤트로 부수 효과를 전달. 값 객체는 불변, 컬렉션은 `IReadOnly*` 우선, 시간은 `DateTimeOffset` 사용. SOLID·캡슐화를 우선하며 라이브러리 성격이 아니면 `ConfigureAwait(false)`를 피합니다.
- TypeScript/React: 함수형 컴포넌트 + 명시적 타입. 파일을 작게 나누고 타입과 파일명을 일치 (`BookingService.cs`, `BookingCard.tsx`). Tailwind 유틸 클래스를 일관되게 사용하고 공통 스타일은 `globals.css`에 둡니다.

## 테스트 가이드

- 백엔드: 대상별 `*.Tests` 프로젝트. 테스트 클래스는 대상명+`Tests`, 메서드는 동작을 서술 (`Creates_booking_when_valid`). `[Fact]/[Theory]`로 성공·경계·실패를 다루고 도메인/서비스를 우선 검증합니다.
- 프런트엔드: 테스트 하네스 없음. 추가 시 소스 인접(`src/app/__tests__/*.test.tsx`)에 두고 React Testing Library를 권장합니다.

## 커밋 & PR

- 커밋: 짧은 명령형 제목(72자 이내)과 선택적 스코프 프리픽스 사용 (`backend: add booking aggregate`, `frontend: tweak hero`). 동작 변경 시 본문에 근거와 영향을 적습니다.
- PR: 의도, 설계 결정, 테스트 근거(`dotnet test`, UI 스크린샷)를 명시하고 관련 이슈를 링크합니다. 깨지는 변경/환경 변수/compose 수정 사항을 알리고 작은 단위로 제출합니다.

## 보안 및 설정

- `docker-compose.yml`과 Dockerfile은 개발 전용이며 일부 자격증명이 하드코딩되어 있습니다. 운영 환경에서는 별도 compose/환경 설정으로 비밀 정보를 분리하세요.
- TrustServerCertificate는 로컬 편의상 켜져 있습니다. 운영에서는 인증서 검증을 활성화하고 비밀번호·토큰을 비밀 관리 스토어로 옮기세요.
