UPDATE [dbo].[Insumo]
   SET [stock] = stock + 'CANTIDADCOMPROMETIDA'
      ,[stockFisico] = stockFisico + 'CANTIDADFISICA'
 WHERE 'ID' = [id]                            