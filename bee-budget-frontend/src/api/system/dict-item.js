import request from '@/utils/request.js'

// 查询字典项列表
export function getDictItemPaged(query) {
  return request({
    url: '/sys-dict-item/paged',
    method: 'get',
    params: query
  })
}

// 查询字典项详细
export function getDictItem(id) {
  return request({
    url: '/sys-dict-item/' + id,
    method: 'get'
  })
}

// 根据字典分类编码查询字典项信息
export function getDictItemAllByCategoryCode(categoryCode) {
  return request({
    url: '/sys-dict-item/by-category-code/' + categoryCode,
    method: 'get'
  })
}

// 新增字典项
export function createDictItem(data) {
  return request({
    url: '/sys-dict-item/',
    method: 'post',
    data: data
  })
}

// 修改字典项
export function updateDictItem(data) {
  return request({
    url: '/sys-dict-item/' + data.id,
    method: 'put',
    data: data
  })
}

// 删除字典项
export function deleteDictItem(id) {
  return request({
    url: '/sys-dict-item/' + id,
    method: 'delete'
  })
}

// 批量删除字典项
export function batchDeleteDictItem(ids) {
  return request({
    url: '/sys-dict-item/batch/',
    method: 'delete',
    data: { ids }
  })
}

// 修改字典项状态
export function changeDictItemStatus(id, status) {
    return request({
        url: '/sys-dict-item/change-status/' + id,
        method: 'put',
        data: { id, status }
    })
}