# 하우스키핑(객실 상태) API 설계서

객실 하우스키핑 상태를 관리하기 위한 경량 API 설계입니다. 청소/점검 상태를 객실 가용성과 일관되게 유지하고, 운영 현황을 조회할 수 있도록 합니다.

## 1. 범위 및 전제
- 버전: `/api/v1` 프리픽스.
- 인증/권한: Admin/Staff. Staff 권한은 정책 플래그로 제어.
- 타임존: 호텔 로컬 기준, ISO 8601 포맷.
- `Room.status`는 운영 가용성(`Active/Inactive/OutOfService`), `Room.housekeepingStatus`는 청소 상태(`Clean/Dirty`)를 나타냅니다.
- 체크아웃 시 `housekeepingStatus=Dirty` 자동 전환은 체크아웃 API에서 수행하며, 본 문서는 수동 변경/조회에 집중합니다.

## 2. 리소스 모델(관련 필드)
- `Room`
  - `id`, `hotelId`, `roomTypeId`, `number`, `status`(`Active/Inactive/OutOfService`), `housekeepingStatus`(`Clean/Dirty`), `version`(rowversion), `updatedAt`.
- `HousekeepingHistory`(옵션)
  - `id`, `roomId`, `fromStatus`, `toStatus`, `changedBy`, `note`, `changedAt`.

## 3. 엔드포인트

### 3.1 객실 보드(상태 조회)
- `GET /api/v1/hotels/{hotelId}/rooms/board`
  - 쿼리: `status`(Room.status), `housekeepingStatus`, `roomTypeId`, `floor`, `onlyAssigned`(배정된 객실만).
  - 응답 아이템: `{ roomId, roomNumber, roomTypeName, status, housekeepingStatus, assignedReservationId? }`.

### 3.2 하우스키핑 상태 변경
- `PATCH /api/v1/rooms/{roomId}/housekeeping-status`
  - 헤더: `If-Match`(선택, rowversion).
  - Body: `housekeepingStatus`(`Clean|Dirty`), `note`(선택), `changedBy`(선택: 직원 식별자).
  - 검증:
    - 기본 허용: `Room.status == Active`.
    - `OutOfService` 허용 여부는 정책 플래그로 결정.
    - `Inactive`는 기본 차단.
  - 결과: 상태 변경 후 `updatedAt` 갱신, 필요 시 히스토리 기록.

### 3.3 운영/청소 요약
- `GET /api/v1/hotels/{hotelId}/rooms/summary`
  - 용도: 당일 운영·청소 현황 스냅샷.
  - 예시: `{ roomTypeId, roomTypeName, total, active, outOfService, clean, dirty }`.

### 3.4 하우스키핑 히스토리(옵션)
- `GET /api/v1/rooms/{roomId}/housekeeping-history`
  - 페이징: `page`, `pageSize`.
  - 응답: 상태 변경 주체/시각/변경 전후 상태 기록.

## 4. 요청/응답 예시

### 상태 변경
`PATCH /api/v1/rooms/{roomId}/housekeeping-status`
```json
{ "housekeepingStatus": "Clean", "note": "Completed by staff#123" }
```
200 OK:
```json
{
  "roomId": "guid",
  "housekeepingStatus": "Clean",
  "updatedAt": "2025-11-25T11:00:00+09:00"
}
```

### 보드 조회
`GET /api/v1/hotels/{hotelId}/rooms/board?housekeepingStatus=Dirty`
200 OK:
```json
{
  "items": [
    {
      "roomId": "guid",
      "roomNumber": "1205",
      "roomTypeName": "Deluxe Twin",
      "status": "Active",
      "housekeepingStatus": "Dirty",
      "assignedReservationId": "guid"
    }
  ]
}
```

## 5. 밸리데이션/에러 코드
- `InvalidStatusTransition` (400): 허용되지 않는 상태 변경.
- `RoomInactive` / `RoomOutOfService` (400/409): 정책상 변경 불가.
- `ConcurrencyConflict` (409): rowversion/ETag 불일치.
- `RoomNotFound` (404): 존재하지 않는 객실 ID.

## 6. 확장 고려
- OutOfService 워크플로우: 시작/종료, 사유, 담당자, 기간 중 예약/배정 차단 정책.
- Task/assignment: 청소 작업 할당, 완료 시각, 메모/사진 첨부.
- 이벤트/알림: 상태 변경 이벤트 발행 → 대시보드/푸시 연계.
- 퍼블릭 노출 없음: 내부(Admin/Staff) API로 제한.
