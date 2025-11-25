# 호텔/객실/객실 타입 관리 API 설계서

본 문서는 호텔 인벤토리 관리(호텔, 객실 타입, 객실)를 위한 HTTP API 설계를 정의합니다. 인증(JWT 기반) 기능은 이미 존재한다고 가정하며, 권한 모델과 밸리데이션 규칙을 명시합니다.

## 1. 범위 및 전제
- 대상 리소스: 호텔(단일 지점 우선), 객실 타입, 객실.
- 버전: `/api/v1` 프리픽스 사용.
- 인증/권한: Bearer JWT. 기본 역할은 Admin(전체 관리), Staff(조회 위주).
- 다중 호텔 확장은 ERD 상 고려하지만 초기 구현은 단일 호텔에 한정. API 경로는 호텔 ID를 포함해 확장 가능하도록 설계.
- 시간/날짜는 ISO 8601, 타임존은 호텔 로컬 기준(문서화).

## 2. 리소스 모델
- `Hotel`
  - `id`(guid), `code`(문자열, 전역 유니크), `name`, `timezone`, `status`(`Active/Inactive`), `createdAt/updatedAt`.
- `RoomType`
  - `id`(guid), `hotelId`, `name`, `description`, `capacity`(정수), `baseRate`(decimal, KRW), `status`(`Active/Inactive`), `createdAt/updatedAt`.
- `Room`
  - `id`(guid), `hotelId`, `roomTypeId`, `number`(문자열, 예: "101"), `floor`(선택), `status`(`Active/Inactive/OutOfService`), `housekeepingStatus`(`Clean/Dirty`), `createdAt/updatedAt`, `version`(낙관적 락용 rowversion).
- 상태 규칙
  - `Room.status != Active`인 경우 예약 생성/배정 불가.
  - `OutOfService`는 일시적 점검/유지보수 상태로, 가용/예약 모두 차단.

## 3. 공통 규칙
- 권한
  - Admin: 전체 CRUD, 상태 변경 가능.
  - Staff: 조회 가능. 상태 변경은 운영 정책에 따라 제한(기본: 불가).
- 응답 포맷: 성공 시 리소스 JSON. 오류 시 `{ "error": { "code": "string", "message": "string", "details": {...} } }`.
- 페이징: `page`(1-base), `pageSize` 쿼리 사용. 응답은 `items`, `total`, `page`, `pageSize`.
- 정렬: 기본 `createdAt desc`. 필요 시 `sort`(필드), `order`(`asc/desc`).
- 낙관적 락: `RowVersion`을 `If-Match` 헤더(또는 본문 `rowVersion`)로 받아 업데이트 시 동시성 충돌을 409로 반환.
- 감사: `X-Request-Id` 수신 시 로깅/추적에 사용.

## 4. 엔드포인트 설계

### 4.1 호텔 (단일 지점 우선)
- `GET /api/v1/hotels` (Admin)  
  - 현재 등록된 호텔 목록 조회(단일 지점이라도 확장 대비).  
  - 필터: `status`.
- `POST /api/v1/hotels` (Admin)  
  - 본사/지점 등록. 단일 호텔 제한 시에도 일관된 인터페이스 유지.  
  - Body: `code`, `name`, `timezone`, `status`.
- `GET /api/v1/hotels/{hotelId}` (Admin/Staff)
- `PUT /api/v1/hotels/{hotelId}` (Admin)  
  - 전체 업데이트. `status` 변경 시 하위 리소스 생성 제한 정책 정의 필요.
- `DELETE /api/v1/hotels/{hotelId}` (Admin)  
  - 기본 정책: 소프트 삭제 or 비활성화로 대체 권장.

### 4.2 객실 타입
- `GET /api/v1/hotels/{hotelId}/room-types` (Admin/Staff)  
  - 필터: `status`. 페이징/정렬 지원.
- `POST /api/v1/hotels/{hotelId}/room-types` (Admin)  
  - Body: `name`(필수), `description`, `capacity`(1~10), `baseRate`(>=0), `status`(`Active/Inactive`).
- `GET /api/v1/room-types/{roomTypeId}` (Admin/Staff)
- `PUT /api/v1/room-types/{roomTypeId}` (Admin)  
  - 전체 업데이트. `status`가 `Inactive`이면 신규 객실 생성 불가.
- `PATCH /api/v1/room-types/{roomTypeId}/status` (Admin)  
  - Body: `status`. 예약/가용성과의 연동 정책을 명시(예: `Inactive` 시 기존 예약 유지, 신규 예약 차단).
- `DELETE /api/v1/room-types/{roomTypeId}` (Admin)  
  - 정책: 실제 삭제 대신 `Inactive` 또는 소프트 삭제 권장. 연관 객실/예약 제약 처리.

### 4.3 객실
- `GET /api/v1/hotels/{hotelId}/rooms` (Admin/Staff)  
  - 필터: `status`, `roomTypeId`, `housekeepingStatus`, `number`(부분 일치), `floor`. 페이징 지원.
- `POST /api/v1/hotels/{hotelId}/rooms` (Admin)  
  - Body: `roomTypeId`(필수), `number`(호텔 내 유니크), `floor`, `status`(`Active/Inactive/OutOfService`), `housekeepingStatus`(기본 `Clean`).
- `GET /api/v1/rooms/{roomId}` (Admin/Staff)
- `PUT /api/v1/rooms/{roomId}` (Admin)  
  - 전체 업데이트. `If-Match` 헤더로 rowversion 처리.
- `PATCH /api/v1/rooms/{roomId}/status` (Admin)  
  - Body: `status`. `Inactive/OutOfService` 전환 시 활성 예약 존재 여부 검사 → 409. 필요 시 적용 시점을 미래로 예약하는 옵션은 확장 사항.
- `PATCH /api/v1/rooms/{roomId}/housekeeping-status` (Admin/Staff*)  
  - Body: `housekeepingStatus`. Staff 허용 여부는 운영 정책 플래그로 제어.
- `DELETE /api/v1/rooms/{roomId}` (Admin)  
  - 기본 정책: 소프트 삭제 or 비활성화로 대체.

### 4.4 조회/요약 유틸
- `GET /api/v1/hotels/{hotelId}/rooms/summary` (Admin/Staff)  
  - 용도: 당일 운영 요약.  
  - 응답 예시: 객실 타입별 `{ roomTypeId, roomTypeName, total, active, outOfService, dirty }`.
- `GET /api/v1/hotels/{hotelId}/rooms/{roomId}/history` (Admin)  
  - 상태 변경 히스토리(감사 로그) 노출. 페이징.

## 5. 요청/응답 예시

### 객실 타입 생성
`POST /api/v1/hotels/{hotelId}/room-types`
```json
{
  "name": "Deluxe Twin",
  "description": "City view, 2 guests",
  "capacity": 2,
  "baseRate": 150000,
  "status": "Active"
}
```
응답 201:
```json
{
  "id": "guid",
  "hotelId": "guid",
  "name": "Deluxe Twin",
  "description": "City view, 2 guests",
  "capacity": 2,
  "baseRate": 150000,
  "status": "Active",
  "createdAt": "2025-11-24T10:00:00+09:00",
  "updatedAt": "2025-11-24T10:00:00+09:00"
}
```

### 객실 상태 변경(OutOfService)
`PATCH /api/v1/rooms/{roomId}/status`
```json
{ "status": "OutOfService", "reason": "Plumbing repair" }
```
- 비즈니스 규칙: 활성 예약이 있으면 409(`ActiveReservationExists`).

## 6. 밸리데이션 및 에러 코드(샘플)
- `RoomNumberConflict`: 동일 호텔 내 `number` 중복 시 409.
- `RoomTypeInactive`: `Inactive` 타입에 대한 객실 생성 시 400.
- `RoomInactive`: `Inactive/OutOfService` 객실에 예약 배정 시 409.
- `HotelInactive`: 비활성 호텔에 대한 리소스 생성 시 400/403.
- `InvalidStatusTransition`: 허용되지 않는 상태 전이 시 400.
- `ConcurrencyConflict`: rowversion 불일치 시 409.
- 공통 입력 검증: 필수 필드 누락, 음수 금액, capacity 범위 초과 시 400.

## 7. 권한 매트릭스(요약)
- Admin: 호텔/객실 타입/객실 전체 CRUD, 상태 변경, 소프트 삭제, 감사 로그 조회.
- Staff: 기본 조회. 하우스키핑 상태 변경은 운영 설정에 따라 허용 옵션. 생성/삭제는 불가.

## 8. 확장 고려
- 다중 호텔: 모든 엔드포인트에 `hotelId` 유지, DB 유니크 키를 `(hotelId, number)`로 정의.
- 소프트 삭제: `IsDeleted` 플래그와 필터를 인프라 레벨에 공통 적용.
- 감사/이력: 상태 변경, 삭제, 중요한 필드 변경은 도메인 이벤트로 감사 테이블에 기록.
- 캐싱/조회 최적화: `rooms/summary` 등 읽기 전용 쿼리는 projection 뷰/인덱스를 분리해 CQRS-lite 적용 가능.
