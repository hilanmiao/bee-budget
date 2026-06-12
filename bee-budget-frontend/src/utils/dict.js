import useDictStore from '@/store/modules/dict'
import { getDictItemAllByCategoryCode } from '@/api/system/dict-item.js'

/**
 * 获取字典项
 */
export function useDict(...args) {
  const res = ref({});
  return (() => {
    args.forEach((categoryCode, index) => {
      res.value[categoryCode] = [];
      const dicts = useDictStore().getDict(categoryCode);
      if (dicts) {
        res.value[categoryCode] = dicts;
      } else {
          getDictItemAllByCategoryCode(categoryCode).then(resp => {
          res.value[categoryCode] = resp.data.map(p => ({ label: p.label, value: p.value, elTagType: p.listClass, elTagClass: p.cssClass }))
          useDictStore().setDict(categoryCode, res.value[categoryCode]);
        })
      }
    })
    return toRefs(res.value);
  })()
}