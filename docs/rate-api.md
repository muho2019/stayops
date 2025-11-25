# 요금/가격 정책(Daily Rate) API 설계서

Daily Rate(날짜별 요금) 기반의 가격 정책 API 설계입니다. 객실 타입 단위로 날짜별 요금을 정의/조회하며, 통화는 KRW를 기본으로 합니다.

## 1. 범위 및 전제
- 버전: `/api/v1` 프리픽스.
- 인증/권한: Bearer JWT. Admin만 작성/수정/삭제 가능, Staff는 조회 전용.
- 모델: 객실 타입별 기본 요금 + 날짜별 예외 요금(Daily Override).
- 타임존: 호텔 로컬 타임존 기준 날짜(체크인 날짜 단위). ISO 8601 사용.
- 과세: 초기 버전은 요금에 세금/봉사료 포함으로 가정(문자열 필드로 표시). 필요 시 별도 필드로 분리 확장.

## 2. 리소스 모델
- `RatePlan`(경량): 초기 버전은 호텔 단일 기본 플랜 1개를 전제로, 추후 다중 플랜 확장 가능.
  - `id`(guid), `hotelId`, `name`(예: "Base Daily Rate"), `currency`(고정: KRW), `isDefault`.
- `DailyRate`
  - `id`(guid), `hotelId`, `roomTypeId`, `date`(YYYY-MM-DD), `amount`(decimal >= 0), `taxIncluded`(bool, 기본 true), `notes`(선택), `createdAt/updatedAt`.
  - 유니크 제약: `(hotelId, roomTypeId, date)`.
- `RoomType`
  - `baseRate`(decimal >= 0) 보유. DailyRate 없을 경우 fallback.

## 3. 가격 결정 규칙(초기)
1. 요청된 `date`와 `roomTypeId`에 대해 DailyRate가 존재하면 해당 금액을 사용.
2. DailyRate가 없으면 `RoomType.baseRate`를 사용.
3. `Inactive` RoomType 또는 호텔에 대해서는 요금 조회/예약 차단.
4. 과거 날짜 업데이트는 기본 차단(옵션: Admin만 허용 플래그).
5. 통화는 KRW 고정. 다중 통화는 확장 사항.

## 4. 엔드포인트 설계

### 4.1 Daily Rate CRUD
- `GET /api/v1/hotels/{hotelId}/room-types/{roomTypeId}/daily-rates` (Admin/Staff)
  - 필터: `from`(date), `to`(date) 범위 필수. 페이징 없음(범위 제한).
- `POST /api/v1/hotels/{hotelId}/room-types/{roomTypeId}/daily-rates` (Admin)
  - Body: `date`(YYYY-MM-DD), `amount`(>=0), `taxIncluded`(bool, default true), `notes`.
  - 제약: 동일 `(hotelId, roomTypeId, date)` 중복 시 409 `DailyRateConflict`.
- `PUT /api/v1/daily-rates/{dailyRateId}` (Admin)
  - Body: `date`, `amount`, `taxIncluded`, `notes`. 전체 업데이트. `If-Match`로 rowversion 대응 가능.
- `DELETE /api/v1/daily-rates/{dailyRateId}` (Admin)
  - 정책: 하드 삭제 가능. 필요 시 소프트 삭제로 확장.

### 4.2 요금 조회(예약/가용성 연동용)
- `GET /api/v1/hotels/{hotelId}/rates/quote`
  - 쿼리: `roomTypeId`, `from`, `to` (체크인/체크아웃 날짜), `includeBreakdown`(bool).
  - 응답: 날짜별 단가 배열 + 총액. DailyRate 존재 시 우선, 없으면 baseRate 사용.
- `GET /api/v1/hotels/{hotelId}/room-types/{roomTypeId}/rates`
  - 쿼리: `from`, `to`. DailyRate + fallback 금액을 함께 반환(조회 용도).

## 5. 요청/응답 예시

### Daily Rate 생성
`POST /api/v1/hotels/{hotelId}/room-types/{roomTypeId}/daily-rates`
```json
{
  "date": "2025-12-24",
  "amount": 180000,
  "taxIncluded": true,
  "notes": "Xmas peak"
}
```
응답 201:
```json
{
  "id": "guid",
  "hotelId": "guid",
  "roomTypeId": "guid",
  "date": "2025-12-24",
  "amount": 180000,
  "taxIncluded": true,
  "notes": "Xmas peak",
  "createdAt": "2025-11-25T10:00:00+09:00",
  "updatedAt": "2025-11-25T10:00:00+09:00"
}
```

### 요금 견적 조회
`GET /api/v1/hotels/{hotelId}/rates/quote?roomTypeId=...&from=2025-12-24&to=2025-12-27&includeBreakdown=true`
응답 200:
```json
{
  "roomTypeId": "guid",
  "currency": "KRW",
  "nights": 3,
  "daily": [
    { "date": "2025-12-24", "amount": 180000, "source": "DailyRate" },
    { "date": "2025-12-25", "amount": 180000, "source": "DailyRate" },
    { "date": "2025-12-26", "amount": 150000, "source": "BaseRate" }
  ],
  "total": 510000,
  "taxIncluded": true
}
```

## 6. 밸리데이션/에러 코드(샘플)
- `DailyRateConflict`: 동일 날짜 중복 등록 시 409.
- `RoomTypeInactive`/`HotelInactive`: 비활성 리소스에 대한 생성/조회 시 400/403.
- `PastDateNotAllowed`: 과거 날짜 수정/생성 차단 시 400.
- `InvalidDateRange`: `from >= to` 또는 범위 초과 시 400.
- `ConcurrencyConflict`: `If-Match` rowversion 불일치 시 409.

## 7. 확장 고려
- 다중 RatePlan: OTA/프로모션/멤버십 플랜 추가 시 `ratePlanId`를 DailyRate와 Quote API에 포함.
- 세금/수수료 분리: `tax`, `serviceFee`, `discount` 필드를 별도 브레이크다운으로 확장.
- 시즌/요일 요금: DailyRate 테이블을 시즌/요일 규칙 테이블로 대체하거나 병행 지원.
- 캐싱: Quote API는 읽기 전용 캐시(LRU) 가능. DailyRate 변경 시 캐시 무효화 이벤트 발행.
