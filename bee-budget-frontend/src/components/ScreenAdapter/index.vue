<template>
  <div
      class="ScreenAdapter"
      :style="style"
  >
    <slot/>
  </div>
</template>
<script setup>
const props = defineProps({
  width: {
    type: [Number, String],
    default: '1920'
  },
  height: {
    type: [Number, String],
    default: '1080'
  },
  boxWidth: {
    type: [Number, String],
  },
  boxHeight: {
    type: [Number, String],
  },
})

const style = ref({
  width: props.width + 'px',
  height: props.height + 'px',
  transform: 'scale(1) translate(-50%, -50%)'
})

watch(props, (newVal, oldVal) => {
  Debounce(setScale, 500)()
})

// 防抖函数
function Debounce(fn, t) {
  const delay = t || 500
  let timer
  return function () {
    const args = arguments
    if (timer) {
      clearTimeout(timer)
    }
    const context = this
    timer = setTimeout(() => {
      timer = null
      fn.apply(context, args)
    }, delay)
  }
}

// 获取放大缩小比例
function getScale() {
  let w, h
  if (props.boxWidth && props.boxHeight) {
    w = props.boxWidth / props.width
    h = props.boxHeight / props.height
  } else {
    w = window.innerWidth / props.width
    h = window.innerHeight / props.height
  }

  return w < h ? w : h
}

// 设置比例
function setScale() {
  style.value.transform = 'scale(' + getScale() + ') translate(-50%, -50%)'
}

onMounted(() => {
  // setScale()
  // window.onresize = Debounce(setScale, 1000)
})

</script>
<style lang="scss" scoped>
.ScreenAdapter {
  transform-origin: 0 0;
  position: absolute;
  left: 50%;
  top: 50%;
  transition: 0.3s;
  //background: #000;
}
</style>
