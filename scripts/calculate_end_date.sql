
CREATE OR REPLACE FUNCTION calculate_end_date(
    start_date DATE,
    schedule_type VARCHAR
) RETURNS DATE AS $$
BEGIN
    RETURN CASE 
        WHEN schedule_type = 'Monthly' THEN start_date + INTERVAL '1 month'
        WHEN schedule_type = 'Semi-Monthly' THEN start_date + INTERVAL '15 days'
        WHEN schedule_type = 'Biweekly' THEN start_date + INTERVAL '2 weeks'
        WHEN schedule_type = 'Weekly' THEN start_date + INTERVAL '1 week'
        ELSE start_date -- Default to no interval if schedule type is unknown
    END;
END;
$$ LANGUAGE plpgsql;

