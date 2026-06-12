import request from '@/utils/request'
import {getDictItemPaged} from "@/api/system/dict-item.js";

// 查询角色列表
export function getRoleAll(query) {
    return request({
        url: '/sys-role/all',
        method: 'get',
        params: query
    })
}

// 查询角色列表
export function getRolePaged(query) {
    return request({
        url: '/sys-role/paged',
        method: 'get',
        params: query
    })
}

// 查询角色详细
export function getRole(id) {
    return request({
        url: '/sys-role/' + id,
        method: 'get'
    })
}

// 新增角色
export function createRole(data) {
    return request({
        url: '/sys-role',
        method: 'post',
        data: data
    })
}

// 修改角色
export function updateRole(data) {
    return request({
        url: '/sys-role/' + data.id,
        method: 'put',
        data: data
    })
}

// 角色状态修改
export function changeRoleStatus(id, status) {
    return request({
        url: '/sys-role/change-status/' + id,
        method: 'put',
        data: {status}
    })
}

// 删除角色
export function deleteRole(id) {
    return request({
        url: '/sys-role/' + id,
        method: 'delete'
    })
}

// 批量删除角色
export function batchDeleteRole(ids) {
    return request({
        url: '/sys-role/batch/',
        method: 'delete',
        data: {ids}
    })
}
