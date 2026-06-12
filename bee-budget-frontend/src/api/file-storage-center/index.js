import request from '@/utils/request'
import {getAppPaged} from "@/api/app/index.js";

export function getFileStorageCenterAll(query) {
  return request({
    url: '/file-storage-center/all',
    method: 'get',
    params: query
  })
}

export function getFileStorageCenterPaged(query) {
  return request({
    url: '/file-storage-center/paged',
    method: 'get',
    params: query
  })
}

export function getFileStorageCenter(id) {
  return request({
    url: '/file-storage-center/' + id,
    method: 'get',
  })
}

export function createFileStorageCenter(data) {
  return request({
    url: '/file-storage-center',
    method: 'post',
    data: data
  })
}

export function updateFileStorageCenter(data) {
  return request({
    url: '/file-storage-center/' + data.id,
    method: 'put',
    data: data
  })
}

export function deleteFileStorageCenter(id) {
  return request({
    url: '/file-storage-center/' + id,
    method: 'delete',
  })
}

export function batchDeleteFileStorageCenter(ids) {
  return request({
    url: '/file-storage-center/batch/',
    method: 'delete',
    data: {ids}
  })
}
