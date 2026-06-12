const http = uni.$u.http

export const getLedgerAll = (data) => http.get('/ledger/all', data)
export const getLedgerPaged = (data) => http.get('/ledger/paged', data)
export const createLedger = (data, config) => http.post('/ledger', data, config)
export const updateLedger = (data, config) => http.put('/ledger/' + data.id, data, config)
export const deleteLedger = (data, config) => http.delete('/ledger/' + data.id, data, config)

export const getTransactionCategoryAll = (data) => http.get('/transaction-category/all', data)
export const createTransactionCategory = (data, config) => http.post('/transaction-category', data, config)
export const updateTransactionCategory = (data, config) => http.put('/transaction-category/' + data.id, data, config)
export const deleteTransactionCategory = (data, config) => http.delete('/transaction-category/' + data.id, data, config)

export const getTransactionPaged = (data) => http.get('/transaction/paged', data)
export const createTransaction = (data, config) => http.post('/transaction', data, config)
export const updateTransaction = (data, config) => http.put('/transaction/' + data.id, data, config)
export const deleteTransaction = (data, config) => http.delete('/transaction/' + data.id, data, config)

export const getTransactionRangeStats = (data) => http.get('/transaction/range-stats', data)
export const getTransactionEarliest = (data) => http.get('/transaction/earliest', data)
export const getTransactionLedgerSnapshot = (data) => http.get('/transaction/ledger-snapshot', data)
export const getTransactionMonthlyExpenseTransactionCategoryTopN = (data) => http.get('/transaction/monthly-expense-transaction-category-top-n', data)
export const getTransactionMonthlyIncomeTransactionCategoryTopN = (data) => http.get('/transaction/monthly-income-transaction-category-top-n', data)
export const getTransactionMonthlyExpenseTransactionTopN = (data) => http.get('/transaction/monthly-expense-transaction-top-n', data)
export const getTransactionMonthlyExpenseTransactionCategory = (data) => http.get('/transaction/monthly-expense-transaction-category', data)
export const getTransactionYearlyStats = (data) => http.get('/transaction/yearly-stats', data)

