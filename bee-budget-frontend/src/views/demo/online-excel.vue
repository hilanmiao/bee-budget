<template>
  <div ref="container" class="container"/>
</template>

<script setup name="OnlineExcel">
// 框架工具等相关
import {FUniver, Univer} from '@univerjs/presets'
import {UniverSheetsCorePreset} from '@univerjs/preset-sheets-core'
import UniverPresetSheetsCoreZhCN from '@univerjs/preset-sheets-core/locales/zh-CN'
import {createUniver, LocaleType, mergeLocales} from '@univerjs/presets'
import {onBeforeUnmount, onMounted, ref} from 'vue'

import '@univerjs/preset-sheets-core/lib/index.css'

const container = ref(null)

let univerInstance = null
let univerAPIInstance = null

onMounted(() => {
  console.log(999)
  const {univer, univerAPI} = createUniver({
    locale: LocaleType.ZH_CN,
    locales: {
      [LocaleType.ZH_CN]: mergeLocales(
          UniverPresetSheetsCoreZhCN,
      ),
    },
    presets: [
      UniverSheetsCorePreset({
        container: container.value,
      }),
    ],
  })

  univerAPI.createWorkbook({})

  univerInstance = univer
  univerAPIInstance = univerAPI
})

onBeforeUnmount(() => {
  univerInstance?.dispose()
  univerAPIInstance?.dispose()
  univerInstance = null
  univerAPIInstance = null
})
</script>

<style scoped lang="scss">
.container {
  width: 100%;
  height: 100%;
}
</style>