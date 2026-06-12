using Bedrock.Core.Entities;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using SqlSugar;
using System.Linq.Expressions;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 交易仓储的具体实现
    /// </summary>
    public class TransactionRepository2
    {
        private readonly ISqlSugarClient _db; // 数据库上下文实例

        /// <summary>
        /// 构造函数注入数据库上下文。
        /// </summary>
        /// <param name="db">SqlSugar 数据库上下文。</param>
        public TransactionRepository2(ISqlSugarClient db)
        {
            _db = db;
        }

      

        ///// <summary>
        ///// 获取最早一笔交易摘要
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <returns></returns>
        //public async Task<TransactionSummary> GetEarliestByLedgerIdAsync(Guid ledgerId)
        //{
        //    var transaction = await _db.Queryable<Transaction, Ledger, Category>((t, l, c) => new object[]
        //        {
        //            JoinType.Left, t.LedgerId == l.Id,
        //            JoinType.Left, t.CategoryId == c.Id,
        //        })
        //        .Where(t => t.DeletedAt == null && t.LedgerId == ledgerId)
        //        .OrderBy((t, l, c) => t.Date, OrderByType.Asc)
        //        .OrderBy((t, l, c) => t.CreatedAt, OrderByType.Asc)
        //        .Select((t, l, c) => new TransactionSummary
        //        {
        //            Id = t.Id,
        //            LedgerId = l.Id,
        //            LedgerName = l.Name,
        //            CategoryId = c.Id,
        //            CategoryName = c.Name,
        //            CategoryType = c.Type,
        //            CategoryIcon = c.Icon,
        //            Amount = t.Amount,
        //            Description = t.Description,
        //            Date = t.Date,
        //            CreatedById = t.CreatedById,
        //            CreatedAt = t.CreatedAt,
        //            UpdatedById = t.UpdatedById,
        //            UpdatedAt = t.UpdatedAt,
        //            DeletedById = t.DeletedById,
        //            DeletedAt = t.DeletedAt
        //        })
        //        .FirstAsync();

        //    return transaction;
        //}

        ///// <summary>
        ///// 获取前N笔支出交易
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <param name="top"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<TransactionSummary>> GetTopExpenseSummaryAsync(Guid ledgerId, DateTime startDate, DateTime endDate, int top)
        //{
        //    var query = _db.Queryable<Transaction, Ledger, Category>((t, l, c) => new object[]
        //    {
        //        JoinType.Left, t.LedgerId == l.Id,
        //        JoinType.Left, t.CategoryId == c.Id
        //    })
        //    .Where((t, l, c) => t.DeletedAt == null && t.LedgerId == ledgerId && c.Type == "支出"
        //    && t.Date >= startDate && t.Date <= endDate);

        //    var result = await query
        //        .OrderBy(t => t.Amount, OrderByType.Desc) // 按交易金额降序排列
        //        .OrderBy((t, l, c) => t.CreatedAt, OrderByType.Desc)
        //        .Select((t, l, c) => new TransactionSummary
        //        {
        //            Id = t.Id,
        //            LedgerId = l.Id,
        //            LedgerName = l.Name,
        //            CategoryId = c.Id,
        //            CategoryName = c.Name,
        //            CategoryType = c.Type,
        //            CategoryIcon = c.Icon,
        //            Amount = t.Amount,
        //            Description = t.Description,
        //            Date = t.Date,
        //            CreatedById = t.CreatedById,
        //            CreatedAt = t.CreatedAt,
        //            UpdatedById = t.UpdatedById,
        //            UpdatedAt = t.UpdatedAt,
        //            DeletedById = t.DeletedById,
        //            DeletedAt = t.DeletedAt
        //        })
        //        .Take(top) // 取前top条记录
        //        .ToListAsync();

        //    return result;
        //}

        ///// <summary>
        ///// 获取分类金额排名
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <param name="categoryType"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<CategoryTotalAmountDto>> GetCategoryTotalAmountRankingAsync(Guid ledgerId, DateTime startDate, DateTime endDate, string? categoryType = null)
        //{
        //    var query = _db.Queryable<Transaction, Ledger, Category>((t, l, c) => new object[]
        //    {
        //        JoinType.Left, t.LedgerId == l.Id,
        //        JoinType.Left, t.CategoryId == c.Id
        //    })
        //    .Where((t, l, c) => t.DeletedAt == null && t.LedgerId == ledgerId && t.Date >= startDate && t.Date <= endDate);

        //    if (!string.IsNullOrWhiteSpace(categoryType))
        //    {
        //        query = query.Where((t, l, c) => c.Type == categoryType);
        //    }

        //    var result = await query
        //    .GroupBy((t, l, c) => new { c.Id, c.Name }) // 按照分类ID和名称进行分组
        //    .Select((t, l, c) => new CategoryTotalAmountDto
        //    {
        //        CategoryId = c.Id,
        //        CategoryName = c.Name,
        //        CategoryType = c.Type,
        //        TotalAmount = SqlFunc.AggregateSum(t.Amount) // 计算每个分类的总金额
        //    })
        //    .MergeTable()
        //    .OrderBy("TotalAmount Desc") // 按总金额降序排列
        //    .ToListAsync();

        //    return result;
        //}

        ///// <summary>
        ///// 根据账本、年、分类类型获取每月总金额
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <param name="categoryType"></param>
        ///// <param name="year"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<MonthlyTotalAmountDto>> GetMonthlyTotalsAsync(Guid ledgerId, string categoryType, int year)
        //{
        //    var query = _db.Queryable<Transaction, Category>((t, c) => t.CategoryId == c.Id)
        //        .Where((t, c) => t.DeletedAt == null && t.LedgerId == ledgerId && t.Date.Year == year && c.Type == categoryType);

        //    var result = await query
        //    .GroupBy((t, c) => new { t.Date.Month })
        //    .Select((t, c) => new MonthlyTotalAmountDto
        //    {
        //        Month = t.Date.Month,
        //        TotalAmount = SqlFunc.AggregateSum(t.Amount)
        //    })
        //    .MergeTable()
        //    .OrderBy("Month Asc") // 按月份排序
        //    .ToListAsync();

        //    // 确保每个月都有记录，即使没有交易时也返回0
        //    var allMonths = Enumerable.Range(1, 12).Select(month =>
        //    {
        //        var total = result.FirstOrDefault(m => m.Month == month)?.TotalAmount ?? 0;
        //        return new MonthlyTotalAmountDto { Month = month, TotalAmount = total };
        //    });

        //    return allMonths;
        //}

        ///// <summary>
        ///// 获取当前财务摘要
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <param name="datetimeNow"></param>
        ///// <returns></returns>
        //public async Task<FinancialSummaryDto> GetFinancialSummariesAsync(Guid ledgerId, DateTime datetimeNow)
        //{
        //    // 定义当前年的时间范围
        //    var startDateOfYear = new DateTime(datetimeNow.Year, 1, 1);
        //    var endDateOfYear = startDateOfYear.AddYears(1).AddMilliseconds(-1); // 当年的最后一毫秒

        //    // 上一年的时间范围
        //    var lastYearStartDate = new DateTime(datetimeNow.Year - 1, 1, 1);
        //    var lastYearEndDate = lastYearStartDate.AddYears(1).AddMilliseconds(-1); // 上一年的最后一毫秒

        //    // 当前月的时间范围
        //    var startDateOfMonth = new DateTime(datetimeNow.Year, datetimeNow.Month, 1);
        //    var endOfMonth = startDateOfMonth.AddMonths(1).AddMilliseconds(-1); // 当月的最后一毫秒

        //    // 上一个月的时间范围
        //    var lastMonthStartDate = startDateOfMonth.AddMonths(-1);
        //    var lastMonthEndDate = startDateOfMonth.AddMilliseconds(-1);

        //    // 当前周的时间范围（假设一周的第一天是星期一）
        //    var startOfWeek = datetimeNow.AddDays(-(datetimeNow.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)datetimeNow.DayOfWeek - 1)).Date;
        //    var endOfWeek = startOfWeek.AddDays(7).AddMilliseconds(-1); // 本周的最后一毫秒

        //    // 上一周的时间范围
        //    var lastWeekStart = startOfWeek.AddDays(-7);
        //    var lastWeekEnd = endOfWeek.AddDays(-7);

        //    // 当前日的时间范围
        //    var startDateOfDay = datetimeNow.Date;
        //    var endOfDay = startDateOfDay.AddDays(1).AddMilliseconds(-1); // 当天的最后一毫秒

        //    // 昨日的时间范围
        //    var yesterdayStart = startDateOfDay.AddDays(-1);
        //    var yesterdayEnd = endOfDay.AddDays(-1);

        //    // 定义条件生成器
        //    Func<DateTime, DateTime, string, Expression<Func<Transaction, Category, bool>>> conditions = (start, end, categoryType) =>
        //        (t, c) => t.LedgerId == ledgerId && c.Type == categoryType && t.Date >= start && t.Date <= end;

        //    // 计算各个时间段的收入和支出
        //    var currentYearIncomeTotal = await CalculateTotal(ledgerId, startDateOfYear, endDateOfYear, "收入", conditions);
        //    var currentYearExpenseTotal = await CalculateTotal(ledgerId, startDateOfYear, endDateOfYear, "支出", conditions);

        //    var lastYearIncomeTotal = await CalculateTotal(ledgerId, lastYearStartDate, lastYearEndDate, "收入", conditions);
        //    var lastYearExpenseTotal = await CalculateTotal(ledgerId, lastYearStartDate, lastYearEndDate, "支出", conditions);

        //    var currentMonthIncomeTotal = await CalculateTotal(ledgerId, startDateOfMonth, endOfMonth, "收入", conditions);
        //    var currentMonthExpenseTotal = await CalculateTotal(ledgerId, startDateOfMonth, endOfMonth, "支出", conditions);

        //    var lastMonthIncomeTotal = await CalculateTotal(ledgerId, lastMonthStartDate, lastMonthEndDate, "收入", conditions);
        //    var lastMonthExpenseTotal = await CalculateTotal(ledgerId, lastMonthStartDate, lastMonthEndDate, "支出", conditions);

        //    var currentWeekIncomeTotal = await CalculateTotal(ledgerId, startOfWeek, endOfWeek, "收入", conditions);
        //    var currentWeekExpenseTotal = await CalculateTotal(ledgerId, startOfWeek, endOfWeek, "支出", conditions);

        //    var lastWeekIncomeTotal = await CalculateTotal(ledgerId, lastWeekStart, lastWeekEnd, "收入", conditions);
        //    var lastWeekExpenseTotal = await CalculateTotal(ledgerId, lastWeekStart, lastWeekEnd, "支出", conditions);

        //    var currentDayIncomeTotal = await CalculateTotal(ledgerId, startDateOfDay, endOfDay, "收入", conditions);
        //    var currentDayExpenseTotal = await CalculateTotal(ledgerId, startDateOfDay, endOfDay, "支出", conditions);

        //    var yesterdayIncomeTotal = await CalculateTotal(ledgerId, yesterdayStart, yesterdayEnd, "收入", conditions);
        //    var yesterdayExpenseTotal = await CalculateTotal(ledgerId, yesterdayStart, yesterdayEnd, "支出", conditions);

        //    // 构建并返回 FinancialSummaryDto 对象
        //    return new FinancialSummaryDto
        //    {
        //        CurrentYearIncomeTotal = currentYearIncomeTotal,
        //        CurrentYearExpenseTotal = currentYearExpenseTotal,
        //        LastYearIncomeTotal = lastYearIncomeTotal,
        //        LastYearExpenseTotal = lastYearExpenseTotal,
        //        CurrentMonthIncomeTotal = currentMonthIncomeTotal,
        //        CurrentMonthExpenseTotal = currentMonthExpenseTotal,
        //        LastMonthIncomeTotal = lastMonthIncomeTotal,
        //        LastMonthExpenseTotal = lastMonthExpenseTotal,
        //        CurrentWeekIncomeTotal = currentWeekIncomeTotal,
        //        CurrentWeekExpenseTotal = currentWeekExpenseTotal,
        //        LastWeekIncomeTotal = lastWeekIncomeTotal,
        //        LastWeekExpenseTotal = lastWeekExpenseTotal,
        //        CurrentDayIncomeTotal = currentDayIncomeTotal,
        //        CurrentDayExpenseTotal = currentDayExpenseTotal,
        //        YesterdayIncomeTotal = yesterdayIncomeTotal,
        //        YesterdayExpenseTotal = yesterdayExpenseTotal
        //    };
        //}

        ///// <summary>
        ///// 获取指定时间范围内的收入和支出总额
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <returns></returns>
        //public async Task<DateRangeTotalAmountDto> GetDateRangeTotalAmountAsync(Guid ledgerId, DateTime startDate, DateTime endDate)
        //{
        //    // 定义条件生成器
        //    Func<DateTime, DateTime, string, Expression<Func<Transaction, Category, bool>>> conditions = (startDate, endDate, categoryType) =>
        //        (t, c) => t.LedgerId == ledgerId && c.Type == categoryType && t.Date >= startDate && t.Date <= endDate;

        //    // 计算各个时间段的收入和支出
        //    var incomeTotal = await CalculateTotal(ledgerId, startDate, endDate, "收入", conditions);
        //    var expenseTotal = await CalculateTotal(ledgerId, startDate, endDate, "支出", conditions);

        //    return new DateRangeTotalAmountDto
        //    {
        //        IncomeTotal = incomeTotal,
        //        ExpenseTotal = expenseTotal,
        //    };
        //}

        ///// <summary>
        ///// 计算指定时间范围和类别类型的收入和支出总额
        ///// </summary>
        ///// <param name="ledgerId"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <param name="categoryType"></param>
        ///// <param name="conditionFunc"></param>
        ///// <returns></returns>
        //private async Task<decimal?> CalculateTotal(Guid ledgerId, DateTime startDate, DateTime endDate, string categoryType,
        //    Func<DateTime, DateTime, string, Expression<Func<Transaction, Category, bool>>> conditionFunc)
        //{
        //    var total = await _db.Queryable<Transaction, Category>((t, c) => t.CategoryId == c.Id)
        //                         .Where(conditionFunc(startDate, endDate, categoryType))
        //                         .Select(t => SqlFunc.AggregateSum(t.Amount))
        //                         .FirstAsync();
        //    return total;
        //}
    }
}