CREATE OR REPLACE PROCEDURE populate_timecard_for_payroll (
    IN company_id INT
)
LANGUAGE plpgsql
AS $$
DECLARE 
    start_date DATE;
    end_date DATE;
    payroll_schedule_id INT;
    previous_payroll_schedule_id INT;
    prev_start_date DATE;
    prev_end_date DATE;
    prev_schedule_type VARCHAR;
BEGIN
    -- Get the previous payroll schedule for the company
    SELECT 
        ps.payrollscheduleid,
        ps.startdate,
        pst.value
    INTO 
        previous_payroll_schedule_id, 
        prev_start_date, 
        prev_schedule_type
    FROM 
        payroll ps
    JOIN 
        payrollscheduletype pst 
    ON 
        ps.payrollscheduleid = pst.id
    WHERE 
        ps.companyid = company_id
    ORDER BY 
        ps.startdate DESC
    OFFSET 1 ROW FETCH NEXT 1 ROW ONLY;

    -- Calculate the previous end date of the Schedule based on Schedule Type to get the time card period
    prev_end_date := calculate_end_date(prev_start_date, prev_schedule_type);

    -- Get the current payroll schedule and the Start and End date of the schedule to get the time card period
    SELECT 
        
        ps.startdate, -- Get the StartDate from PayrollSchedule
        calculate_end_date(ps.startdate, pst.value) AS end_date,
        ps.payrollscheduletypeid
    INTO 
        start_date, 
        end_date, 
        payroll_schedule_id
    FROM 
        payrollschedule ps
    JOIN 
        payrollscheduletype pst 
    ON 
        ps.payrollscheduletypeid = pst.id
    WHERE 
        ps.companyid = company_id
        AND (ps.enddate IS NULL OR ps.enddate > CURRENT_DATE)
    ORDER BY 
        ps.startdate DESC;

    -- Check if there is no current payroll, and create one if necessary
    IF NOT EXISTS (
        SELECT 1
        FROM payroll
        WHERE companyid = company_id
          AND startdate <= CURRENT_DATE
          AND truerundate IS NULL
    )
    THEN
        INSERT INTO payroll (companyid, truerundate, scheduledrundate, createdby,payrollscheduleid, startdate)
        VALUES (company_id, NULL, end_date,'System', payroll_schedule_id, start_date);
    END IF;

    -- Populate the time card for each employee of the company for the current payroll period
    CALL populate_timecardusa(company_id, start_date, end_date);

    -- Populate the time card for each employee of the company for the previous payroll period
    CALL populate_timecardusa(company_id, prev_start_date, prev_end_date);
END;
$$;
