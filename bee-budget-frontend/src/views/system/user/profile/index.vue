<template>
  <div class="app-container">
    <el-row :gutter="20">
      <el-col :span="6" :xs="24">
        <el-card class="box-card">
          <template v-slot:header>
            <div class="clearfix">
              <span>个人信息</span>
            </div>
          </template>
          <div>
            <div class="text-center">
              <userAvatar/>
            </div>
            <ul class="list-group list-group-striped">
              <li class="list-group-item">
                用户名称
                <div class="pull-right">{{ user.userName }}</div>
              </li>
              <li class="list-group-item">
                手机号码
                <div class="pull-right">{{ user.phoneNumber }}</div>
              </li>
              <li class="list-group-item">
                用户邮箱
                <div class="pull-right">{{ user.email }}</div>
              </li>
              <li class="list-group-item">
                所属角色
                <div class="pull-right">{{ user?.roles?.map(o => o.name).join(',') }}</div>
              </li>
              <li class="list-group-item">
                创建日期
                <div class="pull-right">{{ dayjs(user.createdAt).format('YYYY-MM-DD HH:mm:ss') }}</div>
              </li>
            </ul>
          </div>
        </el-card>
      </el-col>
      <el-col :span="18" :xs="24">
        <el-card>
          <template v-slot:header>
            <div class="clearfix">
              <span>基本资料</span>
            </div>
          </template>
          <el-tabs v-model="activeTab">
            <el-tab-pane label="基本资料" name="userinfo">
              <userInfo :user="user"/>
            </el-tab-pane>
            <el-tab-pane label="修改密码" name="resetPwd">
              <resetPwd/>
            </el-tab-pane>
          </el-tabs>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup name="Profile">
import userAvatar from "./user-avatar.vue"
import userInfo from "./user-info.vue"
import resetPwd from "./reset-password.vue"
import {getProfile} from "@/api/system/user-profile.js"
import {ElMessage} from "element-plus";
import dayjs from 'dayjs'

const activeTab = ref("userinfo")
const user = ref({})

async function _getProfile() {
  try {
    const response = await getProfile()
    const {success, data, message} = response
    if (success) {
      user.value = data
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载信息失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

_getProfile()
</script>
<style lang="scss" scoped>
.list-group-striped > .list-group-item {
  border-left: 0;
  border-right: 0;
  border-radius: 0;
  padding-left: 0;
  padding-right: 0;
}

.list-group {
  padding-left: 0px;
  list-style: none;
}

.list-group-item {
  border-bottom: 1px solid #e7eaec;
  border-top: 1px solid #e7eaec;
  margin-bottom: -1px;
  padding: 11px 0px;
  font-size: 13px;
}

.pull-right {
  float: right !important;
}

.text-center {
  text-align: center
}
</style>
