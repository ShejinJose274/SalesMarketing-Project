-- Adds the database safeguard for H8: a salesman can belong to only one active team.
-- Run the duplicate check first. Resolve any returned rows before creating the index.

SELECT salesmanid, COUNT(*) AS active_assignment_count
FROM marketingteamdetail
WHERE status = 1
GROUP BY salesmanid
HAVING COUNT(*) > 1;

CREATE UNIQUE INDEX IF NOT EXISTS uq_mtdetail_active_salesman
	ON marketingteamdetail (salesmanid)
	WHERE status = 1;
