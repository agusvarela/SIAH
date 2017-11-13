DELETE FROM SIAHConnection.dbo.InsumoOcasa
BULK INSERT InsumoOcasa
    FROM 'C:\Tesis\SIAH\SIAH\UploadedFiles\InsumosOcasa.csv'
    WITH
    (
    FIELDTERMINATOR = ';',  --CSV field delimiter
    ROWTERMINATOR = '\n'   --Use to shift the control to next row
    )