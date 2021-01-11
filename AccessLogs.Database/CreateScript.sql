USE [AccessLogs]
GO 
CREATE TABLE [dbo].[Logs](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetimeoffset] NOT NULL,
	[Client] [nvarchar](255) NOT NULL,
	[Path] [nvarchar](255) NOT NULL,
	[QueryParameters] [nvarchar](255) NULL,
	[StatusCode] [int] NOT NULL,
	[Size] [int] NOT NULL,
	[CountryCode] [nvarchar](5) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
))

GO

CREATE NONCLUSTERED INDEX [IX_Logs_Date] ON [dbo].[Logs]
(
	[Date] ASC
)
INCLUDE([Client])

GO

CREATE PROCEDURE [dbo].[GetTopClients](
@topN INT,
@from DATETIMEOFFSET,
@to DATETIMEOFFSET
)
AS
SELECT TOP (@topN) [Client], COUNT(*) [Occurrences] 
FROM [dbo].[Logs] 
WHERE [Date] BETWEEN @from AND @to 
GROUP BY [Client]
ORDER BY [Occurrences] DESC