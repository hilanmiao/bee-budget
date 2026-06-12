import useDictStore from '@/store/modules/dict'
import { getDictItemAllByCategoryCode } from '@/api/system/dict-item.js'

/**
 * 获取单个字典分类的字典项（带缓存）
 * @param {string} categoryCode - 字典分类编码，如 'sys_normal_disable'
 * @returns {Promise<Array>} 字典项列表
 */
export async function getDict(categoryCode) {
    const store = useDictStore()

    // 1. 先查缓存
    const cached = store.getDict(categoryCode)
    if (cached && cached.length > 0) {
        return cached
    }

    // 2. 缓存未命中，请求 API
    try {
        const response = await getDictItemAllByCategoryCode(categoryCode)
        const data = response.data.map(p => ({
            label: p.label,
            value: p.value,
            elTagType: p.listClass,
            elTagClass: p.cssClass
        }))
        store.setDict(categoryCode, data)
        return data
    } catch (error) {
        console.error(`加载字典分类 ${categoryCode} 失败:`, error)
        return [] // 或抛出错误，根据需求
    }
}

/**
 * 批量获取多个字典分类的字典项
 * @param {string[]} categoryCodes - 字典分类编码数组
 * @returns {Promise<Object>} { categoryCode1: [...], categoryCode2: [...] }
 */
export async function getDictsBatch(categoryCodes) {
    const results = {}
    await Promise.all(
        categoryCodes.map(async (categoryCode) => {
            results[categoryCode] = await getDict(categoryCode)
        })
    )
    return results
}