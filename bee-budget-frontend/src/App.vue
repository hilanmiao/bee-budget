<template>
  <router-view />
</template>

<script setup>
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import useSettingsStore from '@/store/modules/settings'
import { handleThemeStyle } from '@/utils/theme'

onMounted(() => {
  nextTick(() => {
    // 初始化主题样式
    handleThemeStyle(useSettingsStore().theme)
  })
})
</script>
