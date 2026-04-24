/*
 * Xoá toàn bộ dữ liệu course_enquiry (và enquiry_note liên quan) để chuẩn bị
 * cho migration AddCourseAndLinkEnquiry — migration này drop cột cũ
 * CourseInterested (NVARCHAR) và thêm CourseInterestedId (FK -> course.Id).
 *
 * Chạy script NÀY TRƯỚC khi thực thi migration trên DB đã có dữ liệu enquiry.
 *
 * Target DB: SQL Server.
 * Idempotent: DELETE sẽ chạy OK kể cả khi bảng rỗng.
 */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

-- Xoá notes trước (enquiry_note.EnquiryId FK -> course_enquiry.Id, Cascade nhưng
-- ta xoá rõ ràng để log số lượng và tránh phụ thuộc vào cascade).
DELETE FROM enquiry_note;
DECLARE @NoteCount INT = @@ROWCOUNT;

DELETE FROM course_enquiry;
DECLARE @EnquiryCount INT = @@ROWCOUNT;

COMMIT TRAN;

PRINT CONCAT('Deleted ', @NoteCount,    ' row(s) from enquiry_note.');
PRINT CONCAT('Deleted ', @EnquiryCount, ' row(s) from course_enquiry.');
