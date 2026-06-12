<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="100px" @submit.prevent="onSearch">
            <el-form-item label="应用名称" prop="name">
              <el-input
                  v-model="searchParams.name"
                  placeholder="请输入应用名称"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" icon="Search" native-type="submit">搜索</el-button>
              <el-button icon="Refresh" @click="onResetSearch">重置</el-button>
            </el-form-item>
          </el-form>
        </el-card>

        <el-card shadow="always" class="table">
          <div class="table-container">
            <div class="table-container__header">
              <el-row :gutter="12">
                <el-col :span="1.5">
                  <el-button type="primary" plain icon="Plus" @click="onAdd">新增
                  </el-button>
                </el-col>
                <el-col :span="1.5">
                  <el-button type="danger" plain icon="Delete" :disabled="hasNoSelection" @click="onDelete"
                  >批量删除
                  </el-button>
                </el-col>
              </el-row>
            </div>
            <div class="table-container__body" ref="tableRef">
              <el-table v-loading="tableLoading"
                        :data="list"
                        border
                        fit
                        style="width: 100%;"
                        :max-height="tableMaxHeight"
                        :default-sort="tableDefaultSort"
                        @selection-change="onSelectionChange"
                        @sort-change="onSortChange">
                <el-table-column type="selection" width="50" align="center"/>
                <el-table-column label="id" align="right" width="100" fixed="left" key="id" prop="id" sortable="custom"
                                 :sort-orders="['descending', 'ascending']"/>
                <el-table-column label="AppID" align="center" width="160" fixed="left" key="appId" prop="appId"/>
                <el-table-column label="应用名称" align="center" width="240" key="name"
                                 prop="name"/>
                <el-table-column label="应用描述" align="center" key="description"
                                 prop="description"/>
                <el-table-column label="应用图标" align="center" width="160" key="icon"
                                 prop="icon">
                  <template #default="scope">
                    <el-image
                        style="width: 100px; height: 100px"
                        :src="BASE_URL + scope.row.icon"
                        :zoom-rate="1.2"
                        :max-scale="7"
                        :min-scale="0.2"
                        :initial-index="0"
                        :preview-src-list="[BASE_URL + scope.row.icon]"
                        :preview-teleported="true"
                        fit="cover"
                    />
                  </template>
                </el-table-column>
                <el-table-column label="状态" align="center" prop="isEnabled" width="80">
                  <template #default="scope">
                    <el-tag
                        :type="scope.row.isEnabled ? 'success' : 'danger'"
                    >{{ scope.row.isEnabled ? "正常" : "禁用" }}
                    </el-tag>
                  </template>
                </el-table-column>
                <el-table-column label="创建时间" align="center" key="createdAt" prop="createdAt" sortable="custom"
                                 :sort-orders="['descending', 'ascending']">
                  <template #default="scope">
                    <span>{{
                        scope.row.createdAt && dayjs(scope.row.createdAt).format('YYYY-MM-DD HH:mm:ss')
                      }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="更新时间" align="center" key="updatedAt" prop="updatedAt" sortable="custom"
                                 :sort-orders="['descending', 'ascending']">
                  <template #default="scope">
                    <span>{{
                        scope.row.updatedAt && dayjs(scope.row.updatedAt).format('YYYY-MM-DD HH:mm:ss')
                      }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="操作" align="center" width="240" fixed="right">
                  <template #default="scope">
                    <el-button link type="primary" icon="Edit" @click="goToVersion(scope.row)">版本管理</el-button>

                    <el-button link type="primary" icon="Edit" @click="onUpdate(scope.row)"
                    >编辑
                    </el-button>
                    <el-button link type="primary" icon="Delete" @click="onDelete(scope.row)"
                    >删除
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
            </div>
            <div class="table-container__footer">
              <pagination
                  v-show="total > 0"
                  :total="total"
                  v-model:page="searchParams.pageNumber"
                  v-model:limit="searchParams.pageSize"
                  @pagination="loadList"
              />
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <el-dialog :title="dialogTitle" v-model="isDialogOpen" width="800px" append-to-body @close="closeDialog">
      <el-form :model="formData" :rules="formRules" ref="formRef" label-width="110">
        <el-form-item label="AppID" prop="appId">
          <el-input v-model="formData.appId" placeholder="请输入AppID" :disabled="!!formData.id"/>
        </el-form-item>
        <el-form-item label="应用名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入应用名称" :disabled="!!formData.id"/>
        </el-form-item>
        <el-form-item label="应用描述" prop="description">
          <el-input v-model="formData.description" placeholder="请输入应用名称"/>
        </el-form-item>
        <el-form-item label="应用图标" prop="icon">
          <el-upload
              class="avatar-uploader"
              ref="uploadCoverRef"
              :limit="1"
              accept=".jpg,.jpeg,.png"
              :headers="uploadCover.headers"
              :action="uploadCover.url"
              :disabled="uploadCover.isUploading"
              :on-progress="onCoverUploadProgress"
              :on-exceed="onCoverUploadExceed"
              :on-success="onCoverUploadSuccess"
              :show-file-list="false"
          >
            <img v-if="formData.icon" :src="BASE_URL + formData.icon" class="avatar"/>
            <el-icon v-else class="avatar-uploader-icon">
              <Plus/>
            </el-icon>
          </el-upload>
        </el-form-item>
        <el-form-item label="应用截图" prop="screenshot">
          <el-upload
              ref="uploadAlbumRef"
              multiple
              :limit="9"
              accept=".jpg,.jpeg,.png"
              :headers="uploadAlbum.headers"
              :action="uploadAlbum.url"
              :disabled="uploadAlbum.isUploading"
              :on-progress="onAlbumUploadProgress"
              :on-success="onAlbumUploadSuccess"
              list-type="picture-card"
              :file-list="formData.screenshot"
          >
            <template #tip>
              <div class="el-upload__tip">
                最多9张图片
              </div>
            </template>
            <el-icon>
              <Plus/>
            </el-icon>
            <template #file="{ file }">
              <div>
                <img class="el-upload-list__item-thumbnail" :src="BASE_URL + file.url" alt=""/>
                <div class="el-upload-list__item-actions">
                      <span class="el-upload-list__item-preview" @click="onAlbumUploadPreview(file)">
                        <el-icon><zoom-in/></el-icon>
                      </span>
                  <span class="el-upload-list__item-delete" @click="onAlbumUploadRemove(file)">
                        <el-icon><Delete/></el-icon>
                      </span>
                </div>
              </div>
            </template>
          </el-upload>
        </el-form-item>
        <el-form-item label="web链接地址" prop="h5Url" label-width="100">
          <el-input v-model="formData.h5Url" placeholder="请输入web链接地址"/>
        </el-form-item>
        <el-form-item label="禁用" prop="isEnabled">
          <el-switch size="large" v-model="formData.isEnabled" class="drawer-switch" :active-value="false"
                     :inactive-value="true"/>
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button type="primary" @click="onSubmitForm" :loading="submitDisabled">确 定</el-button>
          <el-button @click="onCancel">取 消</el-button>
        </div>
      </template>
    </el-dialog>

    <el-dialog v-model="isPreviewDialogOpen" width="1200px" append-to-body>
      <el-image :src="previewImageUrl" fit="fill"/>
    </el-dialog>

  </div>
</template>
<script setup name="AppManage">
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import _ from "lodash";

const router = useRouter()
import {ElMessage, ElMessageBox, genFileId} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import dayjs from 'dayjs'

import {
  getAppPaged,
  getApp,
  createApp,
  updateApp,
  deleteApp,
  batchDeleteApp,
} from "@/api/app"

// 表格相关
const tableRef = ref(null)
const tableMaxHeight = ref(0)
const tableDefaultSort = ref({prop: "createdAt", order: "descending"})
const tableLoading = ref(true)

// 表格数据相关
const list = ref([])
const total = ref(0)
const selectedIds = ref([])
const hasNoSelection = ref(true)
const searchRef = ref(null)
const searchParams = ref({
  pageNumber: 1,
  pageSize: 20,
  orderByField: null,
  orderByType: null,
  name: null,
  status: null,
})

// 表单相关
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  appId: [{required: true, message: "AppId不能为空", trigger: "blur"}],
  name: [{required: true, message: "应用名称不能为空", trigger: "blur"}],
  description: [{required: true, message: "应用描述不能为空", trigger: "blur"}],
})
const formSubmitting = ref(false)
const submitDisabled = computed(() => {
  if (uploadCover.isUploading || uploadAlbum.isUploading || formSubmitting.value) {
    return true
  }
  return false
})

// 字典相关
const {sys_normal_disable} = useDict('sys_normal_disable')

// 上传相关
import {BASE_URL, BASE_API_URL} from '@/utils/constants.js'

const uploadPrefix = ref(BASE_API_URL)
const uploadCoverRef = ref(null)
const uploadCover = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-image"
})
const uploadAlbumRef = ref(null)
const uploadAlbum = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-image"
})
const previewImageUrl = ref('')
const isPreviewDialogOpen = ref(false)

/* 加载列表 */
async function loadList() {
  tableLoading.value = true
  try {
    const response = await getAppPaged(searchParams.value)
    const {success, data, message} = response
    if (success) {
      list.value = data.items
      total.value = data.totalItems
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
    tableLoading.value = false
  }
}

/* 搜索 */
function onSearch() {
  searchParams.value.pageNumber = 1
  loadList()
}

/* 重置搜索 */
function onResetSearch() {
  searchRef.value?.resetFields()
  onSearch()
}

/* 删除 */
async function onDelete(row) {
  const delIds = row.id ? [row.id] : selectedIds.value
  try {
    await ElMessageBox.confirm('是否确认删除编号为 ' + delIds.join(', ') + ' 的数据项？')

    let response
    if (delIds.length > 1) {
      response = await batchDeleteApp(delIds)
    } else {
      response = await deleteApp(delIds[0])
    }

    // 接口成功但业务失败
    const {success, data, message} = response
    if (!success) {
      ElMessage.error(message)
      return
    }

    // 删除成功
    await loadList()
    ElMessage.success('删除成功')
  } catch (error) {
    // 用户点击“取消”或校验失败：静默处理
    if (error === 'cancel' || error === undefined) {
      return
    }

    // 其他异常（如网络错误、接口异常等）
    console.error('删除失败:', error)
    // ElMessage.error('删除失败，请重试')
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 多选框选中数据  */
function onSelectionChange(selection) {
  selectedIds.value = selection.map(item => item.id)
  hasNoSelection.value = !selection.length
}

/* 排序 */
function onSortChange({column, prop, order}) {
  searchParams.value.orderByField = prop
  searchParams.value.orderByType = order === 'ascending' ? 'ASC' : 'DESC'
  loadList()
}

/* 重置表单  */
function reset() {
  formData.value = {
    id: null,
    appId: null,
    name: null,
    description: null,
    icon: null,
    screenshot: [],
    h5Url: null,
    isEnabled: null
  };
  formRef.value?.resetFields()
}

/* 取消表单 */
function onCancel() {
  isDialogOpen.value = false
  reset()
}

/* 新增 */
function onAdd() {
  reset()
  isDialogOpen.value = true
  dialogTitle.value = '添加'
}

/* 修改 */
async function onUpdate(row) {
  reset()
  const appId = row.id
  try {
    const response = await getApp(appId)
    const {success, data, message} = response
    if (success) {
      formData.value = data
      formData.value.screenshot = data.screenshot?.length ? data.screenshot.split(',').map(o => {
        return {name: o, url: o}
      }) : []
      dialogTitle.value = '修改'
      isDialogOpen.value = true
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 提交 */
function onSubmitForm() {
  formRef.value?.validate(async valid => {
    if (valid) {
      formSubmitting.value = true
      try {
        const formDataClone = _.cloneDeep(formData.value)
        formDataClone.screenshot = formData.value.screenshot?.length ? formData.value.screenshot.map(o => o.url).join(',') : null
        const response = await (formDataClone.id !== null
            ? updateApp(formDataClone)
            : createApp(formDataClone))
        const {success, data, message} = response
        if (success) {
          ElMessage.success('操作成功')
          isDialogOpen.value = false
          await loadList()
        } else {
          ElMessage.error(message)
        }
      } catch (error) {
        // 可选：处理错误，比如提示用户
        console.error('提交失败:', error)
      } finally {
        // 无论成功或失败都会执行，如 loading 必须结束
        formSubmitting.value = false
      }
    }
  })
}

/* 跳转版本管理 */
function goToVersion(row) {
  router.push({path: '/app/version', query: {appId: row.appId}})
}

/* 关闭模态框 */
const closeDialog = () => {
  uploadCoverRef.value.clearFiles()
}

/* 封面文件上传中处理 */
const onCoverUploadProgress = (evt, uploadFile, uploadFiles) => {
  uploadCover.isUploading = true
}

/* 封面覆盖前一个 */
const onCoverUploadExceed = (files, uploadFiles) => {
  uploadCoverRef.value.clearFiles()
  const file = files[0]
  file.uid = genFileId()
  uploadCoverRef.value.handleStart(file)
  uploadCoverRef.value.submit()
}

/* 封面上传成功处理 */
const onCoverUploadSuccess = (response, uploadFile, uploadFiles) => {
  uploadCover.isUploading = false
  const {code, success, data, message} = response
  if (code === 200 && success) {
    formData.value.icon = data
    // ElMessageBox.alert("上传文件成功！")
  } else {
    ElMessageBox.alert(message)
  }
}

/* 封面上传失败处理 */
const onCoverUploadError = (error, uploadFile, uploadFiles) => {
  console.log(error, uploadFile, uploadFiles)
  uploadCover.isUploading = false
}

/* 相册上传中处理 */
const onAlbumUploadProgress = (evt, uploadFile, uploadFiles) => {
  uploadAlbum.isUploading = true
}

/* 相册上传成功处理 */
const onAlbumUploadSuccess = (response, uploadFile, uploadFiles) => {
  console.log(uploadFile, uploadFiles)
  uploadAlbum.isUploading = false
  const {code, success, data, message} = response
  if (code === 200 && success) {
    // formData.value.file_path = data.file_path
    uploadFile.url = data
    formData.value.screenshot.push({name: uploadFile.name, url: data})
  } else {
    ElMessageBox.alert(message)
  }
}

/* 相册上传失败处理 */
const onAlbumUploadError = (error, uploadFile, uploadFiles) => {
  console.log(error, uploadFile, uploadFiles)
  uploadAlbum.isUploading = false
}

/* 预览相册图片 */
function onAlbumUploadPreview(uploadFile) {
  previewImageUrl.value = BASE_URL + uploadFile.url
  isPreviewDialogOpen.value = true
}

/* 删除相册图片 */
function onAlbumUploadRemove(uploadFile) {
  const index = formData.value.screenshot.findIndex(item => item.uid === uploadFile.uid || item.name === uploadFile.name)
  if (index !== -1) {
    formData.value.screenshot.splice(index, 1)
  }
}

/* 设置表格最大高度 */
function updateTableMaxHeight() {
  tableMaxHeight.value = tableRef.value.clientHeight
}

loadList()

onMounted(async () => {
  await nextTick(() => {
    updateTableMaxHeight()
  })
  window.addEventListener('resize', () => {
    updateTableMaxHeight()
  })
})
</script>

<style lang='scss' scoped>
.app-container {
  height: 100%;

  .layout {
    height: 100%;
    display: flex;
  }

}

.layout-left {
  width: 300px;
  height: 100%;

  .el-card {
    height: 100%;
  }
}

.layout-center {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding: 0 12px;

}

.layout-right {
  width: 70px;

  :deep(.el-card__body) {
    padding: 0 !important;
  }

  .menu {
    margin: 0;
    padding: 0;
    list-style: none;
    font-size: 12px;

    &__item {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      height: 70px;
      border-bottom: 1px solid var(--el-border-color-light);
      color: var(--el-text-color-regular);

      &:hover {
        background-color: var(--el-color-primary);
        color: #ffffff;
      }
    }

    &__name {
      padding-top: 6px;
    }

    &__item--active {
      background-color: var(--el-color-primary);
      color: #ffffff;
    }
  }
}

.search {
  margin-bottom: 12px;

  :deep(.el-card__body) {
    padding-bottom: 0 !important;
  }

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  &__footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid var(--el-border-color-light);
    margin-top: 10px;
    padding-top: 8px;
  }

  .el-link {
    margin-left: 24px;
  }

  .el-button {
    margin-left: 24px;
  }
}

.table {
  flex: 1;
  overflow: auto;
  display: flex;
  flex-direction: column;

  :deep(.el-card__body) {
    flex: 1;
    display: flex;
    flex-direction: column;
    //overflow: hidden;
    //flex-wrap: nowrap;
  }

  .el-row {
    //width: 100%;
  }
}

.table-container {
  flex: 1;
  overflow: hidden;
  //height: 100%;
  display: flex;
  flex-direction: column;

  &__body {
    flex: 1;
    overflow: hidden;
    margin-top: 8px;
  }

  &__footer {
    height: 44px;

    .pagination-container {
      display: flex;
      justify-content: flex-end;
      margin-top: 12px;
      margin-bottom: 0;
      padding: 0 !important;
      height: auto;

      :deep(.el-pagination) {
        position: relative;
      }
    }
  }
}

</style>
<style lang="scss" scoped>
.avatar-uploader .avatar {
  width: 178px;
  //height: 178px;
  height: auto;
  display: block;
}
</style>
<style lang="scss">
.avatar-uploader .el-upload {
  border: 1px dashed var(--el-border-color);
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: var(--el-transition-duration-fast);
}

.avatar-uploader .el-upload:hover {
  border-color: var(--el-color-primary);
}

.el-icon.avatar-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  width: 178px;
  height: 178px;
  text-align: center;
}
</style>

