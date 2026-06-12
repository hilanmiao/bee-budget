<template>
  <view>
    <view class="item" v-for="(item,index) in list" :key="index"
          @click="goToDetail(item)">
      <view class="item__header">
        <view class="item__name" @click="goToDetail(item)">
          {{ item.name }}
        </view>
        <!--        <view class="item__collection" @click.stop="handleCollect(item.id)">-->
        <!--          <u-icon :name="wasCollected(item.id) ? 'star-fill' : 'star'" size="24" :color="primaryColor"></u-icon>-->
        <!--        </view>-->
      </view>
      <view class="item__content" @click="goToDetail(item)">
        <u-row justify="space-between" gutter="10">
          <u-col span="6">
            <view>
              <text class="item__label">能量阀：</text>
              <text class="item__value"> {{ getName(item.energy_valve_id) }}</text>
            </view>
          </u-col>
          <u-col span="6">
            <view>
              <text class="item__label">暂停状态：</text>
              <text class="item__value">
                {{ item.suspend ? '暂停' : '正常' }}
              </text>
            </view>
          </u-col>
          <u-col span="6">
            <view>
              <text class="item__label">控制模式：</text>
              <text class="item__value">{{ item.control_type }}</text>
            </view>
          </u-col>
          <u-col span="6">
            <view>
              <text class="item__label">目标值：</text>
              <text class="item__value">{{ item.control_value }}</text>
            </view>
          </u-col>
        </u-row>

        <view class="item__date">
          每天 {{ ('0' + item.hour).slice(-2) }}:{{ ('0' + item.minute).slice(-2) }}
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
    energyValveAll: {
      type: Array,
      default: []
    },
  },
  data() {
    return {}
  },
  computed: {
    ...mapState(['primaryColor', 'collectedList']),
  },
  //第一次加载
  created(e) {
  },
  methods: {
    ...mapActions(['collect', 'cancelCollect']),
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    goToDetail(obj) {
      const params = `detail=${JSON.stringify(obj)}`
      const url = '/pages/category/detail?' + params

      this.onJump(url)
    },
    getName(id) {
      const findObj = this.energyValveAll.find(o => o.id === id)
      if (findObj) {
        return findObj.name
      }
      return ''
    }
  }
}
</script>
<style lang="scss" scoped>
@import '@/style/device-card.scss';
@import '@/style/uni-table.scss';

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
