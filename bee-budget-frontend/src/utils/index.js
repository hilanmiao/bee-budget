// 返回项目路径
export function getNormalPath(p) {
  if (p.length === 0 || !p || p == 'undefined') {
    return p
  };
  let res = p.replace('//', '/')
  if (res[res.length - 1] === '/') {
    return res.slice(0, res.length - 1)
  }
  return res
}

/**
 * 将扁平数组转换为树形结构
 * @param {Array} data - 扁平数据源
 * @param {string} [id='id'] - 节点唯一标识字段名
 * @param {string} [parentId='parentId'] - 父节点标识字段名
 * @param {string} [children='children'] - 子节点列表字段名
 * @returns {Array} 树形结构数组
 */
export function buildTree(data, id = 'id', parentId = 'parentId', children = 'children') {
  if (!Array.isArray(data)) return [];

  const config = {
    id,
    parentId,
    childrenList: children
  };

  const map = new Map();
  const tree = [];

  // 第一次遍历：建立 ID 到节点的映射，并初始化 children
  for (const item of data) {
    const node = { ...item }; // 避免修改原始数据（可选）
    if (!node[config.childrenList]) {
      node[config.childrenList] = [];
    }
    map.set(node[config.id], node);
  }

  // 第二次遍历：挂载到父节点或根数组
  for (const item of data) {
    const node = map.get(item[config.id]);
    const parent = map.get(item[config.parentId]);

    if (parent) {
      parent[config.childrenList].push(node);
    } else {
      tree.push(node);
    }
  }

  return tree;
}