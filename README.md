# StayOps – 호텔 미니 PMS (백엔드 포트폴리오)

7년 차 백엔드 개발자로서 설계 의도와 성능 최적화를 중심에 둔 호텔 미니 PMS 프로젝트입니다. 현재는 초기 스캐폴드와 Docker Compose 기반 로컬 환경만 구성된 상태입니다.

## 주요 포인트

- 설계 의도: 계층형 아키텍처(.NET 10, DDD 지향)로 도메인 규칙을 명확히 분리하고, 애그리게이트·도메인 이벤트 중심 확장을 목표. Application 계층은 흐름 조율과 트랜잭션에 집중하고, 도메인은 순수 모델로 불변 조건을 유지하게 설계.
- 성능 최적화: 초기 단계부터 병목 관찰과 튜닝을 염두. API/DB 컨테이너 분리로 쿼리/네트워크 지연을 측정하고, 읽기 최적화(projection, 캐싱)와 인덱스 전략을 계획적으로 도입하며, 비동기 I/O로 리소스 효율을 높이는 것을 목표.

## 프로젝트 구조

- backend
  - `StayOps.Api`: ASP.NET Core 엔트리. HTTP 엔드포인트, 요청 파이프라인, DI 루트 구성. 컨트롤러·필터·미들웨어가 여기에 위치.
  - `StayOps.Application`: 유즈케이스 계층. 도메인 규칙을 호출하고 트랜잭션/흐름을 조율하는 커맨드·쿼리 핸들러, DTO, 서비스 인터페이스를 둘 예정.
  - `StayOps.Domain`: 도메인 모델 계층. 엔티티, 값 객체, 도메인 서비스, 도메인 이벤트와 규칙이 담기는 곳이며 인프라 의존성 없이 순수 모델을 유지.
  - `StayOps.Infrastructure`: 영속성·외부 연동 계층. DB 접근, 캐시 등 기술 세부 구현체와 DI 등록을 담당.
  - 각 계층별 `*.Tests`: xUnit 기반 테스트 프로젝트가 병렬로 위치하여 도메인 → 애플리케이션 → API 순으로 검증 범위를 넓힐 예정.
- frontend
  - `stayops-admin`: 관리자용 Next.js 15.5 앱. React 19, TypeScript 5, Tailwind CSS 4 기반으로 구성.
- docker-compose.yml: API, Web, Azure SQL Edge를 묶어 로컬 실행.

## 로컬 실행 (Docker Compose)

```bash
docker compose up --build
```

- Web: `http://localhost:5101`
- API: `http://localhost:5100`
- DB: `localhost:1433` (Azure SQL Edge, 개발용 설정)
- 참고: API 헬스체크는 `/weatherforecast` 기본 엔드포인트를 사용 중

## 기술 스택

- 백엔드: .NET 10, ASP.NET Core, xUnit, coverlet
- 프런트엔드: Next.js 15.5, React 19, TypeScript 5, Tailwind CSS 4
- 인프라: Docker Compose 3.9, Azure SQL Edge(로컬용)

## 향후 계획 (Roadmap)

- [ ] 도메인 모델링: 예약 애그리게이트(요청 → 확정 → 체크인/체크아웃 → 취소/노쇼) 상태 전이, 객실/재고 애그리게이트(기간 단위 가용성 계산·광역 잠금), 요금 정책 값 객체(시즌/프로모션 우선순위)로 불변 조건 정립
- [ ] API 기능: 예약 생성/수정/취소, 체크인·체크아웃, 가용 객실/요금 조회, 예약 번호·게스트명 검색, 상태별 목록 페이징, 예약 변경 시 동시성 제어(ETag/rowversion 기반)
- [ ] 성능: 핵심 인덱스 설계(예약 상태+체크인 날짜, 객실ID+기간 범위), projection 기반 조회로 N+1 최소화, CQRS-lite로 읽기/쓰기 분리, 조회 전용 캐싱, 만료 예약/보류 해제 배치 비동기 처리
- [ ] 테스트: 도메인/응용 서비스 단위 테스트 확장 및 커버리지 수집

## 비고

- 포트폴리오 데모 목적이므로 개발용 자격증명이 compose에 포함되어 있습니다. 실제 운영 환경에서는 안전한 비밀 관리 솔루션을 사용해야 합니다.
