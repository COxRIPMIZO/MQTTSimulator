CREATE PROCEDURE SP_InsertDataIntoRegisterValue
    @MqttDataType MqttDataType READONLY
AS
BEGIN
    INSERT INTO RegisterValue (Name, Value,TimeStamp)
    SELECT Name, Value, TimeStamp 
    FROM @MqttDataType;
END;
