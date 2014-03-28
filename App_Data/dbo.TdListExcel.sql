

CREATE VIEW [dbo].[TdListExcel]
	AS SELECT [Id]
      ,[Type] '业务类型'
      ,[Amount] '业务金额'
      ,[CreateTime] '创建时间'
      ,[VailedTime] '生效时间'
      ,[Mark] '备注'
      ,[EnableFlag]
  FROM [TdList]