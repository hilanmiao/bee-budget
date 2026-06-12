import request from '@/utils/request.js'
import {getAppPaged} from "@/api/app/index.js";

// 查询产品列表
export function getProductionPaged(query) {
    return request({
        url: '/production/paged',
        method: 'get',
        params: query
    })
}

// 查询产品详细
export function getProduction(id) {
    return request({
        url: '/production/' + id,
        method: 'get'
    })
}

// 新增产品
export function createProduction(data) {
    return request({
        url: '/production/',
        method: 'post',
        data: data
    })
}

// 修改产品
export function updateProduction(data) {
    return request({
        url: '/production/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除产品
export function deleteProduction(id) {
    return request({
        url: '/production/' + id,
        method: 'delete'
    })
}

// 删除产品
export function batchDeleteProduction(ids) {
    return request({
        url: '/production/batch/',
        method: 'delete',
        data: {ids}
    })
}

export function getProductionAll(query) {
    return request({
        url: '/production/all',
        method: 'get',
        params: query
    })
}

export function changeProductionStatus(id, status) {
    return request({
        url: '/production/change-status/' + id,
        method: 'put',
        data: {id, status}
    })
}