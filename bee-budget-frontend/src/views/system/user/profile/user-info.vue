<template>
  <el-form ref="formRef" :model="formData" :formRules="formRules" label-width="80px">
    <el-form-item label="用户昵称" prop="nickName">
      <el-input v-model="formData.nickName" maxlength="30"/>
    </el-form-item>
    <el-form-item label="手机号码" prop="phoneNumber">
      <el-input v-model="formData.phoneNumber" maxlength="11"/>
    </el-form-item>
    <el-form-item label="邮箱" prop="email">
      <el-input v-model="formData.email" maxlength="50"/>
    </el-form-item>
    <el-form-item label="性别">
      <el-radio-group v-model="formData.sex">
        <el-radio value="0">男</el-radio>
        <el-radio value="1">女</el-radio>
      </el-radio-group>
    </el-form-item>
    <el-form-item>
      <el-button type="primary" @click="submit">保存</el-button>
      <el-button type="danger" @click="close">关闭</el-button>
    </el-form-item>
  </el-form>
</template>

<script setup>
import {updateProfile} from "@/api/system/user-profile.js"
import {ElMessage} from "element-plus";
import Tab from "@/plugins/tab.js";
import useUserStore from "@/store/modules/user.js";
const userStore = useUserStore()

const props = defineProps({
  user: {
    type: Object
  }
})

const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  nickName: [{required: true, message: "用户昵称不能为空", trigger: "blur"}],
  email: [{required: true, message: "邮箱地址不能为空", trigger: "blur"}, {
    type: "email",
    message: "请输入正确的邮箱地址",
    trigger: ["blur", "change"]
  }],
  phoneNumber: [{
    required: true,
    message: "手机号码不能为空",
    trigger: "blur"
  }, {pattern: /^1[3|4|5|6|7|8|9][0-9]\d{8}$/, message: "请输入正确的手机号码", trigger: "blur"}],
})

/** 提交按钮 */
function submit() {
  formRef.value?.validate(async valid => {
    if (valid) {
      try {
        const response = await updateProfile(formData.value)
        const {success, data, message} = response
        if (success) {
          ElMessage.success('操作成功')
        } else {
          ElMessage.error(message)
        }
      } catch (error) {
        // 可选：处理错误，比如提示用户
        console.error('提交失败:', error)
      } finally {
        // 无论成功或失败都会执行，如 loading 必须结束
      }
    }
  })
}

/** 关闭按钮 */
function close() {
  Tab.closePage()
}

// 回显当前登录用户信息
watch(() => props.user, user => {
  if (user) {
    formData.value = {nickName: user.nickName, phoneNumber: user.phoneNumber, email: user.email, sex: user.sex}
  }
}, {immediate: true})

</script>
