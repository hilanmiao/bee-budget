import request from '@/utils/request'

export function getAppAll(query) {
    return request({
        url: '/app/all',
        method: 'get',
        params: query
    })
}

export function getAppPaged(query) {
  return request({
    url: '/app/paged',
    method: 'get',
    params: query
  })
}

export function getApp(id) {
  return request({
    url: '/app/' + id,
    method: 'get',
  })
}

export function createApp(data) {
  return request({
    url: '/app',
    method: 'post',
    data: data
  })
}

export function updateApp(data) {
  return request({
    url: '/app/' + data.id,
    method: 'put',
    data: data
  })
}

export function deleteApp(id) {
  return request({
    url: '/app/' + id,
    method: 'delete',
  })
}

export function batchDeleteApp(ids) {
    return request({
        url: '/app/batch/',
        method: 'delete',
        data: { ids }
    })
}

