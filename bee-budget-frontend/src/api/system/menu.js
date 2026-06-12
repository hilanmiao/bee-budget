import request from '@/utils/request'

// 获取路由
export const getRouters = (userId) => {
    return request({
        url: '/sys-menu/router-data/by-user-id/' + userId,
        method: 'get'
    })
}

// 查询菜单列表
export function getMenuAll(query) {
  return request({
    url: '/sys-menu/all',
    method: 'get',
    params: query
  })
}

// 查询菜单详细
export function getMenu(id) {
  return request({
    url: '/sys-menu/' + id,
    method: 'get'
  })
}

// 查询用户拥有的菜单树形选择控件数据
export function getUserMenuTreeSelect(userId) {
  return request({
    url: '/sys-menu/tree-select-data/by-user-id/' + userId,
    method: 'get'
  })
}

// 查询角色拥有的菜单树形选择控件数据
export function getRoleMenuTreeSelect(roleId) {
  return request({
    url: '/sys-menu/tree-select-data/by-role-id/' + roleId,
    method: 'get'
  })
}

// 新增菜单
export function createMenu(data) {
  return request({
    url: '/sys-menu',
    method: 'post',
    data: data
  })
}

// 修改菜单
export function updateMenu(data) {
  return request({
    url: '/sys-menu/' + data.id,
    method: 'put',
    data: data
  })
}

// 删除菜单
export function deleteMenu(id) {
  return request({
    url: '/sys-menu/' + id,
    method: 'delete'
  })
}