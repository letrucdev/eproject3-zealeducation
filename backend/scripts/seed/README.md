# Seed scripts

Hai bộ seed nhỏ, mỗi file phụ trách một bảng. Chạy theo thứ tự để dựng dữ
liệu test cho UI rồi (nếu cần) thêm dữ liệu chart dày để các biểu đồ dashboard
trông giống dữ liệu thật.

```
seed/
├── README.md                   ← bạn đang đọc
├── demo/                       ← dữ liệu vừa đủ test giao diện
│   ├── 01-courses.sql
│   ├── 02-user-accounts.sql
│   ├── 03-staff.sql
│   ├── 04-faculty.sql
│   ├── 05-candidates.sql
│   ├── 06-batches.sql
│   ├── 07-enrollments.sql
│   ├── 08-fee-structures.sql
│   ├── 09-installment-plans.sql
│   ├── 10-payment-transactions.sql
│   ├── 11-class-sessions.sql
│   ├── 12-attendance-records.sql
│   ├── 13-examinations.sql
│   ├── 14-exam-results.sql
│   ├── 15-course-enquiries.sql
│   ├── 16-fines.sql
│   ├── 17-feedbacks.sql
│   ├── 18-certificate-applications.sql
│   └── 19-system-assets.sql
└── chart/                      ← dữ liệu dày cho dashboard charts
    ├── 01-batch-creation-trend.sql
    ├── 02-candidate-registration-trend.sql
    ├── 03-focus-batch-and-students.sql
    └── 04-focus-exams-and-results.sql
```

## Tài khoản mặc định

Mọi user seed (staff lẫn candidate) đều dùng password **`Password123!`**
(BCrypt cost 11). Username có sẵn:

| Role          | Username pattern              |
|---------------|-------------------------------|
| SystemAdmin   | `admin01`..`admin03`          |
| Incharge      | `incharge01`..`incharge03`    |
| Faculty       | `faculty01`..`faculty03`      |
| Counselor     | `counselor01`..`counselor03`  |
| AccountsStaff | `accounts01`..`accounts03`    |
| Candidate     | `candidate01`..`candidate20`  |

## Pre-requisite

Schema phải chạy migrations xong trước. Trong repo này:

```bash
# từ thư mục backend/
./scripts/migrate.sh        # apply EF Core migrations vào DB local
```

## Bộ `demo/` — dữ liệu test giao diện

Mỗi file lo đúng một bảng. **Phải chạy theo đúng số thứ tự** vì các bảng sau
tham chiếu khoá ngoại tới bảng trước (course → user → staff → batch …).
Mọi file đều idempotent (NOT EXISTS guard) → chạy lại nhiều lần vẫn an toàn.

Các điểm cần lưu ý:

- **Bỏ qua phần materials**: `study_material` và `material_download_log`
  không có trong bộ seed này.
- **Ảnh proof để rỗng**: `payment_transaction.ReceiptFilePath` và
  `BankTransferProofPath` đều NULL — UI sẽ hiển thị empty state "chưa có
  chứng từ".
- **CertificateFilePath cũng NULL** — PDF sẽ được sinh khi staff bấm approve
  trong UI.

## Bộ `chart/` — dữ liệu dày cho dashboard

Bộ này tạo ra **~400 batch** + **~200 candidate** rải đều khắp 90 ngày gần
nhất, với 5 mức độ ngày: VHIGH / HIGH / MED / LOW / VLOW. Cách phân bố này
tạo ra dao động cao-thấp rõ rệt giữa các ngày liền kề thay vì một dải dữ
liệu phẳng → biểu đồ trông sống động và giống production.

| Bucket | Active/day | Completed/day | Cancelled/day | Total/day |
|--------|-----------:|--------------:|--------------:|----------:|
| VHIGH  | 6          | 3             | 2             | 11        |
| HIGH   | 4          | 2             | 1             |  7        |
| MEDIUM | 3          | 1             | 1             |  5        |
| LOW    | 1          | 1             | 0             |  2        |
| VLOW   | 1          | 0             | 0             |  1        |

Chạy theo thứ tự:

1. `chart/01-batch-creation-trend.sql` — sinh ~400 batch với
   `batch.CreatedAt` rải đều 90 ngày (cho batch creation trend chart).
2. `chart/02-candidate-registration-trend.sql` — sinh ~200 candidate +
   user_account với `candidate.RegisteredAt` rải đều 90 ngày
   (cho candidate registration trend chart).
3. `chart/03-focus-batch-and-students.sql` — 1 batch focus
   `CHART-FOCUS-2026` + 12 học viên + enrollments.
4. `chart/04-focus-exams-and-results.sql` — 30 exam + 360 exam result
   cho batch focus (cho per-batch exam scores trend & grade distribution).

Re-run `chart/01` và `chart/02` sẽ tự xoá batch / candidate cũ của lần seed
trước rồi sinh lại theo schedule mới (DELETE WHERE CreatedBy =
'seed-chart-data' AND code LIKE 'CHART-B-%' / 'CHART-REG-%'). Batch focus
và 12 học viên trong file 03/04 không bị xoá.

## Cách chạy SQL

### Cách 1 — `sqlcmd` trên container DB của docker-compose

`docker-compose.yml` map MSSQL ra cổng `1434` host:

```bash
# từ thư mục dự án (chỗ có docker-compose.yml)
docker compose up -d mssql
docker compose exec -T mssql /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -d ZealEducationDb -i /dev/stdin < backend/scripts/seed/demo/01-courses.sql
```

Hoặc copy cả thư mục seed vào container rồi loop:

```bash
docker compose cp backend/scripts/seed mssql:/tmp/seed
docker compose exec mssql bash -lc '
  for f in /tmp/seed/demo/*.sql; do
    echo "==> $f";
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
        -C -d ZealEducationDb -b -i "$f" || exit 1;
  done
  for f in /tmp/seed/chart/*.sql; do
    echo "==> $f";
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
        -C -d ZealEducationDb -b -i "$f" || exit 1;
  done
'
```

Cờ `-b` để sqlcmd thoát non-zero ngay khi gặp lỗi (giúp loop dừng đúng lúc).

> **Lưu ý SET options.** SQL Server bắt buộc `QUOTED_IDENTIFIER ON` và
> `ANSI_NULLS ON` khi INSERT vào bảng có filtered index (ví dụ:
> `enrollment.FeeId`, `course_enquiry.Phone`, `certificate_application.*`).
> Mặc định của `sqlcmd` là OFF nên sẽ báo lỗi `Msg 1934`. Tất cả file seed đã
> tự `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` ở đầu file. Nếu chạy bằng
> tool khác mà bỏ qua các SET này, hãy thêm cờ `-I` cho `sqlcmd`
> (`-I` = "enable QUOTED_IDENTIFIER for the connection") hoặc bật tương đương
> trong tool đang dùng.

### Cách 2 — `sqlcmd` từ máy host (đã cài mssql-tools)

```bash
SA_PASSWORD='Zeal_StrongPass!2026'   # đổi cho khớp .env

for f in backend/scripts/seed/demo/*.sql; do
  echo "==> $f"
  sqlcmd -S localhost,1434 -U sa -P "$SA_PASSWORD" -C \
         -d ZealEducationDb -b -i "$f" || break
done

# Optional: thêm dữ liệu chart
for f in backend/scripts/seed/chart/*.sql; do
  echo "==> $f"
  sqlcmd -S localhost,1434 -U sa -P "$SA_PASSWORD" -C \
         -d ZealEducationDb -b -i "$f" || break
done
```

### Cách 3 — PowerShell (Windows / cross-platform)

```powershell
$SAPassword = 'Zeal_StrongPass!2026'

Get-ChildItem backend/scripts/seed/demo/*.sql | Sort-Object Name | ForEach-Object {
    Write-Host "==> $($_.Name)"
    sqlcmd -S "localhost,1434" -U sa -P $SAPassword -C `
           -d ZealEducationDb -b -i $_.FullName
    if ($LASTEXITCODE -ne 0) { throw "Failed: $($_.Name)" }
}

Get-ChildItem backend/scripts/seed/chart/*.sql | Sort-Object Name | ForEach-Object {
    Write-Host "==> $($_.Name)"
    sqlcmd -S "localhost,1434" -U sa -P $SAPassword -C `
           -d ZealEducationDb -b -i $_.FullName
    if ($LASTEXITCODE -ne 0) { throw "Failed: $($_.Name)" }
}
```

### Cách 4 — Azure Data Studio / SSMS / DataGrip

Mở từng file `.sql` rồi chạy lần lượt theo số thứ tự. Tất cả file đều dùng
`BEGIN TRAN` / `COMMIT TRAN` nên có thể rollback dễ dàng nếu cần.

## Kiểm tra dữ liệu sau khi seed

```sql
-- Check daily distribution của batch creation trend
SELECT CAST(CreatedAt AS DATE) AS Day,
       SUM(CASE WHEN Status='Active'    THEN 1 ELSE 0 END) AS Active,
       SUM(CASE WHEN Status='Completed' THEN 1 ELSE 0 END) AS Completed,
       SUM(CASE WHEN Status='Cancelled' THEN 1 ELSE 0 END) AS Cancelled,
       COUNT(*) AS Total
FROM batch
WHERE CreatedBy = 'seed-chart-data'
GROUP BY CAST(CreatedAt AS DATE)
ORDER BY Day;

-- Check daily distribution của candidate registration trend
SELECT CAST(RegisteredAt AS DATE) AS Day,
       SUM(CASE WHEN Status='Active'    THEN 1 ELSE 0 END) AS Active,
       SUM(CASE WHEN Status='Graduated' THEN 1 ELSE 0 END) AS Graduated,
       SUM(CASE WHEN Status='Dropped'   THEN 1 ELSE 0 END) AS Dropped,
       COUNT(*) AS Total
FROM candidate
WHERE CreatedBy = 'seed-chart-data'
GROUP BY CAST(RegisteredAt AS DATE)
ORDER BY Day;

-- Check exam scores trend của batch CHART-FOCUS-2026
SELECT e.ExamDate,
       AVG(r.Score) AS AvgScore,
       MIN(r.Score) AS MinScore,
       MAX(r.Score) AS MaxScore,
       COUNT(*)     AS NumResults
FROM examination e
INNER JOIN exam_result r ON r.ExamId = e.Id
WHERE e.BatchId = 'e1c1e1c1-9999-0000-0000-000000000001'
GROUP BY e.ExamDate
ORDER BY e.ExamDate;
```

## Xoá toàn bộ seed (nếu cần dựng lại sạch)

Cả hai bộ seed đều stamp `CreatedBy` rõ ràng:

- `'seed-demo-data'` — bộ `demo/`
- `'seed-chart-data'` — bộ `chart/`

```sql
-- ⚠️ Xoá theo đúng thứ tự FK ngược (con -> cha)
DELETE FROM exam_result            WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM examination            WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM attendance_record      WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM class_session          WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM payment_transaction    WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM installment_plan       WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM fine                   WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM certificate_application WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM feedback               WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
UPDATE enrollment SET FeeId = NULL WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM fee_structure          WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM enrollment             WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM batch                  WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM enquiry_note           WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM course_enquiry         WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM system_asset           WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM candidate              WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM faculty                WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM staff                  WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM user_account           WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
DELETE FROM course                 WHERE CreatedBy IN ('seed-demo-data','seed-chart-data');
```

Sau khi xoá xong có thể chạy lại từ `demo/01-courses.sql`.
