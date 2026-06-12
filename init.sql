CREATE DATABASE IF NOT EXISTS bee_budget    DEFAULT CHARSET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS bee_budget_hangfire   DEFAULT CHARSET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS bee_budget_health_checks DEFAULT CHARSET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE bee_budget;

/*
 Navicat Premium Dump SQL

 Source Server         : mysql-localhost
 Source Server Type    : MySQL
 Source Server Version : 80041 (8.0.41)
 Source Host           : localhost:3306
 Source Schema         : bee_budget

 Target Server Type    : MySQL
 Target Server Version : 80041 (8.0.41)
 File Encoding         : 65001

 Date: 09/06/2026 09:17:16
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for app
-- ----------------------------
DROP TABLE IF EXISTS `app`;
CREATE TABLE `app`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符（主键）',
  `app_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '应用的唯一标识符，通常用于区分不同的App',
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '应用的名称',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '应用的描述信息',
  `icon` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '应用图标的URL地址',
  `screenshot` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '应用截图的URL地址',
  `h5_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '应用对应的H5页面的URL地址',
  `is_enabled` tinyint(1) NULL DEFAULT NULL COMMENT '标识该应用是否启用。1 表示启用，0 表示禁用',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = 'App应用表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of app
-- ----------------------------

-- ----------------------------
-- Table structure for app_version
-- ----------------------------
DROP TABLE IF EXISTS `app_version`;
CREATE TABLE `app_version`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符（主键）',
  `app_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '应用的唯一标识符，通常用于区分不同的App',
  `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '版本标题，通常用于描述该版本的主要内容或更新点',
  `contents` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '版本更新的内容说明，通常是详细的更新日志',
  `platform` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '平台信息，例如 \"Android\" 或 \"iOS\"，用于区分版本所属的操作系统',
  `version_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '版本名称，例如 \"1.0.0\"，用于人类可读的版本号',
  `version_code` int NULL DEFAULT NULL COMMENT '版本代码，通常是一个整数，用于标识版本的内部编号',
  `url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '版本下载地址的URL',
  `is_stable_publish` tinyint(1) NULL DEFAULT NULL COMMENT '标识该版本是否已稳定发布。1 表示已稳定发布，0 表示未稳定发布',
  `is_silently` tinyint(1) NULL DEFAULT NULL COMMENT '标识该版本是否为静默更新。1 表示静默更新，0 表示非静默更新',
  `is_mandatory` tinyint(1) NULL DEFAULT NULL COMMENT '标识该版本是否为强制更新。1 表示强制更新，0 表示非强制更新',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `IX_AppVersion_AppId_DeletedAt`(`app_id` ASC, `deleted_at` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = 'App应用版本表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of app_version
-- ----------------------------

-- ----------------------------
-- Table structure for demo1
-- ----------------------------
DROP TABLE IF EXISTS `demo1`;
CREATE TABLE `demo1`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符（主键）',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '名称',
  `sort` int NULL DEFAULT NULL COMMENT '显示顺序',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '状态。0 表示正常, 1 表示停用',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci COMMENT = '样例表1' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of demo1
-- ----------------------------
INSERT INTO `demo1` VALUES (1, 'test', 1, '1', '6666', '2026-03-04 01:56:40.000', 1, '2026-05-05 08:34:17.459', 1, NULL, NULL);
INSERT INTO `demo1` VALUES (2, 'test22222', 2, '0', '12', '2026-03-04 01:56:50.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `demo1` VALUES (3, '21212', 4, '0', '1133', '2026-03-04 01:59:21.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `demo1` VALUES (4, '133', 1, '0', '131331', '2026-03-04 01:59:34.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `demo1` VALUES (5, '212', 1, '0', '12', '2026-03-04 01:59:44.000', 1, NULL, NULL, NULL, NULL);

-- ----------------------------
-- Table structure for demo2
-- ----------------------------
DROP TABLE IF EXISTS `demo2`;
CREATE TABLE `demo2`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符（主键）',
  `demo1_id` bigint NULL DEFAULT NULL COMMENT 'Demo1ID',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '名称',
  `alias_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '别名',
  `code` int NULL DEFAULT NULL COMMENT '编码',
  `is_visible` tinyint(1) NULL DEFAULT NULL COMMENT '是否可见。true 表示可见，false 表示隐藏。',
  `sort` int NULL DEFAULT NULL COMMENT '显示顺序',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '状态。0 表示正常, 1 表示停用',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `IX_Demo2_Demo1Id_DeletedAt`(`demo1_id` ASC, `deleted_at` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci COMMENT = '样例表2' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of demo2
-- ----------------------------
INSERT INTO `demo2` VALUES (1, 4, '9922222222222', '99993333333333', 999, 0, 9991, '0', '99912121212', '2026-03-04 02:25:38.000', 1, '2026-03-04 02:28:49.000', 1, NULL, NULL);
INSERT INTO `demo2` VALUES (2, 5, '12', '99', 4, 1, 99, '0', '999', '2026-03-04 02:36:26.000', 1, '2026-03-04 02:36:46.000', 1, NULL, NULL);
INSERT INTO `demo2` VALUES (3, 5, '555', NULL, NULL, 0, 1, '1', NULL, '2026-03-04 02:36:59.000', 1, '2026-03-04 02:38:38.000', 1, NULL, NULL);
INSERT INTO `demo2` VALUES (4, 4, '6666', NULL, NULL, 1, 1, '0', NULL, '2026-03-04 02:37:05.000', 1, NULL, NULL, NULL, NULL);

-- ----------------------------
-- Table structure for file_storage_center
-- ----------------------------
DROP TABLE IF EXISTS `file_storage_center`;
CREATE TABLE `file_storage_center`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符',
  `name` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT '名称',
  `store_path` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL COMMENT '文件存储路径',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 21 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of file_storage_center
-- ----------------------------

-- ----------------------------
-- Table structure for ledger
-- ----------------------------
DROP TABLE IF EXISTS `ledger`;
CREATE TABLE `ledger`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符',
  `name` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT '名称',
  `user_id` bigint NULL DEFAULT NULL COMMENT '用户ID',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 11 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of ledger
-- ----------------------------
INSERT INTO `ledger` VALUES (4, '我的账本', 1, '', '2026-02-10 03:04:43.000', 1, '2026-02-10 03:04:58.000', 1, NULL, 1);
INSERT INTO `ledger` VALUES (7, '测试账本', 1, NULL, '2026-03-09 03:26:59.000', 1, '2026-05-05 08:17:05.317', 1, NULL, 1);
INSERT INTO `ledger` VALUES (8, '1234568989978978978798', 1, '3fffffff', '2026-04-02 03:22:31.519', 1, '2026-04-02 07:10:06.561', 1, '2026-04-03 02:26:51.715', 1);
INSERT INTO `ledger` VALUES (9, '1212222', 1, NULL, '2026-04-02 03:23:15.094', 1, NULL, NULL, '2026-04-02 03:25:24.310', 1);
INSERT INTO `ledger` VALUES (10, '1ff 发发发', 1, '发反反复复反反复复反反复复反反复复', '2026-04-02 06:59:57.797', 1, NULL, NULL, '2026-04-02 07:00:03.383', 1);

-- ----------------------------
-- Table structure for sys_dict_category
-- ----------------------------
DROP TABLE IF EXISTS `sys_dict_category`;
CREATE TABLE `sys_dict_category`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '字典主键',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '字典分类名称',
  `code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '字典分类编码',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '状态（0正常 1停用）',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 121 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '字典分类表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_dict_category
-- ----------------------------
INSERT INTO `sys_dict_category` VALUES (2, '菜单状态', 'sys_show_hide', '0', '菜单状态列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (3, '系统开关', 'sys_normal_disable', '0', '系统开关列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (4, '任务状态', 'sys_job_status', '0', '任务状态列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (5, '任务分组', 'sys_job_group', '0', '任务分组列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (6, '系统是否', 'sys_yes_no', '0', '系统是否列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (7, '通知类型', 'sys_notice_type', '0', '通知类型列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (8, '通知状态', 'sys_notice_status', '0', '通知状态列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (9, '操作类型', 'sys_oper_type', '0', '操作类型列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (10, '系统状态', 'sys_common_status', '0', '登录状态列表', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (101, '布尔值', 'sys_boolean', '0', NULL, '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_category` VALUES (116, '19', '19', '0', '999', '2025-01-01 00:00:00.000', 1, '2025-10-24 08:18:27.000', 1, '2025-10-24 08:19:28.000', 1);
INSERT INTO `sys_dict_category` VALUES (117, '88', '898', '0', '98', '2025-01-01 00:00:00.000', 1, '2025-10-24 08:19:34.000', 1, '2025-10-24 08:20:42.000', 1);
INSERT INTO `sys_dict_category` VALUES (118, '888', '9888998', '0', NULL, '2025-01-01 00:00:00.000', 1, '2025-10-24 08:19:38.000', 1, '2025-10-24 08:20:42.000', 1);
INSERT INTO `sys_dict_category` VALUES (119, '7778', '877878', '1', '877878', '2025-01-01 00:00:00.000', 1, '2025-10-27 07:52:56.000', 1, '2025-10-27 08:29:12.000', 1);
INSERT INTO `sys_dict_category` VALUES (120, '交易状态', 'transaction_status', '0', NULL, '2026-03-07 07:54:32.000', 1, '2026-03-07 07:54:32.000', 1, NULL, NULL);

-- ----------------------------
-- Table structure for sys_dict_item
-- ----------------------------
DROP TABLE IF EXISTS `sys_dict_item`;
CREATE TABLE `sys_dict_item`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '字典编码',
  `category_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '字典分类编码',
  `label` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '字典项标签',
  `value` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '字典项实际值',
  `css_class` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '样式属性（其他样式扩展）',
  `list_class` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '表格回显样式',
  `is_default` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT 'N' COMMENT '是否默认（Y是 N否）',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '状态（0正常 1停用）',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `IX_SysDictItem_CategoryCode`(`category_code` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 126 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '字典项表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_dict_item
-- ----------------------------
INSERT INTO `sys_dict_item` VALUES (1, 'sys_user_sex', '男', '0', '', '', 'Y', '0', 1, '性别男', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (2, 'sys_user_sex', '女', '1', '', '', 'N', '0', 2, '性别女', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (3, 'sys_user_sex', '未知', '2', '', '', 'N', '0', 3, '性别未知', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (4, 'sys_show_hide', '显示', '0', '', 'primary', 'Y', '0', 1, '显示菜单', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (5, 'sys_show_hide', '隐藏', '1', '', 'danger', 'N', '0', 2, '隐藏菜单', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (6, 'sys_normal_disable', '正常', '0', '', 'primary', 'Y', '0', 1, '正常状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (7, 'sys_normal_disable', '停用', '1', '', 'danger', 'N', '0', 2, '停用状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (8, 'sys_job_status', '正常', '0', '', 'primary', 'Y', '0', 1, '正常状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (9, 'sys_job_status', '暂停', '1', '', 'danger', 'N', '0', 2, '停用状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (10, 'sys_job_group', '默认', 'DEFAULT', '', '', 'Y', '0', 1, '默认分组', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (11, 'sys_job_group', '系统', 'SYSTEM', '', '', 'N', '0', 2, '系统分组', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (12, 'sys_yes_no', '是', 'Y', '', 'primary', 'Y', '0', 1, '系统默认是', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (13, 'sys_yes_no', '否', 'N', '', 'danger', 'N', '0', 2, '系统默认否', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (14, 'sys_notice_type', '通知', '1', '', 'warning', 'Y', '0', 1, '通知', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (15, 'sys_notice_type', '公告', '2', '', 'success', 'N', '0', 2, '公告', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (16, 'sys_notice_status', '正常', '0', '', 'primary', 'Y', '0', 1, '正常状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (17, 'sys_notice_status', '关闭', '1', '', 'danger', 'N', '0', 2, '关闭状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (18, 'sys_oper_type', '新增', '1', '', 'info', 'N', '0', 1, '新增操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (19, 'sys_oper_type', '修改', '2', '', 'info', 'N', '0', 2, '修改操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (20, 'sys_oper_type', '删除', '3', '', 'danger', 'N', '0', 3, '删除操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (21, 'sys_oper_type', '授权', '4', '', 'primary', 'N', '0', 4, '授权操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (22, 'sys_oper_type', '导出', '5', '', 'warning', 'N', '0', 5, '导出操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (23, 'sys_oper_type', '导入', '6', '', 'warning', 'N', '0', 6, '导入操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (24, 'sys_oper_type', '强退', '7', '', 'danger', 'N', '0', 7, '强退操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (25, 'sys_oper_type', '生成代码', '8', '', 'warning', 'N', '0', 8, '生成操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (26, 'sys_oper_type', '清空数据', '9', '', 'danger', 'N', '0', 9, '清空操作', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (27, 'sys_common_status', '成功', '0', '', 'primary', 'N', '0', 1, '正常状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (28, 'sys_common_status', '失败', '1', '', 'danger', 'N', '0', 2, '停用状态', '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (105, 'sys_boolean', '是', '1', NULL, 'default', 'N', '0', 0, NULL, '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (106, 'sys_boolean', '否', '0', NULL, 'default', 'N', '0', 1, NULL, '2025-01-01 00:00:00.000', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (119, '1', '1', '1', NULL, 'default', NULL, '0', 0, NULL, '2025-01-01 00:00:00.000', 1, '2025-09-02 13:31:13.000', 1, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (120, '877878', '11119', '22229', '99000', 'default', NULL, '1', 9, '9999000', '2025-10-27 08:26:44.000', 1, '2025-10-27 08:27:29.000', 1, '2025-10-27 08:29:12.000', 1);
INSERT INTO `sys_dict_item` VALUES (121, '877878', '888', '8988', '898989', 'default', NULL, '0', 0, NULL, '2025-10-27 08:28:00.000', 1, '2025-10-27 08:28:00.000', 1, '2025-10-27 08:29:12.000', 1);
INSERT INTO `sys_dict_item` VALUES (122, '877878', '89898', '898989', NULL, 'default', NULL, '0', 0, NULL, '2025-10-27 08:28:10.000', 1, '2025-10-27 08:28:10.000', 1, '2025-10-27 08:29:12.000', 1);
INSERT INTO `sys_dict_item` VALUES (123, '877878', '989889', '989898', NULL, 'default', NULL, '0', 0, NULL, '2025-10-27 08:28:23.000', 1, '2025-10-27 08:28:23.000', 1, '2025-10-27 08:29:12.000', 1);
INSERT INTO `sys_dict_item` VALUES (124, 'transaction_status', '已完成', '0', NULL, 'default', NULL, '0', 0, NULL, '2026-03-07 07:54:52.000', 1, '2026-03-07 07:54:52.000', 1, NULL, NULL);
INSERT INTO `sys_dict_item` VALUES (125, 'transaction_status', '已作废', '1', NULL, 'default', NULL, '0', 1, NULL, '2026-03-07 07:54:59.000', 1, '2026-03-09 06:08:36.000', 1, NULL, NULL);

-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_menu`;
CREATE TABLE `sys_menu`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '菜单ID',
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '菜单名称',
  `parent_id` bigint NULL DEFAULT 0 COMMENT '父菜单ID',
  `route_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '路由名称',
  `path` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '路由地址',
  `component` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '组件路径',
  `query` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '路由参数',
  `is_frame` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '1' COMMENT '是否为外链（0是 1否）',
  `is_cache` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '是否缓存（0缓存 1不缓存）',
  `menu_type` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '菜单类型（M目录 C菜单 F按钮）',
  `visible` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '菜单状态（0显示 1隐藏）',
  `perms` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '权限标识',
  `icon` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '#' COMMENT '菜单图标',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '菜单状态（0正常 1停用）',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2270 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜单权限表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_menu
-- ----------------------------
INSERT INTO `sys_menu` VALUES (1, '系统管理', 0, NULL, 'system', NULL, '', '0', '0', 'M', '0', '', 'setting-fill', '0', 999, '系统管理目录', NULL, NULL, '2025-10-29 07:33:27.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (100, '用户管理', 1, 'User', 'user', 'system/user/index', '', '0', '0', 'C', '0', 'system:user:list', 'adduser', '0', 1, '用户管理菜单', NULL, NULL, '2026-03-04 06:04:56.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (101, '角色管理', 1, 'Role', 'role', 'system/role/index', '', '0', '0', 'C', '0', 'system:role:list', 'tags', '0', 2, '角色管理菜单', NULL, NULL, '2026-03-04 06:05:02.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (102, '菜单管理', 1, 'Menu', 'menu', 'system/menu/index', '', '0', '0', 'C', '0', 'system:menu:list', 'appstore', '0', 3, '菜单管理菜单', NULL, NULL, '2026-03-04 06:05:12.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (103, '部门管理', 1, 'Dept', 'dept', 'system/dept/index', '', '0', '0', 'C', '1', 'system:dept:list', 'team', '1', 4, '部门管理菜单', NULL, NULL, '2026-03-04 06:05:18.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (105, '字典管理', 1, 'DictCategory', 'dict', 'system/dict/category', '', '0', '0', 'C', '0', 'system:dict:list', 'book', '0', 6, '字典管理菜单', NULL, NULL, '2026-03-04 06:06:29.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1001, '用户查询', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:query', '#', '0', 1, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1002, '用户新增', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:add', '#', '0', 2, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1003, '用户修改', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:edit', '#', '0', 3, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1004, '用户删除', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:remove', '#', '0', 4, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1005, '用户导出', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:export', '#', '0', 5, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1006, '用户导入', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:import', '#', '0', 6, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1007, '重置密码', 100, NULL, '', '', '', '0', '0', 'F', '0', 'system:user:resetPwd', '#', '0', 7, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1008, '角色查询', 101, NULL, '', '', '', '0', '0', 'F', '0', 'system:role:query', '#', '0', 1, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1009, '角色新增', 101, NULL, '', '', '', '0', '0', 'F', '0', 'system:role:add', '#', '0', 2, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1010, '角色修改', 101, NULL, '', '', '', '0', '0', 'F', '0', 'system:role:edit', '#', '0', 3, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1011, '角色删除', 101, NULL, '', '', '', '0', '0', 'F', '0', 'system:role:remove', '#', '0', 4, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1012, '角色导出', 101, NULL, '', '', '', '0', '0', 'F', '0', 'system:role:export', '#', '0', 5, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1013, '菜单查询', 102, NULL, '', '', '', '0', '0', 'F', '0', 'system:menu:query', '#', '0', 1, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1014, '菜单新增', 102, NULL, '', '', '', '0', '0', 'F', '0', 'system:menu:add', '#', '0', 2, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1015, '菜单修改', 102, NULL, '', '', '', '0', '0', 'F', '0', 'system:menu:edit', '#', '0', 3, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1016, '菜单删除', 102, NULL, '', '', '', '0', '0', 'F', '0', 'system:menu:remove', '#', '0', 4, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1017, '部门查询', 103, NULL, '', '', '', '0', '0', 'F', '0', 'system:dept:query', '#', '0', 1, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1018, '部门新增', 103, NULL, '', '', '', '0', '0', 'F', '0', 'system:dept:add', '#', '0', 2, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1019, '部门修改', 103, NULL, '', '', '', '0', '0', 'F', '0', 'system:dept:edit', '#', '0', 3, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1020, '部门删除', 103, NULL, '', '', '', '0', '0', 'F', '0', 'system:dept:remove', '#', '0', 4, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1026, '字典查询', 105, NULL, '#', '', '', '0', '0', 'F', '0', 'system:dict:query', '#', '0', 1, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1027, '字典新增', 105, NULL, '#', '', '', '0', '0', 'F', '0', 'system:dict:add', '#', '0', 2, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1028, '字典修改', 105, NULL, '#', '', '', '0', '0', 'F', '0', 'system:dict:edit', '#', '0', 3, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1029, '字典删除', 105, NULL, '#', '', '', '0', '0', 'F', '0', 'system:dict:remove', '#', '0', 4, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (1030, '字典导出', 105, NULL, '#', '', '', '0', '0', 'F', '0', 'system:dict:export', '#', '0', 5, '', NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2186, '分类管理', 0, 'TransactionCategory', 'transaction-category', 'ledger/transaction-category', NULL, '0', '0', 'C', '0', '', 'select', '0', 20, NULL, NULL, NULL, '2026-04-30 06:20:15.067', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2189, '交易管理', 0, 'Transaction', 'transaction', 'ledger/transaction', NULL, '0', '0', 'C', '0', '', 'transaction', '0', 40, NULL, NULL, NULL, '2026-04-30 06:20:24.330', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2242, '账本管理', 0, 'Ledger', 'ledger', 'ledger/index', NULL, '0', '0', 'C', '0', NULL, 'accountbook-fill', '0', 10, NULL, NULL, NULL, '2026-04-30 06:20:09.141', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2244, 'App管理', 0, NULL, 'app', NULL, NULL, '0', '0', 'M', '0', NULL, 'android', '0', 80, NULL, NULL, NULL, '2025-10-29 07:33:20.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2245, '应用管理', 2244, 'AppManage', 'app-manage', 'app/index', NULL, '0', '0', 'C', '0', 'app:manage:list', 'Batchfolding', '0', 1, NULL, NULL, NULL, '2026-03-04 05:41:04.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2246, '版本管理', 2244, 'AppVersion', 'app-version', 'app/version', NULL, '0', '0', 'C', '0', 'app:version:list', 'Batchfolding', '0', 2, NULL, NULL, NULL, '2026-03-04 05:40:54.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2247, '文件中心', 0, 'FileStorageCenter', 'file-storage-center', 'file-storage-center/index', NULL, '0', '0', 'C', '0', 'fileStorageCenter:list', 'filesearch', '0', 70, NULL, NULL, NULL, '2026-04-30 06:24:23.726', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2255, '定时任务', 1, NULL, 'http://localhost:20001/hangfire', NULL, NULL, '0', '0', 'C', '0', NULL, 'Field-time', '0', 20, '', '2025-10-13 07:30:08.000', 1, '2026-04-29 02:13:43.305', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2262, '演示', 0, NULL, 'demo', NULL, NULL, '0', '0', 'M', '0', NULL, 'appstore-fill', '0', 110, NULL, '2026-03-04 01:54:19.000', 1, '2026-03-04 01:54:19.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2263, 'demo1', 2262, 'Demo1', 'demo1', 'demo/demo1', NULL, '0', '0', 'C', '0', NULL, 'block', '0', 10, NULL, '2026-03-04 01:55:02.000', 1, '2026-03-04 06:03:50.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2264, 'demo2', 2262, 'Demo2', 'demo2', 'demo/demo2', NULL, '0', '0', 'C', '0', NULL, 'block', '0', 20, NULL, '2026-03-04 02:21:52.000', 1, '2026-03-04 06:03:57.000', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2266, '健康监测', 1, NULL, 'http://localhost:20001/healthchecks-ui', NULL, NULL, '0', '0', 'C', '0', NULL, 'heart', '0', 30, NULL, '2026-04-29 01:47:52.375', 1, '2026-05-05 06:48:55.611', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2268, '导出excel', 2262, 'Export', 'export', 'demo/export1', NULL, '0', '0', 'C', '0', NULL, 'block', '0', 30, NULL, '2026-04-30 02:41:40.470', 1, '2026-05-05 02:37:56.155', 1, NULL, NULL);
INSERT INTO `sys_menu` VALUES (2269, '在线excel', 2262, 'OnlineExcel', 'online-excel', 'demo/online-excel', NULL, '0', '0', 'C', '0', NULL, 'block', '0', 40, NULL, '2026-04-30 06:22:05.557', 1, '2026-05-05 02:39:29.050', 1, NULL, NULL);

-- ----------------------------
-- Table structure for sys_role
-- ----------------------------
DROP TABLE IF EXISTS `sys_role`;
CREATE TABLE `sys_role`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '角色ID',
  `name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '角色名称',
  `key` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '角色权限',
  `data_scope` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '1' COMMENT '数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）',
  `menu_check_strictly` bit(1) NULL DEFAULT NULL COMMENT '菜单树选择项是否关联显示',
  `dept_check_strictly` bit(1) NULL DEFAULT NULL COMMENT '部门树选择项是否关联显示',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '角色状态（0正常 1停用）',
  `sort` int NOT NULL COMMENT '显示顺序',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 14 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '角色信息表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_role
-- ----------------------------
INSERT INTO `sys_role` VALUES (1, '超级管理员', 'admin', '1', b'1', b'1', '0', 1, '超级管理员', '2025-06-22 05:45:46.000', NULL, '2025-10-16 07:02:52.000', 1, NULL, 1);
INSERT INTO `sys_role` VALUES (2, '普通用户', 'general_user', NULL, b'1', b'1', '0', 2, NULL, '2025-06-22 05:45:46.000', NULL, '2025-09-13 07:23:22.000', 10, NULL, NULL);
INSERT INTO `sys_role` VALUES (3, '测试角色', 'test', NULL, b'1', b'1', '0', 3, '1', '2025-06-22 05:45:46.000', NULL, '2025-10-28 01:15:46.000', 1, NULL, NULL);
INSERT INTO `sys_role` VALUES (10, '1', '12', NULL, b'1', NULL, '0', 111, '888888', '2025-10-24 07:45:02.000', 1, NULL, NULL, '2025-10-24 07:50:25.000', 1);
INSERT INTO `sys_role` VALUES (11, '555588', '55588', NULL, b'1', NULL, '0', 88555, '565568888888', '2025-10-24 07:45:13.000', 1, '2025-10-24 07:48:19.000', 1, '2025-10-24 07:48:27.000', 1);
INSERT INTO `sys_role` VALUES (12, '888', '98898', NULL, b'1', NULL, '0', 9888, NULL, '2025-10-24 07:48:42.000', 1, NULL, NULL, '2025-10-24 07:50:25.000', 1);
INSERT INTO `sys_role` VALUES (13, '8880', '990', NULL, b'1', NULL, '1', 770, '78780', '2025-10-28 01:28:39.000', 1, '2025-10-28 01:29:02.000', 1, '2025-10-28 01:29:08.000', 1);

-- ----------------------------
-- Table structure for sys_role_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_role_menu`;
CREATE TABLE `sys_role_menu`  (
  `role_id` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '角色ID',
  `menu_id` bigint NOT NULL COMMENT '菜单ID',
  PRIMARY KEY (`role_id`, `menu_id`) USING BTREE,
  INDEX `IX_SysRoleMenu_RoleId`(`role_id` ASC) USING BTREE,
  INDEX `IX_SysRoleMenu_MenuId`(`menu_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '角色和菜单关联表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_role_menu
-- ----------------------------
INSERT INTO `sys_role_menu` VALUES ('3', 1);
INSERT INTO `sys_role_menu` VALUES ('3', 100);
INSERT INTO `sys_role_menu` VALUES ('3', 101);
INSERT INTO `sys_role_menu` VALUES ('3', 102);
INSERT INTO `sys_role_menu` VALUES ('3', 103);
INSERT INTO `sys_role_menu` VALUES ('3', 105);
INSERT INTO `sys_role_menu` VALUES ('3', 108);
INSERT INTO `sys_role_menu` VALUES ('3', 500);
INSERT INTO `sys_role_menu` VALUES ('3', 501);
INSERT INTO `sys_role_menu` VALUES ('3', 1001);
INSERT INTO `sys_role_menu` VALUES ('3', 1002);
INSERT INTO `sys_role_menu` VALUES ('3', 1003);
INSERT INTO `sys_role_menu` VALUES ('3', 1004);
INSERT INTO `sys_role_menu` VALUES ('3', 1005);
INSERT INTO `sys_role_menu` VALUES ('3', 1006);
INSERT INTO `sys_role_menu` VALUES ('3', 1007);
INSERT INTO `sys_role_menu` VALUES ('3', 1008);
INSERT INTO `sys_role_menu` VALUES ('3', 1009);
INSERT INTO `sys_role_menu` VALUES ('3', 1010);
INSERT INTO `sys_role_menu` VALUES ('3', 1011);
INSERT INTO `sys_role_menu` VALUES ('3', 1012);
INSERT INTO `sys_role_menu` VALUES ('3', 1013);
INSERT INTO `sys_role_menu` VALUES ('3', 1014);
INSERT INTO `sys_role_menu` VALUES ('3', 1015);
INSERT INTO `sys_role_menu` VALUES ('3', 1016);
INSERT INTO `sys_role_menu` VALUES ('3', 1017);
INSERT INTO `sys_role_menu` VALUES ('3', 1018);
INSERT INTO `sys_role_menu` VALUES ('3', 1019);
INSERT INTO `sys_role_menu` VALUES ('3', 1020);
INSERT INTO `sys_role_menu` VALUES ('3', 1026);
INSERT INTO `sys_role_menu` VALUES ('3', 1027);
INSERT INTO `sys_role_menu` VALUES ('3', 1028);
INSERT INTO `sys_role_menu` VALUES ('3', 1029);
INSERT INTO `sys_role_menu` VALUES ('3', 1030);
INSERT INTO `sys_role_menu` VALUES ('3', 2190);
INSERT INTO `sys_role_menu` VALUES ('2', 2242);
INSERT INTO `sys_role_menu` VALUES ('3', 2242);

-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS `sys_user`;
CREATE TABLE `sys_user`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '用户ID',
  `user_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '用户账号',
  `nick_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '用户昵称',
  `email` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '用户邮箱',
  `phone_number` varchar(11) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '手机号码',
  `sex` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '用户性别（0男 1女 2未知）',
  `avatar` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '头像地址',
  `password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '密码',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '帐号状态（0正常 1停用）',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `IX_SysUser_UserName`(`user_name` ASC) USING BTREE,
  INDEX `IX_SysUser_PhoneNumber`(`phone_number` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 15 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '用户信息表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_user
-- ----------------------------
INSERT INTO `sys_user` VALUES (1, 'admin', '管理员', 'hilanmiao@126.com', '18353674768', '0', '/uploads/images/d25cd9c2-b669-4b4f-9ecf-854235bdb216-9999.png', '$2a$11$r0MX/7tSkwoNMp2B7tnUx.dh7H.jOf3C0GcGGWeBSXRmROAv3LPSy', '0', '管理员', '2025-09-13 07:20:29.000', NULL, '2026-04-10 01:32:42.928', 1, NULL, NULL);
INSERT INTO `sys_user` VALUES (2, 'test', '测试用户', NULL, NULL, NULL, NULL, '$2a$11$q/cEEQnLM44I62deb9UJl.5P4xaFGcK8pmGTLoHe7uyq7Zmmgv.lO', '0', NULL, '2025-09-13 07:20:29.000', 1, '2025-11-22 08:08:21.000', 1, NULL, NULL);
INSERT INTO `sys_user` VALUES (3, 'zhangsan', '张三', 'hilanmiao@126.com', '18353674768', '0', NULL, '$2a$11$b/LRJVuo.wICWAPrWHZFYOSTgn6ZebTR.kOwH.lRNmrKSqwvzGmS.', '0', '888', '2025-09-13 07:22:37.000', 9, '2025-10-16 01:34:05.000', 1, '2025-10-22 08:36:30.000', 1);
INSERT INTO `sys_user` VALUES (12, 'admin1', '12', NULL, NULL, NULL, NULL, '$2a$11$m1AcDCRabiFpLqvhIHmrqur16RTp6PsTL7.BlZo6t/yDXciYubhvK', '0', NULL, '2025-10-24 07:40:00.000', 1, NULL, NULL, '2025-10-24 07:41:16.000', 1);
INSERT INTO `sys_user` VALUES (13, '1222', '12', NULL, '', '0', NULL, '$2a$11$Sunsf8QqQPONZQd0O5TtCenvAwvxIlb4RwswannySeyWv/1TG2XCi', '0', NULL, '2025-10-24 07:40:18.000', 1, '2025-10-24 07:40:34.000', 1, '2025-10-24 07:40:36.000', 1);
INSERT INTO `sys_user` VALUES (14, '55555', '555', NULL, NULL, NULL, NULL, '$2a$11$7qHv4StCpKzFLGS9mgNStO/4T7y4P50lN1aY87W.LPb6MXJpSWnlO', '0', NULL, '2025-10-24 07:40:56.000', 1, NULL, NULL, '2025-10-24 07:41:16.000', 1);

-- ----------------------------
-- Table structure for sys_user_role
-- ----------------------------
DROP TABLE IF EXISTS `sys_user_role`;
CREATE TABLE `sys_user_role`  (
  `user_id` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '用户ID',
  `role_id` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '角色ID',
  PRIMARY KEY (`user_id`, `role_id`) USING BTREE,
  INDEX `IX_SysUserRole_UserId`(`user_id` ASC) USING BTREE,
  INDEX `IX_SysUserRole_RoleId`(`role_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '用户和角色关联表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of sys_user_role
-- ----------------------------
INSERT INTO `sys_user_role` VALUES ('1', '1');
INSERT INTO `sys_user_role` VALUES ('2', '3');

-- ----------------------------
-- Table structure for transaction
-- ----------------------------
DROP TABLE IF EXISTS `transaction`;
CREATE TABLE `transaction`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符（主键）',
  `ledger_id` bigint NULL DEFAULT NULL COMMENT '关联账本的ID',
  `transaction_category_id` bigint NULL DEFAULT NULL COMMENT '关联交易分类的ID',
  `type` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT '类型（收入、支出、不计入收支）',
  `date` datetime(3) NULL DEFAULT NULL COMMENT '交易日期（UTC）',
  `description` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT '交易描述',
  `amount` decimal(18, 4) NULL DEFAULT NULL COMMENT '交易金额',
  `status` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '0' COMMENT '状态。0 表示已完成, 1 表示已作废',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `IX_Transaction_Ledger_Date_DeletedAt`(`ledger_id` ASC, `date` ASC, `deleted_at` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 78 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of transaction
-- ----------------------------
INSERT INTO `transaction` VALUES (1, 4, 1, '不计入收支', '2026-03-09 05:57:12.000', '121212', 111.0000, '0', '121212121', '2026-03-07 08:02:21.000', 1, '2026-03-09 05:57:18.000', 1, NULL, 1);
INSERT INTO `transaction` VALUES (2, 7, 36, '收入', '2026-03-09 05:57:32.000', '22222', 222.0000, '1', '2222', '2026-03-09 05:57:35.000', 1, '2026-03-09 08:23:12.000', 1, NULL, 1);
INSERT INTO `transaction` VALUES (3, 4, 36, '支出', '2026-03-09 06:22:23.000', '12124r', 4.0000, '0', '141', '2026-03-09 06:22:28.000', 1, '2026-03-09 08:23:08.000', 1, NULL, 1);
INSERT INTO `transaction` VALUES (4, 7, 1, '支出', '2026-03-13 06:37:14.000', '1111', 57.0000, '0', NULL, '2026-03-13 06:37:16.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (5, 7, 3, '支出', '2026-03-13 06:37:26.000', '1212', 56.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (6, 7, 3, '支出', '2026-03-13 06:37:27.000', '测试', 55.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (7, 7, 3, '支出', '2026-03-13 06:37:28.000', '测试', 54.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (8, 7, 3, '支出', '2026-03-13 06:37:29.000', '测试', 53.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (9, 7, 3, '支出', '2026-03-13 06:37:30.000', '测试', 52.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (10, 7, 3, '支出', '2026-03-13 06:37:31.000', '测试', 51.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (11, 7, 3, '支出', '2026-03-13 06:37:32.000', '测试', 50.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (12, 7, 3, '支出', '2026-03-13 06:37:33.000', '测试', 49.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (13, 7, 3, '支出', '2026-03-13 06:37:34.000', '测试', 48.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (14, 7, 3, '支出', '2026-03-13 06:37:35.000', '测试', 47.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (15, 7, 3, '支出', '2026-03-13 06:37:36.000', '测试', 46.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (16, 7, 3, '支出', '2026-03-13 06:37:37.000', '测试', 45.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (17, 7, 3, '支出', '2026-03-13 06:37:38.000', '测试', 44.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (18, 7, 3, '支出', '2026-03-13 06:37:39.000', '测试', 43.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (19, 7, 3, '支出', '2026-03-13 06:37:40.000', '测试', 42.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (20, 7, 3, '支出', '2026-03-13 06:37:41.000', '测试', 41.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (21, 7, 3, '支出', '2026-03-13 06:37:42.000', '测试', 40.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (22, 7, 3, '支出', '2026-03-13 06:37:43.000', '测试', 39.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (23, 7, 3, '支出', '2026-03-13 06:37:44.000', '测试', 38.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (24, 7, 3, '支出', '2026-03-13 06:37:45.000', '测试', 37.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (25, 7, 3, '支出', '2026-03-13 06:37:46.000', '测试', 36.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (26, 7, 3, '支出', '2026-03-13 06:37:47.000', '测试', 35.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (27, 7, 3, '支出', '2026-03-13 06:37:48.000', '测试', 34.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (28, 7, 3, '支出', '2026-03-13 06:37:49.000', '测试', 33.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (29, 7, 3, '支出', '2026-03-13 06:37:50.000', '测试', 32.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (30, 7, 3, '支出', '2026-03-13 06:37:51.000', '测试', 31.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (31, 7, 3, '支出', '2026-03-13 06:37:52.000', '测试', 30.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (32, 7, 3, '支出', '2026-03-13 06:37:53.000', '测试', 29.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (33, 7, 3, '支出', '2026-03-13 06:37:54.000', '测试', 28.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (34, 7, 3, '支出', '2026-03-13 06:37:55.000', '测试', 27.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (35, 7, 3, '支出', '2026-03-13 06:37:56.000', '测试', 26.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (36, 7, 3, '支出', '2026-03-13 06:37:57.000', '测试', 25.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (37, 7, 3, '支出', '2026-03-13 06:37:58.000', '测试', 24.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (38, 7, 3, '支出', '2026-03-13 06:37:59.000', '测试', 23.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (39, 7, 3, '支出', '2026-03-13 06:38:00.000', '测试', 22.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (40, 7, 3, '支出', '2026-03-13 06:38:01.000', '测试', 21.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (41, 7, 3, '支出', '2026-03-13 06:38:02.000', '测试', 20.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (42, 7, 3, '支出', '2026-03-13 06:38:03.000', '测试', 19.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (43, 7, 3, '支出', '2026-03-13 06:38:04.000', '测试', 18.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (44, 7, 3, '支出', '2026-03-13 06:38:05.000', '测试', 17.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (45, 7, 3, '支出', '2026-03-13 06:38:06.000', '测试', 16.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (46, 7, 3, '支出', '2026-03-13 06:37:07.000', '测试', 15.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (47, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 14.0000, '0', NULL, '2026-03-13 06:37:27.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (48, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 13.0000, '0', NULL, '2026-03-13 06:37:28.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (49, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 12.0000, '0', NULL, '2026-03-13 06:37:29.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (50, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 11.0000, '0', NULL, '2026-03-13 06:37:30.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (51, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 10.0000, '0', NULL, '2026-03-13 06:37:31.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (52, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 9.0000, '0', NULL, '2026-03-13 06:37:32.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (53, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 8.0000, '0', NULL, '2026-03-13 06:37:33.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (54, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 7.0000, '0', NULL, '2026-03-13 06:37:34.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (55, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 6.0000, '0', NULL, '2026-03-13 06:37:35.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (56, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 5.0000, '0', NULL, '2026-03-13 06:37:36.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (57, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 4.0000, '0', NULL, '2026-03-13 06:37:37.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (58, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 3.0000, '0', NULL, '2026-03-13 06:37:38.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (59, 7, 3, '支出', '2026-03-13 06:37:26.000', '测试', 2.0000, '0', NULL, '2026-03-13 06:37:39.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (60, 7, 3, '支出', '2025-03-13 06:37:26.000', '测试', 1.0000, '0', NULL, '2026-03-13 06:37:40.000', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (61, 7, 4, '', '2026-03-31 00:00:00.000', '京东牛仔外套', 99.9900, '0', NULL, '2026-03-31 06:20:44.219', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (62, 7, 36, '', '2026-03-31 00:00:00.000', '早饭', 3.5000, '0', NULL, '2026-03-31 06:25:56.804', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (63, 7, 36, '', '2026-03-31 00:00:00.000', '京东卫衣', 66.0000, '0', NULL, '2026-03-31 06:27:26.349', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (64, 7, 11, '', '2026-03-31 00:00:00.000', '话费', 50.0000, '0', NULL, '2026-03-31 06:36:17.453', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (65, 7, 36, '', '2026-03-31 00:00:00.000', '222', 2.0000, '0', NULL, '2026-03-31 06:43:06.184', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (66, 7, 36, '', '2026-03-31 14:49:30.253', '11', 1111.0000, '0', NULL, '2026-03-31 06:49:57.543', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (67, 7, 2, '', '2026-03-31 14:50:22.376', '1313', 115.0000, '0', NULL, '2026-03-31 06:50:37.243', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (68, 7, 36, '', '2026-03-31 15:50:48.658', 'test', 2.0000, '0', NULL, '2026-03-31 07:50:59.905', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (69, 7, 36, '', '2026-03-31 00:00:00.000', '3', 75.0000, '0', NULL, '2026-03-31 07:52:22.016', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (70, 7, 5, '', '2026-03-31 15:58:40.214', '1212', 999.0000, '0', NULL, '2026-03-31 07:58:40.282', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (71, 7, 2, '支出', '2026-04-01 01:34:24.000', '测试交易', 12.0000, '0', NULL, '2026-04-01 01:34:27.002', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (72, 7, 18, '收入', '2026-04-01 01:34:51.000', '工资', 3000.0000, '0', NULL, '2026-04-01 01:34:52.883', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (73, 7, 3, '支出', '2026-04-01 02:18:59.000', '1313', 12.0000, '1', NULL, '2026-04-01 02:19:04.959', 1, '2026-04-01 05:28:27.474', 1, NULL, NULL);
INSERT INTO `transaction` VALUES (74, 7, 9, '收入', '2026-04-03 05:39:44.938', '加油9911111', 11199.0000, '1', '99999', '2026-04-01 07:46:06.501', 1, '2026-04-03 05:39:47.617', 1, NULL, NULL);
INSERT INTO `transaction` VALUES (75, 7, 10, '支出', '2026-04-03 05:41:47.551', '1111111111111111', 2596.0000, '0', NULL, '2026-04-03 05:41:47.644', 1, '2026-04-03 05:42:30.133', 1, '2026-04-03 05:43:36.580', 1);
INSERT INTO `transaction` VALUES (76, 7, 10, '支出', '2026-04-03 05:43:49.554', '11', 6699.0000, '0', NULL, '2026-04-03 05:43:49.603', 1, NULL, NULL, NULL, NULL);
INSERT INTO `transaction` VALUES (77, 7, 1, '支出', '2026-06-08 02:34:49.000', '早饭', 12.0000, '0', NULL, '2026-06-08 02:34:52.000', 1, NULL, NULL, NULL, NULL);

-- ----------------------------
-- Table structure for transaction_category
-- ----------------------------
DROP TABLE IF EXISTS `transaction_category`;
CREATE TABLE `transaction_category`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '唯一标识符',
  `name` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT '名称',
  `is_public` bit(1) NULL DEFAULT NULL COMMENT '是否为公共分类',
  `icon` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT '图标',
  `user_id` bigint NULL DEFAULT NULL COMMENT '用户ID',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `created_at` datetime(3) NULL DEFAULT NULL COMMENT '创建时间（UTC）',
  `created_by_id` bigint NULL DEFAULT NULL COMMENT '创建者ID，指向创建该记录的用户',
  `updated_at` datetime(3) NULL DEFAULT NULL COMMENT '更新时间（UTC）',
  `updated_by_id` bigint NULL DEFAULT NULL COMMENT '更新者ID，指向最后一次更新该记录的用户',
  `deleted_at` datetime(3) NULL DEFAULT NULL COMMENT '删除时间（UTC）',
  `deleted_by_id` bigint NULL DEFAULT NULL COMMENT '删除者ID，指向删除该记录的用户',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 39 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of transaction_category
-- ----------------------------
INSERT INTO `transaction_category` VALUES (1, '餐饮', b'1', 'food', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (2, '交通', b'1', 'car-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (3, '服饰', b'1', 'skin-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (4, '购物', b'1', 'shopping-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (5, '服务', b'1', 'customerservice-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (6, '借还款', b'1', 'creditcard-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (7, '教育', b'1', 'read', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (8, '娱乐', b'1', 'sofa', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (9, '运动', b'1', 'sports', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (10, '其他', b'1', 'smile-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (11, '生活缴费', b'1', 'thunderbolt-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (12, '收转账', b'1', 'transaction', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (13, '退款', b'1', 'refund', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (14, '收红包', b'1', 'redenvelope-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (15, '其他人情', b'1', 'addteam', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (16, '生意', b'1', 'business', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (17, '奖金', b'1', 'moneycollect-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (18, '工资', b'1', 'accountbook-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (19, '亲子', b'1', 'user', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (20, '旅行', b'1', 'bulb', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (21, '宠物', b'1', 'dog', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (22, '医疗', b'1', 'medicinebox-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (23, '保险', b'1', 'insurance-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (24, '公益', b'1', 'heartinhand', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (25, '发红包', b'1', 'accountaccess', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (26, '转账', b'1', 'transaction', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (27, '亲属卡', b'1', 'idcard', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (28, '其他人情', b'1', 'deleteteam', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (29, '其他', b'1', 'small-dash', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (30, '理财', b'1', 'bank-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (31, '房贷', b'0', 'pushpin-fill', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `transaction_category` VALUES (36, '测试', b'0', 'audio-fill', 1, '111', '2026-02-25 06:14:52.000', 1, '2026-02-25 06:17:09.000', 1, NULL, 1);
INSERT INTO `transaction_category` VALUES (37, 'app创建分类', b'0', 'zhengqi', 1, '23让发生的发生的', '2026-04-03 02:11:05.826', 1, NULL, NULL, '2026-04-03 02:28:52.064', 1);
INSERT INTO `transaction_category` VALUES (38, '444446', b'0', 'suitcase', 1, '112121212113134', '2026-04-03 02:11:57.091', 1, '2026-04-03 02:28:19.390', 1, '2026-04-03 02:28:22.406', 1);

SET FOREIGN_KEY_CHECKS = 1;
