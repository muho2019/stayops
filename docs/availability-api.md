# 가용성(Availability) 계산 API 설계서

객실 타입/객실 단위 가용성을 날짜 범위로 계산하는 API 설계입니다. 예약 중복 방지와 동시성 제어를 전제로 하며, 예약/객실 상태를 모두 고려해 가용 객실 수를 제공합니다.

## 1. 범위 및 전제
- 버전: `/api/v1` 프리픽스.
- 인증/권한: Admin/Staff 조회 가능. 공개 API 확장은 별도 경로(`/public/...`)로 분리.
- 계산 기준: 호텔 로컬 타임존 기준의 날짜(체크인/체크아웃)로 범위를 받으며, `[from, to)` 반열린 구간.
- 단위: 기본은 객실 타입별 집계. 필요 시 객실별 세부 응답 옵션 제공.
- 포함/제외: `Active` 상태 객실만 가용성 산정. `Inactive`/`OutOfService`/`Dirty` 제외(Dirty는 정책에 따라 선택 제외 가능).

## 2. 데이터 소스 & 제약
- 객실(Room): `status`(`Active/Inactive/OutOfService`), `housekeepingStatus`(`Clean/Dirty`).
- 객실 타입(RoomType): `status`(`Active/Inactive`). `Inactive` 타입은 가용성 0.
- 예약(Reservation): 상태 `Confirmed` 또는 `CheckedIn`은 재고 차감. `Pending` 차감 여부는 정책(기본: 차감 안 함). `Cancelled/CheckedOut` 미차감.
- 유니크 제약: 예약 테이블에 `(roomId, date)` 또는 기간 겹침 금지 제약(도메인/DB 레벨)으로 중복 예약 방지.

## 3. 계산 규칙(초기)
1. 집계 대상 객실: `hotelId` 일치, `Room.status == Active`, `RoomType.status == Active`.
2. 차감 대상 예약: 상태 `Confirmed`/`CheckedIn`이고, 기간이 조회 범위와 겹치는 레코드.
3. 객실별 차감 후 타입별 합산: `available = activeRooms - reservedRooms` (0 미만이면 0으로 클램프).
4. `Dirty` 제외 여부: 기본 포함, 옵션 `excludeDirty=true` 시 차감.
5. 동시성: 예약 생성/변경은 트랜잭션 + 유니크 인덱스/rowversion으로 보호. 가용성 조회는 읽기 전용이므로 캐시 가능.

## 4. 엔드포인트 설계

### 4.1 타입별 가용성 조회(집계)
- `GET /api/v1/hotels/{hotelId}/availability`
  - 쿼리: `from`(date, 필수), `to`(date, 필수), `roomTypeId`(옵션), `excludeDirty`(bool, 기본 false).
  - 응답: 날짜별 객실 타입 집계 배열.
  - 예: `{ date, roomTypeId, roomTypeName, totalRooms, unavailableRooms, availableRooms }`

### 4.2 객실별 세부 가용성(옵션)
- `GET /api/v1/hotels/{hotelId}/availability/rooms`
  - 쿼리: `from`, `to`, `roomTypeId`(옵션), `status` 필터(기본 Active), `excludeDirty`.
  - 응답: 날짜별로 예약 여부 포함한 객실 리스트 또는 `roomId`별 예약 겹침 정보.

### 4.3 예약 가능 여부 체크(단건 빠른 조회)
- `GET /api/v1/hotels/{hotelId}/availability/check`
  - 쿼리: `roomTypeId`, `from`, `to`, `excludeDirty`.
  - 응답: `{ isAvailable: bool, availableCount: number, reason?: string }`.
  - 예약 생성 UI/워크플로우에서 사전 검증 용도.

## 5. 요청/응답 예시

### 타입별 가용성 조회
`GET /api/v1/hotels/{hotelId}/availability?from=2025-12-24&to=2025-12-27`
응답 200:
```json
{
  "hotelId": "guid",
  "currency": "KRW",
  "items": [
    {
      "date": "2025-12-24",
      "roomTypeId": "guid",
      "roomTypeName": "Deluxe Twin",
      "totalRooms": 10,
      "unavailableRooms": 8,
      "availableRooms": 2
    },
    {
      "date": "2025-12-25",
      "roomTypeId": "guid",
      "roomTypeName": "Deluxe Twin",
      "totalRooms": 10,
      "unavailableRooms": 9,
      "availableRooms": 1
    }
  ]
}
```

### 단건 예약 가능 여부 체크
`GET /api/v1/hotels/{hotelId}/availability/check?roomTypeId=...&from=2025-12-24&to=2025-12-26`
응답 200:
```json
{
  "roomTypeId": "guid",
  "from": "2025-12-24",
  "to": "2025-12-26",
  "availableCount": 2,
  "isAvailable": true
}
```

## 6. 밸리데이션/에러 코드(샘플)
- `InvalidDateRange`: `from >= to` 또는 범위 초과 시 400.
- `RoomTypeInactive`/`HotelInactive`: 비활성 리소스 요청 시 400/403.
- `DateRangeTooLong`: 최대 조회 일수 초과 시 400(예: 31일 제한).
- `InvalidFilter`: 잘못된 상태 필터 값 시 400.

## 7. 확장 고려
- 퍼블릭 가용성: `/public/availability`로 레이트리밋, idempotency 키(예약 생성 시)와 함께 노출.
- 동시성 보호: 예약 생성 시 `(roomId, date)` 유니크 인덱스 + 트랜잭션 사용, 또는 타입 단위 재고 테이블/락 도입.
- 성능: 일자/타입 인덱스 최적화, 읽기 전용 캐시(LRU) 적용, ETL된 projection 테이블로 조회 분리(CQRS-lite).
- Dirty 처리: 체크아웃 시 `Dirty` 자동 전환, 하우스키핑 완료 시 `Clean` 전환. `excludeDirty` 정책 플래그로 제어.
- 멀티 호텔: 모든 요청에 `hotelId` 유지, 키 충돌 방지 위해 `(hotelId, roomId)` 스코프.
