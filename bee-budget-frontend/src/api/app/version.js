import request from '@/utils/request'
import {getAppPaged} from "@/api/app/index.js";

export function getAppVersionAll(query) {
    return request({
        url: '/app-version/all',
        method: 'get',
        params: query
    })
}

export function getAppVersionPaged(query) {
  return request({
    url: '/app-version/paged',
    method: 'get',
    params: query
  })
}

export function getAppVersion(id) {
  return request({
    url: '/app-version/' + id,
    method: 'get',
  })
}

export function createAppVersion(data) {
  return request({
    url: '/app-version',
    method: 'post',
    data: data
  })
}

export function updateAppVersion(data) {
  return request({
    url: '/app-version/' + data.id,
    method: 'put',
    data: data
  })
}

export function deleteAppVersion(id) {
  return request({
    url: '/app-version/' + id,
    method: 'delete',
  })
}

export function batchDeleteAppVersion(ids) {
    return request({
        url: '/app-version/batch/',
        method: 'delete',
        data: {ids}
    })
}

export function getAppMaxVersion(id) {
  return request({
    url: '/app-version/max-version/' + id,
    method: 'get',
  })
}
