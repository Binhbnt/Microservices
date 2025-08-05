-- File: init.sql
-- Kịch bản này sẽ tự động chạy khi container PostgreSQL khởi động lần đầu tiên.

-- Tạo các database và chỉ định chủ sở hữu là user 'bnt'
CREATE DATABASE user_db OWNER bnt;
CREATE DATABASE leaverequest_db OWNER bnt;
CREATE DATABASE auditlog_db OWNER bnt;
CREATE DATABASE appnotification_db OWNER bnt;
