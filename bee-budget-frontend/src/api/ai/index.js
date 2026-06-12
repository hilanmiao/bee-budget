import request from '@/utils/request'

export function ask(data) {
    return request({
        url: 'http://192.168.0.99:5188/api/smart/ask-sse',
        method: 'post',
        data
    })
}
