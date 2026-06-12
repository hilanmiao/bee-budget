import request from '@/utils/request.js'

// 查询字典分类列表
export function getDictCategoryPaged(query) {
    return request({
        url: '/sys-dict-category/paged',
        method: 'get',
        params: query
    })
}

// 查询字典分类详细
export function getDictCategory(id) {
    return request({
        url: '/sys-dict-category/' + id,
        method: 'get'
    })
}

// 新增字典分类
export function createDictCategory(data) {
    return request({
        url: '/sys-dict-category/',
        method: 'post',
        data: data
    })
}

// 修改字典分类
export function updateDictCategory(data) {
    return request({
        url: '/sys-dict-category/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除字典分类
export function deleteDictCategory(id) {
    return request({
        url: '/sys-dict-category/' + id,
        method: 'delete'
    })
}

// 批量删除字典分类
export function batchDeleteDictCategory(ids) {
    return request({
        url: '/sys-dict-category/batch/',
        method: 'delete',
        data: {ids}
    })
}

// 查询所有字典分类
export function getDictCategoryAll(query) {
    return request({
        url: '/sys-dict-category/all',
        method: 'get',
        params: query
    })
}

// 修改字典分类状态
export function changeDictCategoryStatus(id, status) {
    return request({
        url: '/sys-dict-category/change-status/' + id,
        method: 'put',
        data: {id, status}
    })
}