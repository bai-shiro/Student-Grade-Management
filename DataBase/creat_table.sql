-- dotnet add package MySql.Data

CREATE DATABASE IF NOT EXISTS school_db;
    DEFAULT CHARACTER SET = 'utf8mb4';

USE school_db;

-- 创建院系表
CREATE TABLE departments (
    department_id VARCHAR(2) PRIMARY KEY,
    department_name VARCHAR(50) NOT NULL
);

-- 创建班级表
CREATE TABLE classes (
    class_id VARCHAR(4) PRIMARY KEY,
    class_name VARCHAR(50) NOT NULL,
    department_id VARCHAR(2) NOT NULL,
    FOREIGN KEY (department_id) REFERENCES departments(department_id)
);

-- 创建学生表
CREATE TABLE students (
    student_id VARCHAR(10) PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    class_id VARCHAR(4) NOT NULL,
    department_id VARCHAR(2) NOT NULL,
    enrollment_year INT NOT NULL,
    FOREIGN KEY (class_id) REFERENCES classes(class_id),
    FOREIGN KEY (department_id) REFERENCES departments(department_id)
);

-- 创建课程表
CREATE TABLE courses (
    course_id VARCHAR(10) PRIMARY KEY,
    course_name VARCHAR(50) NOT NULL,
    credit DECIMAL(3, 1) NOT NULL
);

-- 创建成绩表
CREATE TABLE scores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    student_id VARCHAR(10) NOT NULL,
    course_id VARCHAR(10) NOT NULL,
    score DECIMAL(5, 2) NOT NULL,
    FOREIGN KEY (student_id) REFERENCES students(student_id),
    FOREIGN KEY (course_id) REFERENCES courses(course_id)
);

-- 创建用户表
CREATE TABLE users (
    user_id VARCHAR(10) PRIMARY KEY,
    password VARCHAR(255) NOT NULL,
    role ENUM('admin','student') NOT NULL
);

-- 插入测试管理员用户（必须有至少一位）
INSERT INTO users (user_id, password, role) VALUES 
('admin', '123456', 'admin');

-- 1. 插入院系数据（院系编号06）
INSERT INTO departments (department_id, department_name) 
VALUES ('06', '计算机学院'); -- 可自定义院系名称

-- 2. 插入班级数据（班级编号57，关联院系06）
INSERT INTO classes (class_id, class_name, department_id) 
VALUES ('57', '计科2301班', '06'); -- 可自定义班级名称

USE school_db;

-- 插入测试院系数据
INSERT INTO departments (department_id, department_name) VALUES 
('06', '计算机学院'),
('07', '电子工程学院');

-- 插入测试班级数据（关联院系）
INSERT INTO classes (class_id, class_name, department_id) VALUES 
('57', '计科2301班', '06'),
('58', '计科2302班', '06'),
('60', '电子2301班', '07');

-- ALTER TABLE users 
-- ADD CONSTRAINT fk_users_student_id 
-- FOREIGN KEY (user_id) REFERENCES students(student_id) 
-- ON DELETE CASCADE;

-- 插入测试课程数据
INSERT INTO courses (course_id, course_name, credit) VALUES 
('C001', '高等数学', 4.0),
('C002', '程序设计基础', 3.5),
('C003', '数据结构', 4.0),
('C004', '操作系统', 3.0);

INSERT INTO students (student_id, name, class_id, department_id, enrollment_year) VALUES 
('2023065732', '张三', '57', '06', 2023);

INSERT INTO users (user_id, password, role) VALUES 
('2023065732', '123456', 'student');

-- 插入测试成绩数据（关联已添加的学生2023065732）
INSERT INTO scores (student_id, course_id, score) VALUES 
('2023065732', 'C001', 85.5),
('2023065732', 'C002', 92.0),
('2023065732', 'C003', 78.0);