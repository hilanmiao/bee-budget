<template>
  <view class="page">
    <u-navbar
        title="账本管理"
        @leftClick="navigateBack"
        safeAreaInsetTop
        fixed
        placeholder
        :bgColor="primaryColor"
        leftIconColor="#fff"
        :titleStyle="{color: '#fff'}"
    ></u-navbar>

    <mescroll-body ref="mescrollRef" top="0px" @init="mescrollInit" :down="downOption"
                   @up="upCallback" @down="downCallback">
      <card-list :list="list"></card-list>
    </mescroll-body>

    <view class="add" @click="goToCreate">
      <u-icon name="plus" size="32" color="#ffffff"></u-icon>
    </view>

  </view>
</template>

<script>
import {mapState, mapActions} from 'vuex';
import MescrollMixin from "@/uni_modules/mescroll-uni/components/mescroll-uni/mescroll-mixins.js";
import cardList from "./components/card-list.vue";
import {getLedgerPaged} from '@/api/ledger';

export default {
  mixins: [MescrollMixin], // 使用mixin (在main.js注册全局组件)
  components: {
    cardList
  },
  data() {
    return {
      // mescroll滚动组件
      downOption: {
        auto: false //是否在初始化后,自动执行downCallback; 默认true
      },
      list: [],
    }
  },
  computed: {
    ...mapState(['primaryColor', 'userInfo']),
  },
  watch: {},
  async onLoad(e) {
    // 获取传递过来的参数
    // this.state = e.deviceState;
  },
  onShow() {
  },
  methods: {
    navigateBack() {
      // uni.navigateBack()
      uni.reLaunch({
        url: '/pages/my/myInfo'
      })
    },
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    /*上拉加载的回调: 其中page.num:当前页 从1开始, page.size:每页数据条数,默认10 */
    upCallback(page) {
      this.loadData(page.num)
    },
    //搜索框搜索
    onSearch() {
      this.list = []; // 先清空列表,显示加载进度
      this.$nextTick(() => {
        this.mescroll.scrollTo(0, 0);
        this.mescroll.resetUpScroll();
      })
    },
    goToCreate() {
      const url = '/pages/ledger/create'

      this.onJump(url)
    },
    async loadData(pageNo) {
      let httpData = {
        pageNumber: pageNo,
        pageSize: 10,
      }

      try {
        const response = await getLedgerPaged({params: httpData})
        const {success, data, message} = response
        if (success) {
          let {items, totalItems} = data
          const pageCount = parseInt(Math.ceil(totalItems / httpData.pageSize))

          uni.stopPullDownRefresh();
          this.mescroll.endByPage(items.length, pageCount); //必传参数(当前页的数据个数, 总页数)
          if (pageNo == 1) {
            this.list = items
          } else {
            this.list = this.list.concat(items);
          }
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
        //联网失败, 结束加载
        this.mescroll.endErr();
      }
    },
  },
}
</script>

<style scoped lang="scss">
.page {

}

.add {
  position: fixed;
  right: 24rpx;
  bottom: 48rpx;
  width: 64px;
  height: 64px;
  border-radius: 32px;
  background-color: $theme-color-primary-brand;
  box-shadow: $uni-box-shadow;
  display: flex;
  align-content: center;
  justify-content: center;
}

</style>
