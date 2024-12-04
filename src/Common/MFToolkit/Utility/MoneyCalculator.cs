using System.Globalization;

namespace MFToolkit.Utility;
/// <summary>
/// 提供货币计算功能的静态工具类。通义千问生成
/// </summary>
public static class MoneyCalculator
{
    /// <summary>
    /// 添加两个金额。
    /// </summary>
    /// <param name="amount1">第一个金额</param>
    /// <param name="amount2">第二个金额</param>
    /// <returns>两个金额相加后的结果，四舍五入到最近的分</returns>
    public static decimal Add(decimal amount1, decimal amount2)
    {
        return Math.Round(amount1 + amount2, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 从一个金额中减去另一个金额。
    /// </summary>
    /// <param name="amount1">被减数</param>
    /// <param name="amount2">减数</param>
    /// <returns>两个金额相减后的结果，四舍五入到最近的分</returns>
    public static decimal Subtract(decimal amount1, decimal amount2)
    {
        return Math.Round(amount1 - amount2, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 将金额乘以一个因子。
    /// </summary>
    /// <param name="amount">基数金额</param>
    /// <param name="factor">乘数因子</param>
    /// <returns>金额与因子相乘的结果，四舍五入到最近的分</returns>
    public static decimal Multiply(decimal amount, decimal factor)
    {
        return Math.Round(amount * factor, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 将金额除以一个除数。
    /// </summary>
    /// <param name="amount">被除数金额</param>
    /// <param name="divisor">除数</param>
    /// <returns>金额与除数相除的结果，如果除数为0，则返回null</returns>
    public static decimal? Divide(decimal amount, decimal divisor)
    {
        if (divisor == 0)
        {
            Console.WriteLine("错误：不允许除以零。");
            return null;
        }
        return Math.Round(amount / divisor, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 计算金额的百分比。
    /// </summary>
    /// <param name="amount">基数金额</param>
    /// <param name="percentage">百分比（必须在0和100之间）</param>
    /// <returns>金额的指定百分比值，四舍五入到最近的分</returns>
    /// <exception cref="ArgumentOutOfRangeException">当百分比不在0到100之间时抛出</exception>
    public static decimal CalculatePercentage(decimal amount, decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "百分比必须在0到100之间。");
        }
        return Math.Round(amount * (percentage / 100m), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 应用折扣到金额上。
    /// </summary>
    /// <param name="amount">原价金额</param>
    /// <param name="discountPercentage">折扣百分比</param>
    /// <returns>应用折扣后的金额</returns>
    public static decimal ApplyDiscount(decimal amount, decimal discountPercentage)
    {
        var discount = CalculatePercentage(amount, discountPercentage);
        return Subtract(amount, discount);
    }

    /// <summary>
    /// 将货币值四舍五入到最接近的分。
    /// </summary>
    /// <param name="amount">需要四舍五入的金额</param>
    /// <returns>四舍五入后的金额</returns>
    public static decimal RoundToNearestCent(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
    /// <summary>
    /// 将金额格式化为指定文化的货币字符串。
    /// </summary>
    /// <param name="amount">需要格式化的金额</param>
    /// <param name="cultureName">文化名称（如"zh-CN"表示中国人民币）</param>
    /// <returns>格式化后的货币字符串</returns>
    public static string FormatCurrency(decimal amount, string cultureName = "zh-CN")
    {
        var cultureInfo = new CultureInfo(cultureName);
        return amount.ToString("C", cultureInfo);
    }

    /// <summary>
    /// 比较两个金额是否相等。
    /// </summary>
    /// <param name="amount1">第一个金额</param>
    /// <param name="amount2">第二个金额</param>
    /// <returns>如果两个金额相等则返回true，否则返回false</returns>
    public static bool AreEqual(decimal amount1, decimal amount2)
    {
        return Math.Abs(amount1 - amount2) <= 0.005m; // 考虑到四舍五入误差
    }

    /// <summary>
    /// 判断一个金额是否大于另一个金额。
    /// </summary>
    /// <param name="amount1">要比较的金额</param>
    /// <param name="amount2">基准金额</param>
    /// <returns>如果amount1大于amount2则返回true，否则返回false</returns>
    public static bool IsGreaterThan(decimal amount1, decimal amount2)
    {
        return amount1 > amount2;
    }

    /// <summary>
    /// 计算复利。
    /// </summary>
    /// <param name="principal">本金</param>
    /// <param name="rate">年利率（以小数形式给出，如0.05代表5%）</param>
    /// <param name="timesPerYear">每年计息次数</param>
    /// <param name="years">投资年限</param>
    /// <returns>最终金额，考虑了复利增长</returns>
    public static decimal CalculateCompoundInterest(decimal principal, decimal rate, int timesPerYear, int years)
    {
        if (timesPerYear <= 0 || years < 0 || rate < 0)
        {
            throw new ArgumentException("参数无效。");
        }
        decimal factor = 1 + (rate / timesPerYear);
        decimal power = Convert.ToDecimal(Math.Pow((double)factor, timesPerYear * years));
        return RoundToNearestCent(principal * power);
    }

    /// <summary>
    /// 计算贷款的月还款额。
    /// </summary>
    /// <param name="loanAmount">贷款金额</param>
    /// <param name="annualRate">年利率（以小数形式给出，如0.05代表5%）</param>
    /// <param name="loanTermInYears">贷款期限（以年为单位）</param>
    /// <returns>每月应还金额</returns>
    public static decimal CalculateLoanPayment(decimal loanAmount, decimal annualRate, int loanTermInYears)
    {
        if (loanTermInYears <= 0 || annualRate < 0)
        {
            throw new ArgumentException("参数无效。");
        }
        int totalPayments = loanTermInYears * 12;
        decimal monthlyRate = annualRate / 12;
        return RoundToNearestCent(loanAmount * (monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -totalPayments))));
    }

    /// <summary>
    /// 计算年金终值。
    /// </summary>
    /// <param name="payment">每期支付金额</param>
    /// <param name="rate">每期利率（以小数形式给出）</param>
    /// <param name="periods">总期数</param>
    /// <returns>年金终值</returns>
    public static decimal CalculateFutureValueOfAnnuity(decimal payment, decimal rate, int periods)
    {
        if (periods <= 0 || rate < 0)
        {
            throw new ArgumentException("参数无效。");
        }
        decimal factor = (decimal)Math.Pow(1 + (double)rate, periods);
        return RoundToNearestCent(payment * ((factor - 1) / rate));
    }
    /// <summary>
    /// 计算等额本息还款方式的每月还款额。
    /// </summary>
    /// <param name="loanAmount">贷款金额</param>
    /// <param name="annualRate">年利率（以小数形式给出，如0.05代表5%）</param>
    /// <param name="loanTermInYears">贷款期限（以年为单位）</param>
    /// <returns>每月应还金额</returns>
    public static decimal CalculateEqualPrincipalAndInterestPayment(decimal loanAmount, decimal annualRate, int loanTermInYears)
    {
        if (loanTermInYears <= 0 || annualRate < 0)
        {
            throw new ArgumentException("参数无效。");
        }
        int totalPayments = loanTermInYears * 12;
        decimal monthlyRate = annualRate / 12;
        return Math.Round(loanAmount * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, totalPayments)) / ((decimal)Math.Pow(1 + (double)monthlyRate, totalPayments) - 1), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 计算等额本金还款方式的某月还款额。
    /// </summary>
    /// <param name="loanAmount">贷款金额</param>
    /// <param name="annualRate">年利率（以小数形式给出，如0.05代表5%）</param>
    /// <param name="loanTermInYears">贷款期限（以年为单位）</param>
    /// <param name="month">要计算的月份</param>
    /// <returns>指定月份的还款额</returns>
    public static decimal CalculateEqualPrincipalPayment(decimal loanAmount, decimal annualRate, int loanTermInYears, int month)
    {
        if (loanTermInYears <= 0 || annualRate < 0 || month <= 0 || month > loanTermInYears * 12)
        {
            throw new ArgumentException("参数无效。");
        }
        decimal monthlyRate = annualRate / 12;
        decimal principalPerMonth = loanAmount / (loanTermInYears * 12);
        decimal interestForMonth = loanAmount * monthlyRate * (loanTermInYears * 12 - month + 1) / (loanTermInYears * 12);
        return Math.Round(principalPerMonth + interestForMonth, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 计算投资回报率（ROI）。
    /// </summary>
    /// <param name="netProfit">净利润</param>
    /// <param name="costOfInvestment">投资成本</param>
    /// <returns>投资回报率，以百分比表示</returns>
    public static decimal CalculateROI(decimal netProfit, decimal costOfInvestment)
    {
        if (costOfInvestment <= 0)
        {
            throw new ArgumentException("投资成本必须大于零。");
        }
        return Math.Round(netProfit / costOfInvestment * 100m, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 计算内部收益率（IRR）。
    /// </summary>
    /// <param name="cashFlows">现金流数组，第一个元素是初始投资（负值），后续是各期现金流</param>
    /// <returns>内部收益率，以小数表示</returns>
    public static double CalculateIRR(double[] cashFlows)
    {
        Func<double, double> irrFunction = x => cashFlows.Select((cf, i) => cf / Math.Pow(1 + x, i)).Sum();
        double guess = 0.1; // 初始猜测值
        double result = NewtonRaphson(irrFunction, guess);
        return result;

        double NewtonRaphson(Func<double, double> f, double initialGuess)
        {
            const double tolerance = 1e-7;
            double x = initialGuess;
            double fx = f(x);
            while (Math.Abs(fx) > tolerance)
            {
                double dfx = (f(x + tolerance) - fx) / tolerance;
                if (Math.Abs(dfx) < tolerance)
                    throw new InvalidOperationException("导数接近零，无法继续迭代。");
                x -= fx / dfx;
                fx = f(x);
            }
            return x;
        }
    }

    /// <summary>
    /// 将一个大金额拆分为若干个小金额，适用于现金分配或找零。
    /// </summary>
    /// <param name="amount">需要拆分的总金额</param>
    /// <param name="denominations">面额数组，按降序排列</param>
    /// <returns>一个字典，键为面额，值为该面额的数量</returns>
    public static Dictionary<decimal, int> SplitAmount(decimal amount, decimal[] denominations)
    {
        var result = new Dictionary<decimal, int>();
        foreach (var denomination in denominations)
        {
            int count = (int)(amount / denomination);
            if (count > 0)
            {
                result[denomination] = count;
                amount -= count * denomination;
            }
        }
        if (amount > 0)
        {
            result[amount] = 1; // 最后剩余的小数部分作为最小面额处理
        }
        return result;
    }
    /// <summary>
    /// 安全地添加两个金额，确保不会发生溢出。
    /// </summary>
    /// <param name="amount1">第一个金额</param>
    /// <param name="amount2">第二个金额</param>
    /// <returns>两个金额相加后的结果，四舍五入到最近的分</returns>
    /// <exception cref="OverflowException">如果操作导致溢出，则抛出此异常</exception>
    public static decimal SafeAdd(decimal amount1, decimal amount2)
    {
        try
        {
            checked
            {
                return Math.Round(amount1 + amount2, 2, MidpointRounding.AwayFromZero);
            }
        }
        catch (OverflowException)
        {
            throw new OverflowException("金额相加操作导致溢出。");
        }
    }

    /// <summary>
    /// 安全地减去一个金额从另一个金额，确保不会发生溢出。
    /// </summary>
    /// <param name="amount1">被减数</param>
    /// <param name="amount2">减数</param>
    /// <returns>两个金额相减后的结果，四舍五入到最近的分</returns>
    /// <exception cref="OverflowException">如果操作导致溢出，则抛出此异常</exception>
    public static decimal SafeSubtract(decimal amount1, decimal amount2)
    {
        try
        {
            checked
            {
                return Math.Round(amount1 - amount2, 2, MidpointRounding.AwayFromZero);
            }
        }
        catch (OverflowException)
        {
            throw new OverflowException("金额相减操作导致溢出。");
        }
    }

    /// <summary>
    /// 安全地乘以一个金额和一个因子，确保不会发生溢出。
    /// </summary>
    /// <param name="amount">基数金额</param>
    /// <param name="factor">乘数因子</param>
    /// <returns>金额与因子相乘的结果，四舍五入到最近的分</returns>
    /// <exception cref="OverflowException">如果操作导致溢出，则抛出此异常</exception>
    public static decimal SafeMultiply(decimal amount, decimal factor)
    {
        try
        {
            checked
            {
                return Math.Round(amount * factor, 2, MidpointRounding.AwayFromZero);
            }
        }
        catch (OverflowException)
        {
            throw new OverflowException("金额相乘操作导致溢出。");
        }
    }

    /// <summary>
    /// 安全地除以一个金额和一个除数，确保不会发生溢出或除以零的情况。
    /// </summary>
    /// <param name="amount">被除数金额</param>
    /// <param name="divisor">除数</param>
    /// <returns>金额与除数相除的结果，四舍五入到最近的分</returns>
    /// <exception cref="DivideByZeroException">如果除数为0，则抛出此异常</exception>
    /// <exception cref="OverflowException">如果操作导致溢出，则抛出此异常</exception>
    public static decimal SafeDivide(decimal amount, decimal divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("除数不能为零。");
        }

        try
        {
            checked
            {
                return Math.Round(amount / divisor, 2, MidpointRounding.AwayFromZero);
            }
        }
        catch (OverflowException)
        {
            throw new OverflowException("金额相除操作导致溢出。");
        }
    }

    /// <summary>
    /// 精确地计算金额的百分比，确保不会超过合理的范围。
    /// </summary>
    /// <param name="amount">基数金额</param>
    /// <param name="percentage">百分比（必须在0和100之间）</param>
    /// <returns>金额的指定百分比值，四舍五入到最近的分</returns>
    /// <exception cref="ArgumentOutOfRangeException">当百分比不在0到100之间时抛出</exception>
    public static decimal PreciseCalculatePercentage(decimal amount, decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "百分比必须在0到100之间。");
        }
        return Math.Round(amount * (percentage / 100m), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 检查金额是否在有效的范围内。
    /// </summary>
    /// <param name="amount">要检查的金额</param>
    /// <param name="minValue">最小允许值</param>
    /// <param name="maxValue">最大允许值</param>
    /// <exception cref="ArgumentOutOfRangeException">如果金额超出范围，则抛出此异常</exception>
    public static void ValidateAmountInRange(decimal amount, decimal minValue, decimal maxValue)
    {
        if (amount < minValue || amount > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), $"金额必须在{minValue}和{maxValue}之间。");
        }
    }
    /// <summary>
    /// 比较两个金额是否相等，考虑到舍入误差。
    /// </summary>
    /// <param name="amount1">第一个金额</param>
    /// <param name="amount2">第二个金额</param>
    /// <returns>如果两个金额在考虑舍入误差后相等，则返回true；否则返回false。</returns>
    public static bool AreAmountsEqual(decimal amount1, decimal amount2)
    {
        // 使用一个很小的容差值来比较两个金额是否相等
        return Math.Abs(amount1 - amount2) <= 0.005m;
    }

    /// <summary>
    /// 格式化金额为指定文化的货币字符串，并可以选择显示特定的小数位数。
    /// </summary>
    /// <param name="amount">需要格式化的金额</param>
    /// <param name="cultureName">文化名称（如"zh-CN"表示中国人民币）</param>
    /// <param name="decimalDigits">要显示的小数位数，默认为2</param>
    /// <returns>格式化后的货币字符串</returns>
    public static string FormatCurrency(decimal amount, string cultureName = "zh-CN", int decimalDigits = 2)
    {
        var cultureInfo = new CultureInfo(cultureName);
        return amount.ToString($"C{decimalDigits}", cultureInfo);
    }

    /// <summary>
    /// 将一种货币金额转换为另一种货币金额，使用给定的汇率。
    /// </summary>
    /// <param name="amount">原始金额</param>
    /// <param name="exchangeRate">汇率（从原始货币到目标货币）</param>
    /// <returns>转换后的金额，四舍五入到最近的分</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果汇率小于或等于零，则抛出此异常</exception>
    public static decimal ConvertCurrency(decimal amount, decimal exchangeRate)
    {
        if (exchangeRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "汇率必须大于零。");
        }
        return Math.Round(amount * exchangeRate, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 获取多个金额中的最大值。
    /// </summary>
    /// <param name="amounts">金额数组</param>
    /// <returns>最大金额</returns>
    /// <exception cref="ArgumentException">如果传入的数组为空，则抛出此异常</exception>
    public static decimal GetMaxAmount(params decimal[] amounts)
    {
        if (amounts == null || amounts.Length == 0)
        {
            throw new ArgumentException("金额数组不能为空。");
        }
        return amounts.Max();
    }

    /// <summary>
    /// 获取多个金额中的最小值。
    /// </summary>
    /// <param name="amounts">金额数组</param>
    /// <returns>最小金额</returns>
    /// <exception cref="ArgumentException">如果传入的数组为空，则抛出此异常</exception>
    public static decimal GetMinAmount(params decimal[] amounts)
    {
        if (amounts == null || amounts.Length == 0)
        {
            throw new ArgumentException("金额数组不能为空。");
        }
        return amounts.Min();
    }

    /// <summary>
    /// 计算一组金额的总和。
    /// </summary>
    /// <param name="amounts">金额数组</param>
    /// <returns>金额总和，四舍五入到最近的分</returns>
    /// <exception cref="ArgumentException">如果传入的数组为空，则抛出此异常</exception>
    public static decimal SumAmounts(params decimal[] amounts)
    {
        if (amounts == null || amounts.Length == 0)
        {
            throw new ArgumentException("金额数组不能为空。");
        }
        return Math.Round(amounts.Sum(), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 计算一组金额的平均值。
    /// </summary>
    /// <param name="amounts">金额数组</param>
    /// <returns>金额平均值，四舍五入到最近的分</returns>
    /// <exception cref="ArgumentException">如果传入的数组为空，则抛出此异常</exception>
    public static decimal AverageAmounts(params decimal[] amounts)
    {
        if (amounts == null || amounts.Length == 0)
        {
            throw new ArgumentException("金额数组不能为空。");
        }
        return Math.Round(amounts.Average(), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 累计应用一系列折扣到一个初始金额上。
    /// </summary>
    /// <param name="initialAmount">初始金额</param>
    /// <param name="discountPercentages">一系列折扣百分比</param>
    /// <returns>应用所有折扣后的最终金额</returns>
    public static decimal ApplyMultipleDiscounts(decimal initialAmount, params decimal[] discountPercentages)
    {
        decimal finalAmount = initialAmount;
        foreach (var discount in discountPercentages)
        {
            if (discount < 0 || discount > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(discount), "折扣百分比必须在0到100之间。");
            }
            finalAmount = SafeSubtract(finalAmount, PreciseCalculatePercentage(finalAmount, discount));
        }
        return finalAmount;
    }
}