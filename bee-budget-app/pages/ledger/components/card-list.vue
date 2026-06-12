<template>
  <view>
    <view class="item" v-for="(item,index) in list" :key="index"
          @click="goToDetail(item)">
      <view class="item__header">
        <view class="item__name" @click="goToDetail(item)">
          {{ item.name }}
        </view>
      </view>
      <view class="item__content" @click="goToDetail(item)">
        <u-row justify="space-between" gutter="10">
          <u-col span="12">
            <view>
              <text class="item__label">备注：</text>
              <text class="item__value"> {{ item.remark }}</text>
            </view>
          </u-col>
        </u-row>

        <view class="item__date">
          创建时间：{{ $dayjs(item.createdAt).format('YYYY-MM-DD HH:mm:ss') }}
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import {mapState, mapActions} from 'vuex';

export default {
  props: {
    list: {
      type: Array,
      default: []
    },
  },
  data() {
    return {}
  },
  computed: {
    ...mapState(['primaryColor']),
  },
  //第一次加载
  created(e) {
  },
  methods: {
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    goToDetail(obj) {
      const params = `detail=${JSON.stringify(obj)}`
      const url = '/pages/ledger/detail?' + params

      this.onJump(url)
    },
  }
}
</script>
<style lang="scss" scoped>
@import '@/style/device-card.scss';

.item {
  &__name {
    width: 80%;
  }

  &__desc {
    .u-row {
      flex-wrap: wrap;
    }
  }
}
</style>
