import request from '@/utils/request'

export function getLedgerAll(query) {
    return request({
        url: '/ledger/all',
        method: 'get',
        params: query
    })
}

export function getLedgerPaged(query) {
    return request({
        url: '/ledger/paged',
        method: 'get',
        params: query
    })
}

export function getLedger(id) {
    return request({
        url: '/ledger/' + id,
        method: 'get',
    })
}

export function createLedger(data) {
    return request({
        url: '/ledger',
        method: 'post',
        data: data
    })
}

export function updateLedger(data) {
    return request({
        url: '/ledger/' + data.id,
        method: 'put',
        data: data
    })
}

export function deleteLedger(id) {
    return request({
        url: '/ledger/' + id,
        method: 'delete',
    })
}

export function batchDeleteLedger(ids) {
    return request({
        url: '/ledger/batch/',
        method: 'delete',
        data: { ids }
    })
}

