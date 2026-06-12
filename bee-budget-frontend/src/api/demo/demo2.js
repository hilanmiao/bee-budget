import request from '@/utils/request.js'

// 查询列表
export function getDemo2Paged(query) {
    return request({
        url: '/demo2/paged',
        method: 'get',
        params: query
    })
}

// 查询详细
export function getDemo2(id) {
    return request({
        url: '/demo2/' + id,
        method: 'get'
    })
}

// 新增
export function createDemo2(data) {
    return request({
        url: '/demo2/',
        method: 'post',
        data: data
    })
}

// 修改
export function updateDemo2(data) {
    return request({
        url: '/demo2/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除
export function deleteDemo2(id) {
    return request({
        url: '/demo2/' + id,
        method: 'delete'
    })
}

// 批量删除
export function batchDeleteDemo2(ids) {
    return request({
        url: '/demo2/batch/',
        method: 'delete',
        data: {ids}
    })
}

// 查询所有
export function getDemo2All(query) {
    return request({
        url: '/demo2/all',
        method: 'get',
        params: query
    })
}

// 修改状态
export function changeDemo2Status(id, status) {
    return request({
        url: '/demo2/change-status/' + id,
        method: 'put',
        data: {id, status}
    })
}