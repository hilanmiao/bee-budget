import request from '@/utils/request.js'

// 查询列表
export function getDemo1Paged(query) {
    return request({
        url: '/demo1/paged',
        method: 'get',
        params: query
    })
}

// 查询详细
export function getDemo1(id) {
    return request({
        url: '/demo1/' + id,
        method: 'get'
    })
}

// 新增
export function createDemo1(data) {
    return request({
        url: '/demo1/',
        method: 'post',
        data: data
    })
}

// 修改
export function updateDemo1(data) {
    return request({
        url: '/demo1/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除
export function deleteDemo1(id) {
    return request({
        url: '/demo1/' + id,
        method: 'delete'
    })
}

// 批量删除
export function batchDeleteDemo1(ids) {
    return request({
        url: '/demo1/batch/',
        method: 'delete',
        data: {ids}
    })
}

// 查询所有
export function getDemo1All(query) {
    return request({
        url: '/demo1/all',
        method: 'get',
        params: query
    })
}

// 修改状态
export function changeDemo1Status(id, status) {
    return request({
        url: '/demo1/change-status/' + id,
        method: 'put',
        data: {id, status}
    })
}