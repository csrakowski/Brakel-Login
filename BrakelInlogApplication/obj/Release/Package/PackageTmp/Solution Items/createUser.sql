USE [master]
GO
CREATE LOGIN [BrakelApplication] WITH PASSWORD=N'BrakelPassword', DEFAULT_DATABASE=[Brakel-Login], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO
USE [Brakel-Login]
GO
CREATE USER [BrakelApplication] FOR LOGIN [BrakelApplication]
GO
