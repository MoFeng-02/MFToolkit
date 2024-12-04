namespace MFToolkit.Utility;
/// <summary>
/// 金融库，通义千问生成
/// </summary>
public static class Finance
{
    /// <summary>
    /// 计算复利终值。
    /// </summary>
    /// <param name="principal">本金</param>
    /// <param name="rate">年利率（如5%应输入0.05）</param>
    /// <param name="years">年限</param>
    /// <returns>复利终值</returns>
    public static double CalculateCompoundInterest(double principal, double rate, int years)
    {
        return principal * Math.Pow(1 + rate, years);
    }

    /// <summary>
    /// 计算等额本息还款的每月还款额。
    /// </summary>
    /// <param name="loanAmount">贷款金额</param>
    /// <param name="annualRate">年利率</param>
    /// <param name="months">还款月数</param>
    /// <returns>每月还款额</returns>
    public static double CalculateMonthlyPayment(double loanAmount, double annualRate, int months)
    {
        double monthlyRate = annualRate / 12;
        return loanAmount * (monthlyRate * Math.Pow(1 + monthlyRate, months)) / (Math.Pow(1 + monthlyRate, months) - 1);
    }

    /// <summary>
    /// 计算投资回报率。
    /// </summary>
    /// <param name="gainFromInvestment">从投资中获得的总收益</param>
    /// <param name="costOfInvestment">总投资成本</param>
    /// <returns>投资回报率</returns>
    public static double CalculateROI(double gainFromInvestment, double costOfInvestment)
    {
        if (costOfInvestment == 0)
            throw new DivideByZeroException("总投资成本不能为零");

        return (gainFromInvestment - costOfInvestment) / costOfInvestment;
    }

    /// <summary>
    /// 计算折现率。
    /// </summary>
    /// <param name="futureValue">未来价值</param>
    /// <param name="presentValue">当前价值</param>
    /// <param name="years">年限</param>
    /// <returns>折现率</returns>
    public static double CalculateDiscountRate(double futureValue, double presentValue, int years)
    {
        if (presentValue == 0)
            throw new DivideByZeroException("当前价值不能为零");

        return Math.Pow(futureValue / presentValue, 1.0 / years) - 1;
    }
    /// <summary>
    /// 计算年金现值。
    /// </summary>
    /// <param name="payment">每期支付额</param>
    /// <param name="ratePerPeriod">每期利率</param>
    /// <param name="numberOfPeriods">期数</param>
    /// <returns>年金现值</returns>
    public static double CalculatePresentValueOfAnnuity(double payment, double ratePerPeriod, int numberOfPeriods)
    {
        return payment * (1 - Math.Pow(1 + ratePerPeriod, -numberOfPeriods)) / ratePerPeriod;
    }

    /// <summary>
    /// 计算未来价值（Future Value）。
    /// </summary>
    /// <param name="presentValue">当前价值</param>
    /// <param name="ratePerPeriod">每期利率</param>
    /// <param name="numberOfPeriods">期数</param>
    /// <param name="additionalPayments">每期额外支付（默认为0）</param>
    /// <returns>未来价值</returns>
    public static double CalculateFutureValue(double presentValue, double ratePerPeriod, int numberOfPeriods, double additionalPayments = 0)
    {
        double futureValue = presentValue * Math.Pow(1 + ratePerPeriod, numberOfPeriods);
        if (additionalPayments != 0)
        {
            futureValue += additionalPayments * ((Math.Pow(1 + ratePerPeriod, numberOfPeriods) - 1) / ratePerPeriod);
        }
        return futureValue;
    }

    /// <summary>
    /// 计算有效年利率（Effective Annual Rate, EAR）。
    /// </summary>
    /// <param name="nominalRate">名义年利率</param>
    /// <param name="compoundingFrequency">复利频率（一年内的复利次数）</param>
    /// <returns>有效年利率</returns>
    public static double CalculateEffectiveAnnualRate(double nominalRate, int compoundingFrequency)
    {
        return Math.Pow(1 + nominalRate / compoundingFrequency, compoundingFrequency) - 1;
    }

    /// <summary>
    /// 计算净现值（Net Present Value, NPV）。
    /// </summary>
    /// <param name="cashFlows">现金流数组，其中第一个元素是初始投资（通常为负数）</param>
    /// <param name="discountRate">折现率</param>
    /// <returns>净现值</returns>
    public static double CalculateNetPresentValue(double[] cashFlows, double discountRate)
    {
        double npv = 0;
        for (int i = 0; i < cashFlows.Length; i++)
        {
            npv += cashFlows[i] / Math.Pow(1 + discountRate, i);
        }
        return npv;
    }

    /// <summary>
    /// 计算内部收益率（Internal Rate of Return, IRR）使用二分法查找近似解。
    /// </summary>
    /// <param name="cashFlows">现金流数组，其中第一个元素是初始投资（通常为负数）</param>
    /// <returns>内部收益率</returns>
    public static double CalculateInternalRateOfReturn(double[] cashFlows, double guess = 0.1, double precision = 1e-6)
    {
        // 使用牛顿法或其他数值方法求解IRR是一个复杂的问题，这里简化处理，采用二分查找
        double lowerBound = -1, upperBound = 1;
        double irr = guess;

        while (Math.Abs(CalculateNetPresentValue(cashFlows, irr)) > precision)
        {
            double npv = CalculateNetPresentValue(cashFlows, irr);

            if (npv > 0)
                lowerBound = irr;
            else
                upperBound = irr;

            irr = (lowerBound + upperBound) / 2;

            // 防止无限循环
            if (upperBound - lowerBound < precision)
                break;
        }

        return irr;
    }
    /// <summary>
    /// 计算股票或投资组合的夏普比率（Sharpe Ratio）。
    /// </summary>
    /// <param name="expectedReturn">预期回报率</param>
    /// <param name="riskFreeRate">无风险利率</param>
    /// <param name="standardDeviation">标准差（风险度量）</param>
    /// <returns>夏普比率</returns>
    public static double CalculateSharpeRatio(double expectedReturn, double riskFreeRate, double standardDeviation)
    {
        if (standardDeviation == 0)
            throw new DivideByZeroException("标准差不能为零");

        return (expectedReturn - riskFreeRate) / standardDeviation;
    }

    /// <summary>
    /// 计算资产的贝塔系数（Beta），表示相对于市场指数的波动性。
    /// </summary>
    /// <param name="covariance">资产与市场指数的协方差</param>
    /// <param name="varianceOfMarket">市场指数的方差</param>
    /// <returns>贝塔系数</returns>
    public static double CalculateBeta(double covariance, double varianceOfMarket)
    {
        if (varianceOfMarket == 0)
            throw new DivideByZeroException("市场指数的方差不能为零");

        return covariance / varianceOfMarket;
    }

    /// <summary>
    /// 根据CAPM模型计算预期回报率。
    /// </summary>
    /// <param name="riskFreeRate">无风险利率</param>
    /// <param name="beta">贝塔系数</param>
    /// <param name="marketReturn">市场期望回报率</param>
    /// <returns>预期回报率</returns>
    public static double CalculateExpectedReturnCapm(double riskFreeRate, double beta, double marketReturn)
    {
        return riskFreeRate + beta * (marketReturn - riskFreeRate);
    }

    /// <summary>
    /// 计算调整后净现值（Adjusted Present Value, APV）。
    /// </summary>
    /// <param name="npv">基础项目的净现值</param>
    /// <param name="financingEffects">融资效应的价值</param>
    /// <returns>调整后的净现值</returns>
    public static double CalculateAdjustedPresentValue(double npv, double financingEffects)
    {
        return npv + financingEffects;
    }

    /// <summary>
    /// 计算持续增长模型下的公司估值（Gordon Growth Model）。
    /// </summary>
    /// <param name="nextYearDividend">下一年的股息</param>
    /// <param name="requiredRateOfReturn">要求的回报率</param>
    /// <param name="growthRate">股息增长率</param>
    /// <returns>公司估值</returns>
    public static double CalculateCompanyValuationGGM(double nextYearDividend, double requiredRateOfReturn, double growthRate)
    {
        if (requiredRateOfReturn <= growthRate)
            throw new ArgumentException("要求的回报率必须大于股息增长率");

        return nextYearDividend / (requiredRateOfReturn - growthRate);
    }

    /// <summary>
    /// 计算债券价格。
    /// </summary>
    /// <param name="faceValue">面值</param>
    /// <param name="couponRate">票面利率</param>
    /// <param name="yieldToMaturity">到期收益率</param>
    /// <param name="yearsToMaturity">到期年数</param>
    /// <param name="paymentsPerYear">每年支付次数</param>
    /// <returns>债券价格</returns>
    public static double CalculateBondPrice(double faceValue, double couponRate, double yieldToMaturity, int yearsToMaturity, int paymentsPerYear)
    {
        double payment = faceValue * couponRate / paymentsPerYear;
        double periodYield = yieldToMaturity / paymentsPerYear;
        int totalPayments = yearsToMaturity * paymentsPerYear;

        // 现值的利息部分
        double presentValueOfInterestPayments = CalculatePresentValueOfAnnuity(payment, periodYield, totalPayments);

        // 面值的现值
        double presentValueOfFaceValue = faceValue / Math.Pow(1 + periodYield, totalPayments);

        return presentValueOfInterestPayments + presentValueOfFaceValue;
    }
    /// <summary>
    /// 计算期权价格（使用布莱克-斯科尔斯模型）。
    /// </summary>
    /// <param name="spotPrice">标的资产当前价格</param>
    /// <param name="strikePrice">执行价格</param>
    /// <param name="timeToMaturity">到期时间（年）</param>
    /// <param name="riskFreeRate">无风险利率</param>
    /// <param name="volatility">波动率</param>
    /// <param name="isCall">是否为看涨期权</param>
    /// <returns>期权价格</returns>
    public static double CalculateOptionPrice(double spotPrice, double strikePrice, double timeToMaturity, double riskFreeRate, double volatility, bool isCall)
    {
        double d1 = (Math.Log(spotPrice / strikePrice) + (riskFreeRate + 0.5 * Math.Pow(volatility, 2)) * timeToMaturity) / (volatility * Math.Sqrt(timeToMaturity));
        double d2 = d1 - volatility * Math.Sqrt(timeToMaturity);
        double nd1 = NormalDistribution(d1);
        double nd2 = NormalDistribution(d2);

        if (isCall)
            return spotPrice * nd1 - strikePrice * Math.Exp(-riskFreeRate * timeToMaturity) * nd2;
        else // Put option
            return strikePrice * Math.Exp(-riskFreeRate * timeToMaturity) * (1 - nd2) - spotPrice * (1 - nd1);
    }

    /// <summary>
    /// 标准正态分布累积分布函数（CDF）。
    /// </summary>
    /// <param name="x">输入值</param>
    /// <returns>CDF值</returns>
    private static double NormalDistribution(double x)
    {
        // 使用近似公式计算标准正态分布的CDF
        double t = 1 / (1 + 0.2316419 * Math.Abs(x));
        double cdf = 1 - 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-x * x / 2) *
                     t * (0.319381530 + t * (-0.356563782 + t * (1.781477937 + t * (-1.821255978 + t * 1.330274429))));
        return x > 0 ? cdf : 1 - cdf;
    }

    /// <summary>
    /// 计算股票的詹森阿尔法（Jensen's Alpha）。
    /// </summary>
    /// <param name="portfolioReturn">投资组合回报率</param>
    /// <param name="marketReturn">市场回报率</param>
    /// <param name="riskFreeRate">无风险利率</param>
    /// <param name="beta">贝塔系数</param>
    /// <returns>詹森阿尔法</returns>
    public static double CalculateJensensAlpha(double portfolioReturn, double marketReturn, double riskFreeRate, double beta)
    {
        return portfolioReturn - (riskFreeRate + beta * (marketReturn - riskFreeRate));
    }

    /// <summary>
    /// 计算投资组合的风险价值（Value at Risk, VaR）。
    /// </summary>
    /// <param name="portfolioValue">投资组合价值</param>
    /// <param name="confidenceLevel">置信水平（例如：0.95表示95%置信水平）</param>
    /// <param name="holdingPeriod">持有期（天数）</param>
    /// <param name="dailyVolatility">每日波动率</param>
    /// <returns>风险价值VaR</returns>
    /// <exception cref="ArgumentOutOfRangeException" />
    public static double CalculateVaR(double portfolioValue, double confidenceLevel, int holdingPeriod, double dailyVolatility)
    {
        double zScore = NormalInverse(confidenceLevel); // 置信水平对应的Z分数
        return portfolioValue * (zScore * dailyVolatility * Math.Sqrt(holdingPeriod));
    }

    /// <summary>
    /// 标准正态分布逆累积分布函数（用于VaR计算等）。
    /// </summary>
    /// <param name="p">概率（0<p<1）</param>
    /// <returns>Z分数</returns>
    private static double NormalInverse(double p)
    {
        if (p <= 0 || p >= 1)
            throw new ArgumentOutOfRangeException(nameof(p), "概率必须在(0, 1)之间");

        // Constants for the approximation
        double[] a = { -3.969683028665376e+01, 2.209460984245205e+02, -2.759920031906780e+02,
                   1.383577518672690e+02, -3.066479806614716e+01, 2.506628277459239e+00 };
        double[] b = { -5.447609879822406e+01, 1.615858368580409e+02, -1.556989798598866e+02,
                   6.680131188771972e+01, -1.328068155288572e+01 };
        double[] c = { -7.784894002430293e-03, -3.223964580411365e-01, -2.400758277161838e+00,
                   -2.549732539343734e+00, 4.374664141464968e+00, 2.938163982698783e+00 };
        double[] d = { 7.784695709041462e-03, 3.224671290700398e-01, 2.445134137142996e+00,
                   3.754408661907416e+00 };

        double q, r;

        if (p < 0.5)
        {
            q = Math.Sqrt(-2 * Math.Log(p));
            r = (((a[0] * q + a[1]) * q + a[2]) * q + a[3]) * q + a[4];
            r = ((b[0] * q + b[1]) * q + b[2]) * q + b[3];
            return (r * q + a[5]) / (r + b[4]);
        }
        else if (p > 0.5)
        {
            q = Math.Sqrt(-2 * Math.Log(1 - p));
            r = (((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4];
            r = ((d[0] * q + d[1]) * q + d[2]) * q + d[3];
            return -(r * q + c[5]) / (r + d[4]);
        }
        else // p == 0.5
        {
            return 0.0;
        }
    }
    /// <summary>
    /// 计算期望短缺（Expected Shortfall, ES），也称为条件VaR（CVaR）。
    /// </summary>
    /// <param name="portfolioValue">投资组合价值</param>
    /// <param name="confidenceLevel">置信水平（例如：0.95表示95%置信水平）</param>
    /// <param name="holdingPeriod">持有期（天数）</param>
    /// <param name="dailyVolatility">每日波动率</param>
    /// <returns>期望短缺ES</returns>
    public static double CalculateExpectedShortfall(double portfolioValue, double confidenceLevel, int holdingPeriod, double dailyVolatility)
    {
        double zScore = NormalInverse(confidenceLevel);
        double var = CalculateVaR(portfolioValue, confidenceLevel, holdingPeriod, dailyVolatility);
        double es = portfolioValue * (zScore + Math.Sqrt(holdingPeriod) * dailyVolatility / (1 - confidenceLevel) * Math.Exp(-Math.Pow(zScore, 2) / 2) / Math.Sqrt(2 * Math.PI));
        return es;
    }
}
