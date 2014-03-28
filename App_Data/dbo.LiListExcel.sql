


CREATE VIEW [dbo].[LiListExcel]
	AS SELECT 
       [ImName] '标识名称'
      ,[LoanUnit] '放款单位'
      ,[Mpr] '月利率'
      ,[StartTime] '起息日期'
      ,[CapitalAmount] '借入本金'
      ,[PayableInterest] '应付利息'
      ,[LastPayableDate] '最近结息日期'
      ,[BorrowedTime] '借入日期'
  FROM [dbo].[LiList]