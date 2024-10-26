CREATE OR REPLACE PROCEDURE populate_timecardusa(
    company_id INT,
    start_date DATE,
    end_date DATE
)
LANGUAGE plpgsql
AS $$
DECLARE
    job_location RECORD;
    employment RECORD;
    day DATE;
BEGIN
   
    FOR job_location IN
        SELECT id
        FROM job
        WHERE companyid = company_id
    LOOP
        
        FOR employment IN
            SELECT personid, jobid
            FROM employment
            WHERE jobid = job_location.id
        LOOP
            
            FOR day IN
                SELECT generate_series(
                    start_date, 
                    end_date,   
                    INTERVAL '1 day'
                )::DATE
            LOOP
                IF EXTRACT(DOW FROM day) = 3 THEN -- Wednesday
                    -- Populate timecardusa table for Wednesday with two shifts
                    INSERT INTO timecardusa (personid, createdby,jobid, clockin, clockout)
                    VALUES
                    (employment.personid,'System', employment.jobid, (day + TIME '09:00:00')::TIMESTAMP WITHOUT TIME ZONE, (day + TIME '13:00:00')::TIMESTAMP WITHOUT TIME ZONE),
                    (employment.personid,'System', employment.jobid, (day + TIME '14:00:00')::TIMESTAMP WITHOUT TIME ZONE, (day + TIME '18:00:00')::TIMESTAMP WITHOUT TIME ZONE);
                ELSIF EXTRACT(DOW FROM day) BETWEEN 1 AND 5 THEN -- Monday to Friday
                    -- Populate timecardusa table for other weekdays with one shift
                    INSERT INTO timecardusa (personid,createdby, jobid, clockin, clockout)
                    VALUES
                    (employment.personid,'System', employment.jobid, (day + TIME '09:00:00')::TIMESTAMP WITHOUT TIME ZONE, (day + TIME '17:00:00')::TIMESTAMP WITHOUT TIME ZONE);
                END IF;
            END LOOP;
        END LOOP;
    END LOOP;
END $$;
