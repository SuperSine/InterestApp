
CREATE VIEW [dbo].[TdList]
	AS select * from TransactionDetails td where td.Type <> 'I1' and td.Type <> 'I2'