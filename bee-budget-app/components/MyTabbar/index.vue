<template>
      <u-tabbar
          :value="tabIndex"
          @change="onTabbar"
          fixed
          placeholder
          safeAreaInsetBottom
          :activeColor="activeColor || primaryColor"
          :inactiveColor="inactiveColor"
          :border="false"
      >
        <block v-for="(item,index) in list" :key="index">
          <!-- 自定义icon -->
          <u-tabbar-item :text="item.name" :badge="item.badge" :dot="item.dot" :badgeStyle="item.badgeStyle">
            <view slot="active-icon">
<!--              <view class="box-active">-->

<!--              </view>-->
              <view class="custom-icon" :class="['custom-icon-'+item.iconFill]" style="font-size: 20px;"
                    :style="{color:activeColor || primaryColor}"></view>
              <!-- 自定义图标 -->
              <!-- <MyIcon :name="item.iconFill" size="40" :color="activeColor || primaryColor"></MyIcon> -->
              <!-- 图片路径 -->
              <!-- <image class="icon" :src="item.iconFill"></image> -->
            </view>
            <view slot="inactive-icon">
              <view class="custom-icon" :class="['custom-icon-'+item.icon]" style="font-size: 20px;"
                    :style="{color:inactiveColor}"></view>
              <!-- 自定义图标 -->
<!--               <MyIcon :name="item.icon" size="40" :color="inactiveColor"></MyIcon>-->
              <!-- 图片路径 -->
              <!-- <image class="icon" :src="item.icon"></image> -->
            </view>
          </u-tabbar-item>
        </block>
      </u-tabbar>
</template>

<script>
import {mapState, mapMutations} from 'vuex';
import { systemInfo } from '@/config/index.js';

export default {
  name: 'f-tabbar',
  props: {
    // 是否固定在底部
    isFixed: {
      type: Boolean,
      default: true,
    },
    // 是否设置防止塌陷高度
    isFillHeight: {
      type: Boolean,
      default: true,
    },
    // 选中的颜色--为空显示主题色
    activeColor: {
      type: String,
      default: '',
    },
    // 未选中颜色
    inactiveColor: {
      type: String,
      default: '#7d7e80',
    },
    // 是否显示边框色
    border: {
      type: Boolean,
      default: function () {
        return false;
      }
    },
    // 右上角的角标提示信息
    badge: {
      type: [String, Number, null],
      default: uni.$u.props.tabbarItem.badge
    },
    // 是否显示圆点，将会覆盖badge参数
    dot: {
      type: Boolean,
      default: uni.$u.props.tabbarItem.dot
    },
    // 控制徽标的位置，对象或者字符串形式，可以设置top和right属性
    badgeStyle: {
      type: [Object, String],
      default: uni.$u.props.tabbarItem.badgeStyle
    }
  },
  computed: {
    ...mapState(['primaryColor']),
  },
  data() {
    return {
      safeAreaInsetBottom: false,
      systemInfo: systemInfo,
      tabIndex: 0,
      path: '', //当前路径
      list: [{
        name: '工作台',
        url: 'pages/home/index',
        icon: 'home',
        iconFill: 'home-fill'
        },
        {
        name: '统计',
        url: 'pages/statistic/index',
        icon: 'piechart',
        iconFill: 'piechart-circle-fil'
        },
        {
          name: '我的',
          url: 'pages/my/myInfo',
          icon: 'user',
          iconFill: 'user-fill',
          // dot: true
        }
        ]
    }
  },
  created() {
    //获取页面路径
    let currentPages = getCurrentPages();
    let page = currentPages[currentPages.length - 1];
    this.path = page.route;
    //获取页面路径
    this.list.forEach((item, index) => {
      if (this.path == item.url) {
        this.tabIndex = index
      }
    })
    // #ifdef H5
    this.safeAreaInsetBottom = true
    // #endif
  },
  methods: {
    onTabbar(index) {
      if (this.path !== this.list[index].url) {
        uni.switchTab({
          url: '/' + this.list[index].url
        });
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.u-tabbar {
  //box-shadow: 0 0 10px 0 hsla(0,6%,58%,.1);
  ::v-deep &__content {
    border-radius: 24px 24px 0 0;
    .box-active {
      background-color: red;
      width: 60px;
      height: 40px;
      position: absolute;
      top: -20px;
      left: 50%;
      border-radius: 100%;
      transform: translateX(-50%);
      &:before {
        content: '';
        width: 12px;
        height: 12px;
        background-color: blue;
        position: absolute;
        left: 0;
        top: 0;
      }
    }
  }
}
</style>
