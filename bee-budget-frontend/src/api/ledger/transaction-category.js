import request from '@/utils/request.js'

// 查询交易分类列表
export function getTransactionCategoryPaged(query) {
    return request({
        url: '/transaction-category/paged',
        method: 'get',
        params: query
    })
}

// 查询交易分类详细
export function getTransactionCategory(id) {
    return request({
        url: '/transaction-category/' + id,
        method: 'get'
    })
}

// 新增交易分类
export function createTransactionCategory(data) {
    return request({
        url: '/transaction-category/',
        method: 'post',
        data: data
    })
}

// 修改交易分类
export function updateTransactionCategory(data) {
    return request({
        url: '/transaction-category/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除交易分类
export function deleteTransactionCategory(id) {
    return request({
        url: '/transaction-category/' + id,
        method: 'delete'
    })
}

// 批量删除交易分类
export function batchDeleteTransactionCategory(ids) {
    return request({
        url: '/transaction-category/batch/',
        method: 'delete',
        data: {ids}
    })
}

// 查询所有交易分类
export function getTransactionCategoryAll(query) {
    return request({
        url: '/transaction-category/all',
        method: 'get',
        params: query
    })
}