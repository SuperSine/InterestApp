

CREATE VIEW [dbo].[LiList]
	AS SELECT (SUM(isnull(mt.rate,0) * isnull(mt.duration,0)) * (cpint.IncrCapitalAmount-cpint.DecrCapitalAmount) + (cpint.IncrInterestAmount - cpint.DecrInterestAmount) ) as PayableInterest,mt.ImName,mt.LoanUnit,mt.Mpr,mt.StartTime,(cpint.IncrCapitalAmount-cpint.DecrCapitalAmount) as CapitalAmount,mt.Id,mt.LastPayableDate,mt.BorrowedTime,(cpint.IncrInterestAmount - cpint.DecrInterestAmount) as InterestAmount 
,(select top(1) Rate from RateDetails where InterestMasterId = mt.Id and GETDATE() >= Since order by Since desc) as CurrentRate
,cpint.DecrInterestAmount as PaiedInterest
,cpint.DecrCapitalAmount as PaiedCapital

FROM (
	SELECT (select top(1) VailedTime From TransactionDetails Where InterestMasterId = base.Id and Type = 'D2' order by VailedTime desc) as LastIncrIntrst
		,StartTime
		,base.Id
		,DATEDIFF(DAY, a.Since, isnull(b.Since, GETDATE())) AS duration
		,a.Rate
		,ImName
		,LoanUnit
		,Mpr
		,DATEADD(DAY,ROUND(DATEDIFF(DAY,StartTime,GETDATE()) / CycleOfPayment,0,1) * CycleOfPayment,StartTime) as LastPayableDate
		,BorrowedTime
	FROM InterestMasters base
	LEFT JOIN (
		SELECT ROW_NUMBER() OVER (
				ORDER BY [Id] ASC
				) AS oId
			,*
		FROM RateDetails
		) a ON a.InterestMasterId = base.Id
		AND a.Since >= isnull(LastIncrIntrst, StartTime) and a.Since < GETDATE()
	LEFT JOIN (
		SELECT ROW_NUMBER() OVER (
				ORDER BY [Id] ASC
				) - 1 AS oId
			,*
		FROM RateDetails
		) b ON a.oId = b.oId AND a.InterestMasterId = b.InterestMasterId
		AND b.Since >= isnull(LastIncrIntrst, StartTime) and b.Since < GETDATE()
	) mt
LEFT JOIN (
	SELECT isnull(SUM(CASE WHEN [Type] = 'I2' THEN Amount END),0) AS IncrCapitalAmount
		  ,isnull(SUM(CASE WHEN [Type] = 'I1' THEN Amount END),0) AS IncrInterestAmount
		  ,isnull(SUM(CASE WHEN [Type] = 'D2' THEN Amount END),0) AS DecrCapitalAmount
		  ,isnull(SUM(CASE WHEN [Type] = 'D1' THEN Amount END),0) AS DecrInterestAmount
		,InterestMasterId
	FROM TransactionDetails
	WHERE [Type] = 'I2'
		OR [Type] = 'D2'
		OR [Type] = 'I1'
		OR [Type] = 'D1'
	GROUP BY InterestMasterId
	) cpint ON cpint.InterestMasterId = mt.Id
GROUP BY mt.Id
	,DecrCapitalAmount
	,DecrInterestAmount,IncrCapitalAmount,IncrInterestAmount,mt.ImName,mt.LoanUnit,mt.Mpr,mt.StartTime,mt.LastPayableDate,mt.BorrowedTime