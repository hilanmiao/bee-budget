<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="68px" @submit.prevent="onSearch">
            <el-form-item label="角色名称" prop="name">
              <el-input
                  v-model="searchParams.name"
                  placeholder="请输入角色名称"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item label="权限字符" prop="key">
              <el-input
                  v-model="searchParams.key"
                  placeholder="请输入权限字符"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item label="状态" prop="status">
              <el-select
                  v-model="searchParams.status"
                  placeholder="角色状态"
                  clearable
                  style="width: 160px"
              >
                <el-option
                    v-for="dict in sys_normal_disable"
                    :key="dict.value"
                    :label="dict.label"
                    :value="dict.value"
                />
              </el-select>
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
                  <el-button type="primary" plain icon="Plus" @click="onAdd" v-hasPermi="['system:role:add']">新增
                  </el-button>
                </el-col>
                <el-col :span="1.5">
                  <el-button type="danger" plain icon="Delete" :disabled="hasNoSelection" @click="onDelete"
                             v-hasPermi="['system:role:remove']">批量删除
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
                <el-table-column label="角色名称" key="name" prop="name"/>
                <el-table-column label="权限字符" key="key" prop="key"/>
                <el-table-column label="显示顺序" key="sort" prop="sort" width="120"/>
                <el-table-column label="状态" align="center" key="status">
                  <template #default="scope">
                    <el-switch
                        v-if="scope.row.key !== 'admin'"
                        v-model="scope.row.status"
                        active-value="0"
                        inactive-value="1"
                        active-text="正常"
                        inactive-text="停用"
                        @change="onStatusChange(scope.row)"
                    ></el-switch>
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
                    <el-button link type="primary" icon="Edit" @click="onUpdate(scope.row)"
                               v-if="scope.row.key !== 'admin'">编辑
                    </el-button>
                    <el-button link type="primary" icon="Delete" @click="onDelete(scope.row)"
                               v-if="scope.row.key !== 'admin'">删除
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

    <el-dialog :title="dialogTitle" v-model="isDialogOpen" width="800px" append-to-body>
      <el-form :model="formData" :rules="formRules" ref="formRef" label-width="100">
        <el-form-item label="角色名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入角色名称"/>
        </el-form-item>
        <el-form-item prop="key">
          <template #label>
                  <span>
                     <el-tooltip content="控制器中定义的权限字符，如：@PreAuthorize(`@ss.hasRole('admin')`)"
                                 placement="top">
                        <el-icon><question-filled/></el-icon>
                     </el-tooltip>
                     权限字符
                  </span>
          </template>
          <el-input v-model="formData.key" placeholder="请输入权限字符"/>
        </el-form-item>
        <el-form-item label="角色顺序" prop="sort">
          <el-input-number v-model="formData.sort" controls-position="right" :min="0"/>
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="formData.status">
            <el-radio
                v-for="dict in sys_normal_disable"
                :key="dict.value"
                :value="dict.value"
            >{{ dict.label }}
            </el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="菜单权限">
          <el-checkbox v-model="menuExpand" @change="handleCheckedTreeExpand($event, 'menu')">展开/折叠</el-checkbox>
          <el-checkbox v-model="menuNodeAll" @change="handleCheckedTreeNodeAll($event, 'menu')">全选/全不选
          </el-checkbox>
          <el-checkbox v-model="formData.menuCheckStrictly" @change="handleCheckedTreeConnect($event, 'menu')">父子联动
          </el-checkbox>
          <el-tree
              class="tree-border"
              :data="menuOptions"
              show-checkbox
              ref="menuRef"
              node-key="id"
              :check-strictly="!formData.menuCheckStrictly"
              empty-text="加载中，请稍候"
              :props="{ label: 'label', children: 'children' }"
          ></el-tree>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="formData.remark" type="textarea" placeholder="请输入内容"></el-input>
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button type="primary" @click="onSubmitForm" :loading="isSubmitDisabled">确 定</el-button>
          <el-button @click="onCancel">取 消</el-button>
        </div>
      </template>
    </el-dialog>

  </div>
</template>

<script setup name="Role">
// --- 框架工具等相关 ---
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import {ElMessage, ElMessageBox} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import dayjs from 'dayjs'
import useUserStore from '@/store/modules/user'

const userStore = useUserStore();

// --- api 相关 ----
import {
  getRolePaged,
  getRole,
  createRole,
  updateRole,
  deleteRole,
  batchDeleteRole,
  changeRoleStatus,
} from "@/api/system/role"
import {getRoleMenuTreeSelect, getUserMenuTreeSelect} from "@/api/system/menu";
import {getDictItemPaged} from "@/api/system/dict-item.js";

// --- 表格相关 ---
const tableRef = ref(null)
const tableMaxHeight = ref(0)
const tableDefaultSort = ref({prop: "createdAt", order: "descending"})
const tableLoading = ref(true)

// --- 表格数据相关 ---
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

// --- 表单相关 ---
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  name: [{required: true, message: "角色名称不能为空", trigger: "blur"}],
  key: [{required: true, message: "权限字符不能为空", trigger: "blur"}],
  sort: [{required: true, message: "角色顺序不能为空", trigger: "blur"}]
})
const formSubmitting = ref(false)
const isSubmitDisabled = computed(() => {
  // if (uploadCover.isUploading || formSubmitting.value) {
  //   return true
  // }
  // return false
  return formSubmitting.value
})
const menuOptions = ref([]);
const menuExpand = ref(false);
const menuNodeAll = ref(false);
const menuRef = ref(null);

// --- 字典相关 ---
const {sys_normal_disable} = useDict('sys_normal_disable')

/* 加载列表 */
async function loadList() {
  tableLoading.value = true
  try {
    const response = await getRolePaged(searchParams.value)
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
      response = await batchDeleteRole(delIds)
    } else {
      response = await deleteRole(delIds[0])
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

/* 修改状态  */
async function onStatusChange(row) {
  const originalStatus = row.status === '0' ? '1' : '0'
  const text = row.status === '0' ? '启用' : '停用'

  try {
    await ElMessageBox.confirm(`确认要 ${text} ${row.name} 吗?`)
    const response = await changeRoleStatus(row.id, row.status)

    // 接口成功但业务失败
    const {success, data, message} = response
    if (!success) {
      ElMessage.error(message)
      row.status = originalStatus // 直接回滚，不依赖 catch
    }

    ElMessage.success('操作成功')
  } catch (error) {
    // 无论是用户取消，还是接口失败，都回滚状态
    row.status = originalStatus

    // 用户点击“取消”或校验失败：静默处理
    if (error === 'cancel' || error === undefined) {
      return
    }

    // 其他异常（如网络错误、接口异常等）
    console.error('状态切换异常:', error)
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
  if (menuRef.value !== null) {
    menuRef.value.setCheckedKeys([]);
  }
  menuExpand.value = false;
  menuNodeAll.value = false;
  formData.value = {
    id: null,
    name: null,
    key: null,
    sort: 0,
    status: "0",
    menuIds: [],
    menuCheckStrictly: true,
    remark: null
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
  loadUserMenuTreeSelect();
  isDialogOpen.value = true
  dialogTitle.value = '添加'
}

/* 修改 */
async function onUpdate(row) {
  reset()
  try {
    const roleMenu = loadRoleMenuTreeSelect(row.id);
    const response = await getRole(row.id)
    const {success, data, message} = response
    if (success) {
      formData.value = data
      dialogTitle.value = '修改'
      isDialogOpen.value = true

      await nextTick(() => {
        roleMenu.then((res) => {
          let checkedKeys = res.data.checkedKeys;
          checkedKeys.forEach((v) => {
            nextTick(() => {
              menuRef.value.setChecked(v, true, false);
            });
          });
        });
      });
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
        formData.value.menuIds = getMenuAllCheckedKeys();
        const response = await (formData.value.id !== null
            ? updateRole(formData.value)
            : createRole(formData.value))
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

/* 查询菜单树结构 */
function loadUserMenuTreeSelect() {
  getUserMenuTreeSelect(userStore.id).then(response => {
    menuOptions.value = response.data;
  });
}

/* 根据角色ID查询菜单树结构 */
function loadRoleMenuTreeSelect(roleId) {
  return getRoleMenuTreeSelect(roleId).then(response => {
    menuOptions.value = response.data.menus;
    return response;
  });
}

/* 树权限（展开/折叠）*/
function handleCheckedTreeExpand(value, type) {
  if (type == "menu") {
    let treeList = menuOptions.value;
    for (let i = 0; i < treeList.length; i++) {
      menuRef.value.store.nodesMap[treeList[i].id].expanded = value;
    }
  }
}

/* 树权限（全选/全不选） */
function handleCheckedTreeNodeAll(value, type) {
  if (type == "menu") {
    menuRef.value.setCheckedNodes(value ? menuOptions.value : []);
  }
}

/* 树权限（父子联动） */
function handleCheckedTreeConnect(value, type) {
  if (type == "menu") {
    formData.value.menuCheckStrictly = value ? true : false;
  }
}

/* 所有菜单节点数据 */
function getMenuAllCheckedKeys() {
  // 目前被选中的菜单节点
  let checkedKeys = menuRef.value.getCheckedKeys();
  // 半选中的菜单节点
  let halfCheckedKeys = menuRef.value.getHalfCheckedKeys();
  checkedKeys.unshift.apply(checkedKeys, halfCheckedKeys);
  return checkedKeys;
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

.tree-border {
  margin-top: 5px;
  border: 1px solid var(--el-border-color-light, #e5e6e7);
  background: var(--el-bg-color, #FFFFFF) none;
  border-radius: 4px;
  width: 100%;
}

</style>

