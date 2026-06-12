<template>
  <view class="page">
    <u-navbar
        title="交易分类管理"
        @leftClick="navigateBack"
        safeAreaInsetTop
        fixed
        placeholder
        :bgColor="primaryColor"
        leftIconColor="#fff"
        :titleStyle="{color: '#fff'}"
    ></u-navbar>

    <view class="main">
      <u-row justify="flex-start" gutter="10" class="category">
        <u-col span="3" class="category__item"
               v-for="item in list" :key="item.id" @click="goToDetail(item)">
          <view class="category__icon">
            <my-icon :name="item.icon" size="48" color="#999"></my-icon>
          </view>
          <view class="category__label">
            {{ item.name }}
          </view>
        </u-col>
      </u-row>
    </view>

    <view class="add" @click="goToCreate">
      <u-icon name="plus" size="32" color="#ffffff"></u-icon>
    </view>

  </view>
</template>

<script>
import {mapState} from 'vuex';
import {
  getTransactionCategoryAll,
} from '@/api/ledger';

export default {
  components: {},
  data() {
    return {
      list: [],
    }
  },
  computed: {
    ...mapState(['primaryColor']),
  },
  watch: {},
  async onLoad(e) {
    // 获取传递过来的参数
    // this.state = e.deviceState;

    await this._getTransactionTransactionCategoryAll()
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
    goToCreate() {
      const url = '/pages/transaction-category/create'

      this.onJump(url)
    },
    goToDetail(obj) {
      const params = `detail=${JSON.stringify(obj)}`
      const url = '/pages/transaction-category/detail?' + params
      
      this.onJump(url)
    },
    async _getTransactionTransactionCategoryAll() {
      try {
        const res = await getTransactionCategoryAll()
        console.log(res)
        this.list = res
      } catch (err) {
        console.log(err, 'catch')
      }
    },
  },
}
</script>

<style scoped lang="scss">
.page {
  background-color: #fff;
  height: 100vh;
  overflow: auto;
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

.main {
  padding: 36px 0 80px 0;
  overflow: hidden;
}

.category {
  padding: 0 16px 0 16px;
  flex-wrap: wrap;
  overflow: auto;

  &__item {
    display: flex;
    flex-direction: column;
    align-items: center !important;
    justify-content: center !important;
    padding-bottom: 16px;

    &:hover {
      .category__icon {
        background: rgba(100, 147, 237, .2);
        color: $theme-color-primary-brand;

        ::v-deep .custom-icon {
          color: $theme-color-primary-brand !important;
        }
      }
    }
  }

  &__icon {
    width: 40px;
    height: 40px;
    border-radius: 12px;
    //background: #f9f9f9;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #333;
  }

  &__label {
    padding-top: 10px;
    color: #666;
    font-size: 12px;
    text-align: center;
  }
}


</style>
