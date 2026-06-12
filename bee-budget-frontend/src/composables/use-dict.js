import { ref, onMounted } from 'vue'
import { getDictsBatch } from '@/services/dict.js'

/**
 * Vue Composable：获取多个字典分类的响应式数据
 */
export function useDict(...categoryCodes) {
    const dictRefs = {}
    categoryCodes.forEach(categoryCode => {
        dictRefs[categoryCode] = ref([])
    })

    const loading = ref(false)

    const load = async () => {
        loading.value = true
        try {
            const data = await getDictsBatch(categoryCodes)
            categoryCodes.forEach(categoryCode => {
                dictRefs[categoryCode].value = data[categoryCode] || []
            })
        } finally {
            loading.value = false
        }
    }

    // 自动加载
    onMounted(() => {
        load()
    })

    return {
        ...dictRefs,
        loading: readonly(loading),
        reload: load
    }
}