import request from '@/utils/request.js'

// 查询列表
export function getTransactionPaged(query) {
    return request({
        url: '/transaction/paged',
        method: 'get',
        params: query
    })
}

// 查询详细
export function getTransaction(id) {
    return request({
        url: '/transaction/' + id,
        method: 'get'
    })
}

// 新增
export function createTransaction(data) {
    return request({
        url: '/transaction/',
        method: 'post',
        data: data
    })
}

// 修改
export function updateTransaction(data) {
    return request({
        url: '/transaction/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除
export function deleteTransaction(id) {
    return request({
        url: '/transaction/' + id,
        method: 'delete'
    })
}

// 批量删除
export function batchDeleteTransaction(ids) {
    return request({
        url: '/transaction/batch/',
        method: 'delete',
        data: {ids}
    })
}

// 查询所有
export function getTransactionAll(query) {
    return request({
        url: '/transaction/all',
        method: 'get',
        params: query
    })
}

// 修改状态
export function changeTransactionStatus(id, status) {
    return request({
        url: '/transaction/change-status/' + id,
        method: 'put',
        data: {id, status}
    })
}

// 获取指定账本的实时数据快照
export function getTransactionLedgerSnapshot(query) {
    return request({
        url: '/transaction/ledger-snapshot',
        method: 'get',
        params: query
    })
}

// 获取本月支出分类排名 TopN
export function getTransactionMonthlyExpenseTransactionCategoryTopN(query) {
    return request({
        url: '/transaction/monthly-expense-transaction-category-top-n',
        method: 'get',
        params: query
    })
}

// 获取本月收入分类排名 TopN
export function getTransactionMonthlyIncomeTransactionCategoryTopN(query) {
    return request({
        url: '/transaction/monthly-income-transaction-category-top-n',
        method: 'get',
        params: query
    })
}

// 获取本月支出分类构成
export function getTransactionMonthlyExpenseTransactionCategory(query) {
    return request({
        url: '/transaction/monthly-expense-transaction-category',
        method: 'get',
        params: query
    })
}

// 获取本月单笔金额最大的前 10 笔支出交易记录
export function getTransactionMonthlyExpenseTransactionTopN(query) {
    return request({
        url: '/transaction/monthly-expense-transaction-top-n',
        method: 'get',
        params: query
    })
}

// 统计指定账本本年度每月的收入与支出金额
export function getTransactionYearlyStats(query) {
    return request({
        url: '/transaction/yearly-stats',
        method: 'get',
        params: query
    })
}