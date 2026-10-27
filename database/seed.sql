-- Repo Vault demo seed data for UCC BSCS 2A (MariaDB 10.4, XAMPP).
-- Database: repo_vault_db. Import with: /opt/lampp/bin/mysql -u root repo_vault_db < database/seed.sql
-- Idempotent: safe to run multiple times (INSERT IGNORE, scoped DELETE+INSERT, ON DUPLICATE KEY UPDATE).
-- All accounts log in with password: Password123! (PBKDF2-SHA256, 100000 iterations).
-- BareRepoDiskPath values are placeholders; DemoWorkspaceSeeder realigns them
-- to absolute paths under the chosen seed root on first run.

USE `repo_vault_db`;

-- Faculty and students. Hashes below verify against Password123!.
INSERT IGNORE INTO `Users` (IdNumber, Username, PasswordHash, PasswordSalt, Iterations, FullName, Email, Role) VALUES
('FAC-2020-001', 'prof_santos', 'mnaaqNcCPFbXuB/aecPzx3u5LD+bJEFiGiwYXqxjzMY=', 'RphGKenRS7/3AJ991rnsYA==', 100000, 'Prof. Crisanto Santos', 'csantos@ucc.edu.ph', 'Faculty'),
('2023-00142', 'frtzhahn', 'hqoMkaYSQ1Vvt5mF/SCN3yl0WupiP9RMI0hE+ixuRsQ=', 'dPwY0vc7pL7V2xxmDAdSYg==', 100000, 'Aldrin James Alciso', 'frtzhahn@ucc.edu.ph', 'Student'),
('2023-00117', 'sduites', 'p5UGoniWHdnDDj8Z6uIg5lCnGA46AvRNJFjEyFPv4Hs=', 'wxwjk9cl+qc7lPepUDgP8g==', 100000, 'Stephen Duites', 'sduites@ucc.edu.ph', 'Student'),
('2023-00098', 'epapina', 'UbXUvi5RZgdC1P78xe28xOShX57xGXowELL3oh5STlI=', '6vDIFEVJG6EsAKOqZbpuVw==', 100000, 'Exequiel Papina', 'epapina@ucc.edu.ph', 'Student'),
('2023-00205', 'aobrence', 'Xs0Z+yFV2x+kVyE5HBwWgzXwoxmI6fu8J6t6s2UbdCs=', 'dZnLZFy8r8MNSw1qJtbxPQ==', 100000, 'Aegelyn Obrence', 'aobrence@ucc.edu.ph', 'Student'),
('2024-00031', 'jdelacruz', 'g3m+4sSp7mgalHD8UJZG2Oi7bph4HX7lyPYkxDK0a4A=', '32r+gE7goz/dBqVyti+NWw==', 100000, 'Juan Dela Cruz', 'jdelacruz@ucc.edu.ph', 'Student');

-- Courses owned by prof_santos.
INSERT IGNORE INTO `Courses` (CourseCode, CourseTitle, Department, Semester, CreatedByFacultyId) VALUES
('CS201', 'Data Structures and Algorithms', 'CSD', '1st Sem 2025-2026', (SELECT UserId FROM Users WHERE Username = 'prof_santos')),
('CS204', 'Object-Oriented Programming', 'CSD', '2nd Sem 2025-2026', (SELECT UserId FROM Users WHERE Username = 'prof_santos')),
('CS206', 'Applications Development and Emerging Tech', 'CSD', '2nd Sem 2025-2026', (SELECT UserId FROM Users WHERE Username = 'prof_santos'));

-- Coding tasks posted under courses by prof_santos.
INSERT IGNORE INTO `CodingTasks` (CourseId, Title, Instructions, StarterTemplate, DeadlineUtc, AllowGroupWork, MaxGroupSize, CreatedByFacultyId) VALUES
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS201' AND Semester = '1st Sem 2025-2026'), 'Midterm Lab: Linked Lists', 'Implement generic linked list data structures with insert, delete, and reverse operations.', 'csharp', '2025-10-01 16:00:00', 0, 1, (SELECT UserId FROM Users WHERE Username = 'prof_santos')),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS204' AND Semester = '2nd Sem 2025-2026'), 'Lab 3: JavaFX Fact Generator', 'Build an OOP GUI application that loads random trivia from a local database.', 'java', '2026-03-20 16:00:00', 0, 1, (SELECT UserId FROM Users WHERE Username = 'prof_santos')),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS206' AND Semester = '2nd Sem 2025-2026'), 'Term Project: 8-Bit Architecture Emulator', 'Develop an interactive microarchitecture emulator with instruction decoding and registers.', 'csharp', '2026-03-15 16:00:00', 1, 4, (SELECT UserId FROM Users WHERE Username = 'prof_santos')),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS206' AND Semester = '2nd Sem 2025-2026'), 'Final Project: Habit Accountability Tracker', 'Full stack productivity and accountability tracker with LAN synchronization.', 'csharp', '2026-04-10 16:00:00', 1, 3, (SELECT UserId FROM Users WHERE Username = 'prof_santos'));

-- Project repositories. Disk paths are seeder placeholders, see header note.
INSERT IGNORE INTO `Repositories` (CourseId, OwnerStudentId, TaskId, RepoSlug, Title, Description, BareRepoDiskPath, DeadlineUtc, IsLocked, IsSubmitted, SubmittedAt, LifecycleStatus, TrashedAt) VALUES
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS206' AND Semester = '2nd Sem 2025-2026'),
 (SELECT UserId FROM Users WHERE Username = 'frtzhahn'),
 (SELECT TaskId FROM CodingTasks WHERE Title LIKE 'Term Project%' LIMIT 1),
 'micro-8', 'micro-8', '8-bit microarchitecture emulator and debugger', 'seed-repos/micro-8.git', '2026-03-15 16:00:00', 0, 0, NULL, 'Active', NULL),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS204' AND Semester = '2nd Sem 2025-2026'),
 (SELECT UserId FROM Users WHERE Username = 'sduites'),
 (SELECT TaskId FROM CodingTasks WHERE Title LIKE 'Lab 3%' LIMIT 1),
 'randfacts', 'randfacts', 'JavaFX and SQLite random facts generator', 'seed-repos/randfacts.git', '2026-03-20 16:00:00', 0, 0, NULL, 'Active', NULL),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS206' AND Semester = '2nd Sem 2025-2026'),
 (SELECT UserId FROM Users WHERE Username = 'epapina'),
 (SELECT TaskId FROM CodingTasks WHERE Title LIKE 'Final Project%' LIMIT 1),
 'lockedin', 'lockedin', 'Habit tracking and accountability platform', 'seed-repos/lockedin.git', '2026-04-10 16:00:00', 0, 0, NULL, 'Active', NULL),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS201' AND Semester = '1st Sem 2025-2026'),
 (SELECT UserId FROM Users WHERE Username = 'aobrence'),
 NULL,
 'web-archive', 'web-archive', 'Digital archive project in Node.js and SQLite', 'seed-repos/web-archive.git', '2025-12-05 16:00:00', 0, 0, NULL, 'Active', NULL),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS204' AND Semester = '2nd Sem 2025-2026'),
 (SELECT UserId FROM Users WHERE Username = 'frtzhahn'),
 NULL,
 'trashed-calc', 'trashed-calc', 'Old calculator project for Repo Bin restore demo', 'seed-repos/trashed-calc.git', NULL, 0, 0, NULL, 'Trashed', UTC_TIMESTAMP(6)),
((SELECT CourseId FROM Courses WHERE CourseCode = 'CS201' AND Semester = '1st Sem 2025-2026'),
 (SELECT UserId FROM Users WHERE Username = 'frtzhahn'),
 (SELECT TaskId FROM CodingTasks WHERE Title LIKE 'Midterm Lab%' LIMIT 1),
 'archived-exam', 'archived-exam', 'Locked midterm lab submission', 'seed-repos/archived-exam.git', '2025-10-01 16:00:00', 1, 1, '2025-09-30 15:30:00', 'Active', NULL);

-- Student teams and collaborators.
INSERT IGNORE INTO `RepositoryContributors` (RepoId, StudentUserId, TeamRole, AccessLevel, ApprovalStatus, ApprovedByFacultyId, ApprovedAt) VALUES
-- micro-8: lead owner, approved groupmates, pending ticket for aobrence, peer viewer for jdelacruz
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'),
 (SELECT UserId FROM Users WHERE Username = 'frtzhahn'), 'Lead', 'Owner', 'Approved', NULL, NULL),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'),
 (SELECT UserId FROM Users WHERE Username = 'sduites'), 'Developer', 'GroupCollaborator', 'Approved', (SELECT UserId FROM Users WHERE Username = 'prof_santos'), '2026-01-20 10:00:00'),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'),
 (SELECT UserId FROM Users WHERE Username = 'epapina'), 'Documenter', 'GroupCollaborator', 'Approved', (SELECT UserId FROM Users WHERE Username = 'prof_santos'), '2026-01-20 10:00:00'),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'),
 (SELECT UserId FROM Users WHERE Username = 'aobrence'), 'Tester', 'GroupCollaborator', 'Pending', NULL, NULL),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'),
 (SELECT UserId FROM Users WHERE Username = 'jdelacruz'), 'Peer Reviewer', 'PeerViewer', 'Approved', NULL, NULL),

-- randfacts
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'randfacts' AND c.CourseCode = 'CS204'),
 (SELECT UserId FROM Users WHERE Username = 'sduites'), 'Lead', 'Owner', 'Approved', NULL, NULL),

-- lockedin
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'lockedin' AND c.CourseCode = 'CS206'),
 (SELECT UserId FROM Users WHERE Username = 'epapina'), 'Lead', 'Owner', 'Approved', NULL, NULL),

-- web-archive
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'web-archive' AND c.CourseCode = 'CS201'),
 (SELECT UserId FROM Users WHERE Username = 'aobrence'), 'Lead', 'Owner', 'Approved', NULL, NULL),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'web-archive' AND c.CourseCode = 'CS201'),
 (SELECT UserId FROM Users WHERE Username = 'jdelacruz'), 'Developer', 'GroupCollaborator', 'Approved', (SELECT UserId FROM Users WHERE Username = 'prof_santos'), '2025-11-01 10:00:00'),

-- trashed-calc
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'trashed-calc' AND c.CourseCode = 'CS204'),
 (SELECT UserId FROM Users WHERE Username = 'frtzhahn'), 'Lead', 'Owner', 'Approved', NULL, NULL),

-- archived-exam
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'archived-exam' AND c.CourseCode = 'CS201'),
 (SELECT UserId FROM Users WHERE Username = 'frtzhahn'), 'Lead', 'Owner', 'Approved', NULL, NULL);

-- Peer review thread on micro-8. Scoped delete keeps re-runs idempotent.
DELETE FROM `DiscussionComments` WHERE RepoId IN (
  SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId)
  WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206');

INSERT INTO `DiscussionComments` (RepoId, AuthorUserId, CommentBody) VALUES
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'), (SELECT UserId FROM Users WHERE Username = 'prof_santos'), 'nice work on the fetch-decode loop. add bounds checks on memory writes before the demo.'),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'), (SELECT UserId FROM Users WHERE Username = 'frtzhahn'), 'thanks prof, added bounds checks and a test rom in the latest push.'),
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'), (SELECT UserId FROM Users WHERE Username = 'prof_santos'), 'debugger breakpoint list looks good. document the cli flags in the readme.');

-- Faculty evaluation on micro-8. TotalScore is generated (23+24+22+25=94).
INSERT INTO `Evaluations` (RepoId, FacultyUserId, ArchitectureScore, FunctionalityScore, CodeQualityScore, DocumentationScore, CritiqueNotes) VALUES
((SELECT r.RepoId FROM Repositories r JOIN Courses c USING (CourseId) WHERE r.RepoSlug = 'micro-8' AND c.CourseCode = 'CS206'), (SELECT UserId FROM Users WHERE Username = 'prof_santos'), 23, 24, 22, 25, 'solid emulator core and clean commit history. polish the readme with usage examples and keep method sizes small.')
ON DUPLICATE KEY UPDATE ArchitectureScore = VALUES(ArchitectureScore), FunctionalityScore = VALUES(FunctionalityScore), CodeQualityScore = VALUES(CodeQualityScore), DocumentationScore = VALUES(DocumentationScore), CritiqueNotes = VALUES(CritiqueNotes);
