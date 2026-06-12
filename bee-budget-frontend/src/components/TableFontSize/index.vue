<template>
  <el-radio-group v-model="tableFontSize">
    <el-radio-button label="small">小</el-radio-button>
    <el-radio-button label="medium">中</el-radio-button>
    <el-radio-button label="large">大</el-radio-button>
  </el-radio-group>
</template>

<script setup name="BusinessTree">
import {computed} from "vue";
const emits = defineEmits(['selected']);

const tableFontSize = ref('medium')
const tableFontSizeClass = computed(() => {
  let className
  if(tableFontSize.value === 'small') {
    className = 'table-font-size-small'
  } else if (tableFontSize.value === 'medium') {
    className = 'table-font-size-medium'
  } else {
    className = 'table-font-size-large'
  }

  return className
})

watch(tableFontSize, () => {
  emits('selected', tableFontSizeClass.value)
})



</script>

<style lang='scss' scoped>

</style>
