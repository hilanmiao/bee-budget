import request from '@/utils/request'
import {getDictItemPaged} from "@/api/system/dict-item.js";

// 查询所有用户列表
export function getUserAll(query) {
    return request({
        url: '/sys-user/all',
        method: 'get',
        params: query
    })
}

// 查询用户列表
export function getUserPaged(query) {
    return request({
        url: '/sys-user/paged',
        method: 'get',
        params: query
    })
}

// 查询用户详细
export function getUser(id) {
    return request({
        url: '/sys-user/' + id,
        method: 'get'
    })
}

// 新增用户
export function createUser(data) {
    return request({
        url: '/sys-user',
        method: 'post',
        data: data
    })
}

// 修改用户
export function updateUser(data) {
    return request({
        url: '/sys-user/' + data.id,
        method: 'put',
        data: data
    })
}

// 删除用户
export function deleteUser(id) {
    return request({
        url: '/sys-user/' + id,
        method: 'delete'
    })
}

// 批量删除用户
export function batchDeleteUser(ids) {
    return request({
        url: '/sys-user/batch/',
        method: 'delete',
        data: {ids}
    })
}

// 重置用户密码
export function resetUserPassword(id, password) {
    return request({
        url: '/sys-user/reset-password/' + id,
        method: 'put',
        data: {password}
    })
}

// 用户状态修改
export function changeUserStatus(id, status) {
    return request({
        url: '/sys-user/change-status/' + id,
        method: 'put',
        data: {status}
    })
}
