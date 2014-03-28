SELECT isnull((SUM(mt.rate * mt.duration) * cpint.CapitalAmount + cpint.InterestAmount ),0) as PayableInterest,mt.ImName,mt.LoanUnit,mt.Mpr,mt.StartTime,cpint.CapitalAmount,cpint.InterestAmount,mt.Id
,isnull((select top(1) Rate from RateDetails where InterestMasterId = mt.Id and GETDATE() >= Since order by Since desc),0) as CurrentRate
FROM (
	SELECT (select top(1) VailedTime From TransactionDetails Where InterestMasterId = base.Id and Type = 'D2' order by VailedTime desc) as LastIncrIntrst
		,StartTime
		,base.Id
		,DATEDIFF(DAY, a.Since, isnull(b.Since, {1})) AS duration
		,a.Rate
		,ImName
		,LoanUnit
		,Mpr
	FROM InterestMasters base
	LEFT JOIN (
		SELECT ROW_NUMBER() OVER (
				ORDER BY [Id] ASC
				) AS oId
			,*
		FROM RateDetails
		) a ON a.InterestMasterId = base.Id
		AND a.Since >= isnull(LastIncrIntrst, StartTime) and a.Since < {1}
	LEFT JOIN (
		SELECT ROW_NUMBER() OVER (
				ORDER BY [Id] ASC
				) - 1 AS oId
			,*
		FROM RateDetails
		) b ON a.oId = b.oId AND a.InterestMasterId = b.InterestMasterId
		AND b.Since >= isnull(LastIncrIntrst, StartTime) and b.Since < {1}
	) mt
LEFT JOIN (
	SELECT isnull(SUM(CASE 
					WHEN [Type] = 'I2'
						THEN Amount
					WHEN [Type] = 'D2'
						THEN Amount * - 1
					END), 0) AS CapitalAmount
		,isnull(SUM(CASE 
					WHEN [Type] = 'I1'
						THEN Amount
					WHEN [Type] = 'D1'
						THEN Amount * - 1
					END), 0) AS InterestAmount
		,InterestMasterId
	FROM TransactionDetails
	WHERE [Type] = 'I2'
		OR [Type] = 'D2'
		OR [Type] = 'I1'
		OR [Type] = 'D1'
	GROUP BY InterestMasterId
	) cpint ON cpint.InterestMasterId = mt.Id WHERE mt.Id = {0}
GROUP BY mt.Id
	,CapitalAmount
	,InterestAmount,mt.ImName,mt.LoanUnit,mt.Mpr,mt.StartTime