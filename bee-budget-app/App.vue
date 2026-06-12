<script>
// #ifdef APP-PLUS
import APPUpdate from '@/uni_modules/zhouWei-APPUpdate/js_sdk/appUpdate';
// #endif
export default {
  onLaunch(e) {
    // #ifdef APP-PLUS
    APPUpdate() //检测app更新
    // #endif
  },
  onShow(e) {

  },
  onHide() {
    console.log('App Hide')
  }
}
</script>

<style lang="scss">
/* #ifndef APP-NVUE */
@import "@/uni_modules/uview-ui/index.scss";
@import './style/iconfont.scss';
@import "./style/common.scss";

page {
  background-color: #f6f6f6;
}

// 隐藏滚动条
::-webkit-scrollbar {
  width: 0;
  height: 0;
  color: transparent;
}

/* #endif */
</style>
