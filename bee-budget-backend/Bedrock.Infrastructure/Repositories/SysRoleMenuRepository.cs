using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;
using System;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 角色菜单关联仓储实现类，用于管理角色菜单关联的增删改查操作。
    /// </summary>
    public class SysRoleMenuRepository : ISysRoleMenuRepository
    {
        private readonly ISqlSugarClient _db; // 数据库上下文实例

        /// <summary>
        /// 构造函数注入数据库上下文。
        /// </summary>
        /// <param name="db">SqlSugar 数据库上下文。</param>
        public SysRoleMenuRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建角色菜单关联。
        /// </summary>
        /// <param name="entity">要创建的角色菜单关联实体对象。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> CreateAsync(SysRoleMenu entity)
        {
            var rowsAffected = await _db.Insertable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量创建角色菜单关联。
        /// </summary>
        /// <param name="entities">要创建的角色菜单关联实体对象集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        public async Task<int> CreateBatchAsync(IEnumerable<SysRoleMenu> entities)
        {
            var entityArray = entities as SysRoleMenu[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Insertable(entities.ToList()).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 删除角色菜单关联。
        /// </summary>
        /// <param name="entity">要删除的角色菜单关联实体对象。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        public async Task<int> DeleteAsync(SysRoleMenu entity)
        {
            var rowsAffected = await _db.Deleteable<SysRoleMenu>()
                .Where(rm => rm.RoleId == entity.RoleId && rm.MenuId == entity.MenuId)
                .ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 根据角色ID删除所有关联的菜单。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        public async Task<int> DeleteByRoleIdAsync(long roleId)
        {
            var rowsAffected= await _db.Deleteable<SysRoleMenu>()
                .Where(rm => rm.RoleId == roleId)
                .ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 根据菜单ID删除所有关联的角色。
        /// </summary>
        /// <param name="menuId">菜单ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        public async Task<int> DeleteByMenuIdAsync(long menuId)
        {
            var rowsAffected = await _db.Deleteable<SysRoleMenu>()
                .Where(rm => rm.MenuId == menuId)
                .ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 根据角色ID获取所有关联的菜单ID。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <returns>返回与角色关联的菜单ID集合。</returns>
        public async Task<List<long>> GetMenuIdsByRoleIdAsync(long roleId)
        {
            var MenuIds = await _db.Queryable<SysRoleMenu>()
                .Where(rm => rm.RoleId == roleId)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            return MenuIds;
        }

        /// <summary>
        /// 根据菜单ID获取所有关联的角色ID。
        /// </summary>
        /// <param name="menuId">菜单ID。</param>
        /// <returns>返回与菜单关联的角色ID集合。</returns>
        public async Task<List<long>> GetRoleIdsByMenuIdAsync(long menuId)
        {
            var roleIds = await _db.Queryable<SysRoleMenu>()
                .Where(rm => rm.MenuId == menuId)
                .Select(rm => rm.RoleId)
                .ToListAsync();

            return roleIds;
        }

        /// <summary>
        /// 检查角色和菜单是否已关联。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <param name="menuId">菜单ID。</param>
        /// <returns>如果已关联返回true，否则返回false。</returns>
        public async Task<bool> ExistsAsync(long roleId, long menuId)
        {
            var exists = await _db.Queryable<SysRoleMenu>()
                .Where(rm => rm.RoleId == roleId && rm.MenuId == menuId)
                .AnyAsync();

            return exists;
        }

        /// <summary>
        /// 获取所有角色菜单关联。
        /// </summary>
        /// <returns>返回所有角色菜单关联的集合。</returns>
        public async Task<List<SysRoleMenu>> GetAllAsync()
        {
            var entities = await _db.Queryable<SysRoleMenu>()
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 根据角色 ID 列表，批量软删除菜单记录，并记录操作人。
        /// </summary>
        /// <param name="roleIds">要删除的角色 ID 列表。</param>
        /// <returns>返回成功标记为删除的记录数量；若角色 ID 列表为空，则返回 0。</returns>        
        public async Task<int> DeleteByRoleIdsAsync(IEnumerable<long> roleIds)
        {
            var roleIdList = roleIds as long[] ?? roleIds.ToArray();
            if (!roleIdList.Any())
                return 0;

            var rowsAffected = await _db.Deleteable<SysRoleMenu>()
                .Where(e => roleIdList.Contains(e.RoleId))
                .ExecuteCommandAsync();

            return rowsAffected;
        }
    }
} 