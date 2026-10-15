-- Repo Vault database schema for MariaDB 10.4 (XAMPP).
-- Database: repo_vault_db, charset utf8mb4, engine InnoDB for all tables.
-- All timestamps are stored in UTC as DATETIME(6).
-- Import with: /opt/lampp/bin/mysql -u root < database/schema.sql

CREATE DATABASE IF NOT EXISTS `repo_vault_db`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `repo_vault_db`;

-- Users: student and faculty accounts. Passwords use PBKDF2 (hash + salt + iterations).
CREATE TABLE IF NOT EXISTS `Users` (
  `UserId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `IdNumber` VARCHAR(32) NOT NULL,
  `Username` VARCHAR(50) NOT NULL,
  `PasswordHash` VARCHAR(255) NOT NULL,
  `PasswordSalt` VARCHAR(64) NOT NULL,
  `Iterations` INT UNSIGNED NOT NULL DEFAULT 100000,
  `FullName` VARCHAR(100) NOT NULL,
  `Email` VARCHAR(255) NOT NULL,
  `Role` ENUM('Student','Faculty') NOT NULL DEFAULT 'Student',
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `uq_users_idnumber` (`IdNumber`),
  UNIQUE KEY `uq_users_username` (`Username`),
  UNIQUE KEY `uq_users_email` (`Email`),
  KEY `idx_users_role` (`Role`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Courses: catalog entries owned by a faculty member. Semester is a plain column.
CREATE TABLE IF NOT EXISTS `Courses` (
  `CourseId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `CourseCode` VARCHAR(20) NOT NULL,
  `CourseTitle` VARCHAR(255) NOT NULL,
  `Department` VARCHAR(100) NOT NULL,
  `Semester` VARCHAR(32) NOT NULL,
  `CreatedByFacultyId` INT UNSIGNED NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`CourseId`),
  UNIQUE KEY `uq_courses_code_semester` (`CourseCode`, `Semester`),
  KEY `idx_courses_faculty` (`CreatedByFacultyId`),
  CONSTRAINT `fk_courses_faculty`
    FOREIGN KEY (`CreatedByFacultyId`) REFERENCES `Users` (`UserId`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Repositories: project archives linked to a course, with cached code metrics.
CREATE TABLE IF NOT EXISTS `Repositories` (
  `RepoId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `CourseId` INT UNSIGNED NOT NULL,
  `RepoSlug` VARCHAR(100) NOT NULL,
  `Title` VARCHAR(255) NOT NULL,
  `Description` TEXT NULL,
  `BareRepoDiskPath` VARCHAR(500) NOT NULL,
  `DeadlineUtc` DATETIME(6) NULL,
  `IsLocked` TINYINT(1) NOT NULL DEFAULT 0,
  `LifecycleStatus` ENUM('Active','Trashed') NOT NULL DEFAULT 'Active',
  `TrashedAt` DATETIME(6) NULL,
  `TotalFiles` INT UNSIGNED NOT NULL DEFAULT 0,
  `SourceLines` INT UNSIGNED NOT NULL DEFAULT 0,
  `CommentLines` INT UNSIGNED NOT NULL DEFAULT 0,
  `BlankLines` INT UNSIGNED NOT NULL DEFAULT 0,
  `PrimaryLanguage` VARCHAR(50) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`RepoId`),
  UNIQUE KEY `uq_repos_course_slug` (`CourseId`, `RepoSlug`),
  UNIQUE KEY `uq_repos_diskpath` (`BareRepoDiskPath`),
  KEY `idx_repos_course` (`CourseId`),
  KEY `idx_repos_status` (`LifecycleStatus`),
  CONSTRAINT `fk_repos_course`
    FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- RepositoryContributors: student team roster per repository.
CREATE TABLE IF NOT EXISTS `RepositoryContributors` (
  `RepoId` INT UNSIGNED NOT NULL,
  `StudentUserId` INT UNSIGNED NOT NULL,
  `TeamRole` VARCHAR(50) NOT NULL DEFAULT 'Member',
  `AssignedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`RepoId`, `StudentUserId`),
  KEY `idx_contrib_student` (`StudentUserId`),
  CONSTRAINT `fk_contrib_repo`
    FOREIGN KEY (`RepoId`) REFERENCES `Repositories` (`RepoId`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_contrib_student`
    FOREIGN KEY (`StudentUserId`) REFERENCES `Users` (`UserId`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- RepositoryLanguages: per-language breakdown cached from the metrics scanner.
CREATE TABLE IF NOT EXISTS `RepositoryLanguages` (
  `RepoId` INT UNSIGNED NOT NULL,
  `LanguageName` VARCHAR(50) NOT NULL,
  `FileCount` INT UNSIGNED NOT NULL DEFAULT 0,
  `SourceLines` INT UNSIGNED NOT NULL DEFAULT 0,
  `CommentLines` INT UNSIGNED NOT NULL DEFAULT 0,
  `BlankLines` INT UNSIGNED NOT NULL DEFAULT 0,
  `PercentageShare` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  PRIMARY KEY (`RepoId`, `LanguageName`),
  CONSTRAINT `fk_repolang_repo`
    FOREIGN KEY (`RepoId`) REFERENCES `Repositories` (`RepoId`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- PushLedger: server-side audit log of every accepted push.
CREATE TABLE IF NOT EXISTS `PushLedger` (
  `PushId` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `RepoId` INT UNSIGNED NOT NULL,
  `PusherUserId` INT UNSIGNED NOT NULL,
  `BranchRef` VARCHAR(255) NOT NULL,
  `OldCommitHash` VARCHAR(64) NOT NULL,
  `NewCommitHash` VARCHAR(64) NOT NULL,
  `ClientIpAddress` VARCHAR(45) NULL,
  `PushedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`PushId`),
  KEY `idx_push_repo` (`RepoId`),
  KEY `idx_push_pusher` (`PusherUserId`),
  KEY `idx_push_pushedat` (`PushedAt`),
  CONSTRAINT `fk_push_repo`
    FOREIGN KEY (`RepoId`) REFERENCES `Repositories` (`RepoId`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_push_pusher`
    FOREIGN KEY (`PusherUserId`) REFERENCES `Users` (`UserId`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- CommitLogs: commit metadata scraped from the bare repo after each push.
CREATE TABLE IF NOT EXISTS `CommitLogs` (
  `CommitId` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `RepoId` INT UNSIGNED NOT NULL,
  `CommitHash` VARCHAR(64) NOT NULL,
  `AuthorName` VARCHAR(255) NOT NULL,
  `AuthorEmail` VARCHAR(255) NOT NULL,
  `CommittedAt` DATETIME(6) NOT NULL,
  `CommitMessage` TEXT NOT NULL,
  PRIMARY KEY (`CommitId`),
  UNIQUE KEY `uq_commitlogs_repo_hash` (`RepoId`, `CommitHash`),
  KEY `idx_commitlogs_repo_date` (`RepoId`, `CommittedAt`),
  CONSTRAINT `fk_commitlogs_repo`
    FOREIGN KEY (`RepoId`) REFERENCES `Repositories` (`RepoId`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- DiscussionComments: peer review and faculty feedback threads per repository.
CREATE TABLE IF NOT EXISTS `DiscussionComments` (
  `CommentId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `RepoId` INT UNSIGNED NOT NULL,
  `AuthorUserId` INT UNSIGNED NOT NULL,
  `CommentBody` TEXT NOT NULL,
  `IsModeratedHidden` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`CommentId`),
  KEY `idx_comments_repo` (`RepoId`),
  KEY `idx_comments_author` (`AuthorUserId`),
  CONSTRAINT `fk_comments_repo`
    FOREIGN KEY (`RepoId`) REFERENCES `Repositories` (`RepoId`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_comments_author`
    FOREIGN KEY (`AuthorUserId`) REFERENCES `Users` (`UserId`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Evaluations: one faculty rubric grade per repository, TotalScore is computed.
CREATE TABLE IF NOT EXISTS `Evaluations` (
  `EvaluationId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `RepoId` INT UNSIGNED NOT NULL,
  `FacultyUserId` INT UNSIGNED NOT NULL,
  `ArchitectureScore` TINYINT UNSIGNED NOT NULL,
  `FunctionalityScore` TINYINT UNSIGNED NOT NULL,
  `CodeQualityScore` TINYINT UNSIGNED NOT NULL,
  `DocumentationScore` TINYINT UNSIGNED NOT NULL,
  `TotalScore` INT GENERATED ALWAYS AS
    (`ArchitectureScore` + `FunctionalityScore` + `CodeQualityScore` + `DocumentationScore`) STORED,
  `CritiqueNotes` TEXT NULL,
  `EvaluatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`EvaluationId`),
  UNIQUE KEY `uq_evaluations_repo` (`RepoId`),
  KEY `idx_evaluations_faculty` (`FacultyUserId`),
  CONSTRAINT `chk_evaluations_range`
    CHECK (`ArchitectureScore` BETWEEN 0 AND 25
      AND `FunctionalityScore` BETWEEN 0 AND 25
      AND `CodeQualityScore` BETWEEN 0 AND 25
      AND `DocumentationScore` BETWEEN 0 AND 25),
  CONSTRAINT `fk_evaluations_repo`
    FOREIGN KEY (`RepoId`) REFERENCES `Repositories` (`RepoId`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_evaluations_faculty`
    FOREIGN KEY (`FacultyUserId`) REFERENCES `Users` (`UserId`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- App user for local XAMPP development (passwordless, localhost only).
-- The default connection string uses root with no password, this is an optional alias.
CREATE USER IF NOT EXISTS 'repovault'@'127.0.0.1' IDENTIFIED BY '';
CREATE USER IF NOT EXISTS 'repovault'@'localhost' IDENTIFIED BY '';
GRANT ALL PRIVILEGES ON `repo_vault_db`.* TO 'repovault'@'127.0.0.1';
GRANT ALL PRIVILEGES ON `repo_vault_db`.* TO 'repovault'@'localhost';
FLUSH PRIVILEGES;
