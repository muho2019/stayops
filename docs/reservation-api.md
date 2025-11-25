# 예약 생성/조회/변경/취소 API 설계서

예약 라이프사이클(Pending → Confirmed → CheckedIn → CheckedOut/Cancelled) 기반의 API 설계입니다. 객실/객실 타입 및 요금/가용성과 연동해 중복 예약을 방지하고, 상태 전이를 명확히 정의합니다.

## 1. 범위 및 전제
- 버전: `/api/v1` 프리픽스.
- 인증/권한: Admin/Staff 사용. Admin 전체, Staff는 생성/조회/변경/취소 가능(권한 플래그로 세분화 가능).
- 가용성/요금 연동: 예약 생성/변경 시 가용성 API와 요금 견적(Quote)로 검증 후 확정. 중복 예약 방지 위해 DB 제약 사용.
- 타임존: 호텔 로컬 기준. 날짜는 `[checkIn, checkOut)` 반열린 구간.
- 상태: `Pending`, `Confirmed`, `CheckedIn`, `CheckedOut`, `Cancelled`(사유: `User`, `NoShow`, `System`).
- 낙관적 락: `If-Match` 헤더(rowversion)로 동시성 제어. 불일치 시 409 `ConcurrencyConflict`.
- Idempotency: `Idempotency-Key` 헤더(선택)로 동일 요청 재시도 안전성 확보.

## 2. 리소스 모델
- `Reservation`
  - `id`(guid), `hotelId`, `reservationNumber`(문자열, 읽기 전용), `guestName`, `guestPhone`, `roomTypeId`, `roomId`(선택), `checkInDate`, `checkOutDate`, `adults`, `children`, `status`, `totalAmount`(KRW), `currency`(KRW 고정), `notes`, `createdAt/updatedAt`, `version`(rowversion).
  - 상태 전이 규칙: `Pending → Confirmed → CheckedIn → CheckedOut` 또는 `Pending/Confirmed → Cancelled`.
- `Payment`/`Invoice`는 범위 외(추후 확장).

## 3. 엔드포인트 설계

### 3.1 예약 생성
- `POST /api/v1/hotels/{hotelId}/reservations`
  - 헤더: `Idempotency-Key`(선택), `X-Request-Id`(선택).
  - Body:
    - `guestName`(필수), `guestPhone`(필수)
    - `roomTypeId`(필수), `roomId`(선택; 없으면 자동 배정)
    - `checkInDate`, `checkOutDate`(필수, `checkIn < checkOut`)
    - `adults`(>=1), `children`(>=0)
    - `notes`(선택)
  - 동작: 가용성 검증 → 요금 계산(Quote) → 방 배정(자동/지정) → `status=Confirmed` 기본.
  - 오류: 가용성 없음 409 `NoAvailability`, 중복 예약 제약 409 `OverlapConflict`, 비활성 객실/타입 400/403.

### 3.2 예약 목록 조회
- `GET /api/v1/hotels/{hotelId}/reservations`
  - 쿼리: `from`, `to`(체크인 기준 필터), `status`, `roomTypeId`, `roomId`, `guestName`(부분 일치), `reservationNumber`, `page`, `pageSize`, `sort`(`checkInDate|createdAt`), `order`(`asc|desc`).
  - 응답: 페이징 리스트 + 요약 필드.

### 3.3 예약 상세 조회
- `GET /api/v1/reservations/{reservationId}`
  - 응답: 예약 전체 필드 + 상태 변경 히스토리(옵션) + 요금 브레이크다운(옵션).

### 3.4 예약 변경
- 날짜/인원 변경
  - `PATCH /api/v1/reservations/{reservationId}/dates`
    - Body: `checkInDate`, `checkOutDate`, `adults`, `children`(옵션)
    - 가용성 재검증 후 업데이트. 실패 시 409 `NoAvailability`.
- 객실 배정/변경
  - `PATCH /api/v1/reservations/{reservationId}/room`
    - Body: `roomId`(동일 타입 내 변경). 타입 변경은 새 예약 처리 권장.
    - 활성 예약 겹침 검사. 불가 시 409 `OverlapConflict`.
- 일반 정보 변경
  - `PATCH /api/v1/reservations/{reservationId}`
    - Body: `guestName`, `guestPhone`, `notes` 등 비핵심 필드.

### 3.5 예약 취소
- `POST /api/v1/reservations/{reservationId}/cancel`
  - Body: `reason`(`User|NoShow|System`), `comment`(선택).
  - 상태 규칙: `Confirmed`/`Pending` → `Cancelled`. 이미 체크인된 예약은 400 `InvalidStatusTransition`.

### 3.6 상태 전이(체크인/체크아웃)
- 체크인: `POST /api/v1/reservations/{reservationId}/checkin`
  - Body: `actualCheckIn`(datetime, 선택). 상태 `Confirmed → CheckedIn`.
- 체크아웃: `POST /api/v1/reservations/{reservationId}/checkout`
  - Body: `actualCheckOut`(datetime, 선택). 상태 `CheckedIn → CheckedOut`. 체크아웃 시 객실 `housekeepingStatus`를 `Dirty`로 전환.

## 4. 요청/응답 예시

### 예약 생성
`POST /api/v1/hotels/{hotelId}/reservations`
```json
{
  "guestName": "Jane Doe",
  "guestPhone": "010-1234-5678",
  "roomTypeId": "guid",
  "checkInDate": "2025-12-24",
  "checkOutDate": "2025-12-26",
  "adults": 2,
  "children": 0,
  "notes": "Late arrival"
}
```
응답 201:
```json
{
  "id": "guid",
  "reservationNumber": "R-20251224-001",
  "hotelId": "guid",
  "roomTypeId": "guid",
  "roomId": "guid",
  "status": "Confirmed",
  "checkInDate": "2025-12-24",
  "checkOutDate": "2025-12-26",
  "adults": 2,
  "children": 0,
  "totalAmount": 510000,
  "currency": "KRW",
  "createdAt": "2025-11-25T10:00:00+09:00",
  "updatedAt": "2025-11-25T10:00:00+09:00"
}
```

### 예약 취소
`POST /api/v1/reservations/{reservationId}/cancel`
```json
{ "reason": "User", "comment": "Change of plan" }
```
응답 200:
```json
{
  "id": "guid",
  "status": "Cancelled",
  "cancelReason": "User",
  "cancelledAt": "2025-11-25T12:00:00+09:00"
}
```

## 5. 밸리데이션/에러 코드(샘플)
- `NoAvailability`: 가용 객실 없음(409).
- `OverlapConflict`: 동일 객실 기간 중복(409).
- `RoomInactive`/`RoomTypeInactive`/`HotelInactive`: 비활성 리소스(400/403).
- `InvalidDateRange`: `checkIn >= checkOut`(400).
- `InvalidStatusTransition`: 허용되지 않는 상태 전이(400).
- `ConcurrencyConflict`: rowversion 불일치(409).
- `IdempotencyConflict`: 동일 키에 상이한 요청(409).

## 6. 확장 고려
- Public 예약: `/public/reservations`에 idempotency 키 필수, 레이트리밋 적용.
- 결제 연동: `Payment` 상태와 함께 예약 상태 전이 묶기.
- 변경 히스토리: 상태/날짜/객실 변경을 도메인 이벤트로 감사 테이블에 기록, `/history` 엔드포인트로 노출 가능.
- ETag/rowversion: GET 응답에 `ETag` 포함해 PATCH에 `If-Match` 사용.
