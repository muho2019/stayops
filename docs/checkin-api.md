# Check-in/Checkout API 설계서

체크인/체크아웃 운영을 위한 HTTP API 설계입니다. 예약/객실 상태와 연동하여 중복·잘못된 상태 전이를 방지하고, 자동 객실 배정 및 하우스키핑 연계를 포함합니다.

## 1. 범위 및 전제
- 버전: `/api/v1` 프리픽스.
- 인증/권한: Admin/Staff 사용. Staff 권한은 정책 플래그로 제어.
- 타임존: 호텔 로컬 기준. 날짜/시간은 ISO 8601.
- 상태 전이: `Confirmed -> CheckedIn -> CheckedOut` 또는 `Confirmed/Pending -> Cancelled`(별도 취소 API).
- 자동 배정: 체크인 시 객실 미배정이면 동일 객실 타입 내 가용 객실 자동 배정(가용성 검증 포함).

## 2. 리소스 모델(관련 필드)
- `Reservation`
  - `id`, `hotelId`, `reservationNumber`, `status`, `roomTypeId`, `roomId?`, `checkInDate`, `checkOutDate`, `adults/children`, `totalAmount`, `notes`, `createdAt/updatedAt`, `version`(rowversion), `checkedInAt?`, `checkedOutAt?`.
- `Room`
  - `id`, `roomTypeId`, `number`, `status`(`Active/Inactive/OutOfService`), `housekeepingStatus`(`Clean/Dirty`), `version`.

## 3. 엔드포인트

### 3.1 체크인
- `POST /api/v1/reservations/{reservationId}/checkin`
  - Headers: `If-Match`(선택, rowversion), `Idempotency-Key`(선택), `X-Request-Id`(선택).
  - Body: `actualCheckIn`(datetime, optional; default server time), `note`(optional).
  - Behavior:
    1. 상태 검증: `Confirmed`만 허용, 아니면 400 `InvalidStatusTransition`.
    2. 객실 배정: `roomId` 없으면 가용성 조회 후 자동 배정(동일 타입). 없으면 409 `NoAvailability`.
    3. 예약 상태 `CheckedIn`으로 전이, `checkedInAt` 기록.
  - Errors: `NoAvailability`, `RoomInactive`, `RoomOutOfService`, `ConcurrencyConflict`(rowversion 불일치), `RoomTypeInactive`.

### 3.2 체크아웃
- `POST /api/v1/reservations/{reservationId}/checkout`
  - Headers: `If-Match`(선택), `Idempotency-Key`(선택), `X-Request-Id`(선택).
  - Body: `actualCheckOut`(datetime, optional), `note`(optional).
  - Behavior:
    1. 상태 검증: `CheckedIn`만 허용, 아니면 400 `InvalidStatusTransition`.
    2. 상태를 `CheckedOut`으로 전이, `checkedOutAt` 기록.
    3. 배정된 객실의 `housekeepingStatus`를 `Dirty`로 전환(하우스키핑 연계).
  - Errors: `InvalidStatusTransition`, `RoomNotAssigned`(옵션 정책), `ConcurrencyConflict`.

### 3.3 당일 운영 목록(옵션)
- `GET /api/v1/hotels/{hotelId}/frontdesk/today`
  - Query: `date`(default today), `status` 필터(`DueCheckIn|DueCheckOut`).
  - Response: 체크인/체크아웃 예정 예약 리스트(게스트, 타입, 배정 객실, 상태, 예상 시간 포함).

## 4. 요청/응답 예시

### 체크인
`POST /api/v1/reservations/{reservationId}/checkin`
```json
{
  "actualCheckIn": "2025-12-24T15:10:00+09:00",
  "note": "Late arrival"
}
```
200 OK:
```json
{
  "id": "guid",
  "status": "CheckedIn",
  "roomId": "guid",
  "checkedInAt": "2025-12-24T15:10:00+09:00"
}
```

### 체크아웃
`POST /api/v1/reservations/{reservationId}/checkout`
```json
{
  "actualCheckOut": "2025-12-26T11:05:00+09:00",
  "note": "Left early"
}
```
200 OK:
```json
{
  "id": "guid",
  "status": "CheckedOut",
  "checkedOutAt": "2025-12-26T11:05:00+09:00",
  "roomId": "guid"
}
```

## 5. 밸리데이션/에러 코드
- `InvalidStatusTransition` (400): 허용되지 않는 상태에서 체크인/체크아웃 시도.
- `NoAvailability` (409): 체크인 자동 배정 가능한 객실 없음.
- `RoomInactive` / `RoomOutOfService` (400/409): 비활성/점검 객실 배정 시도.
- `RoomTypeInactive` (400/403): 비활성 객실 타입.
- `ConcurrencyConflict` (409): rowversion/ETag 불일치.
- `RoomNotAssigned` (400, 옵션): 체크아웃 시 객실이 배정되지 않은 경우 정책에 따라 반환.

## 6. 확장 고려
- Late checkout / early checkin: 시간대별 추가 요금 계산을 요금 API와 연계.
- 키 발급/신분 확인: 체크인 시 OCR/신분 확인 플로우 연동(미구현, 훅만 제공).
- 하우스키핑 연계: 체크아웃 시 `Dirty` 전환, 청소 완료 시 `Clean` 전환(PATCH housekeeping API).
- 이벤트 발행: 체크인/체크아웃 도메인 이벤트 → 대시보드/알림 시스템 연동.
- 퍼블릭 노출 없음: 모두 내부(Admin/Staff) API.
