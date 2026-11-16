using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualSorting.Entities;

namespace VisualSorting.Repositories
{
    /// <summary>
    /// 用户（会员）仓储
    /// 封装Member实体的数据库CRUD操作
    /// 
    /// 继承：RepositoryBase（提供DbContext和IDisposable）
    /// 实现：IRepository&lt;Member&gt;（标准CRUD契约）
    /// 
    /// 使用者：
    /// - LoginViewModel.OnLoginCommandAsync()：登录验证时查询用户名和密码
    /// </summary>
    public class MemberRepository : RepositoryBase, IRepository<Member>
    {
        /// <summary>
        /// 删除用户记录
        /// 通过设置EntityState为Deleted标记删除，调用SaveChanges提交到数据库
        /// </summary>
        /// <param name="entity">要删除的用户记录（需包含Id主键）</param>
        /// <returns>受影响的行数，大于0表示删除成功</returns>
        public int Delete(Member entity)
        {
            db.Entry(entity).State = System.Data.Entity.EntityState.Deleted;
            return db.SaveChanges();
        }

        /// <summary>
        /// 按用户名关键字查找用户记录
        /// 使用Name字段的Contains方法进行模糊匹配
        /// </summary>
        /// <param name="keyword">搜索关键字，匹配Name字段</param>
        /// <returns>匹配的用户记录列表</returns>
        public List<Member> FindAll(string keyword)
        {
            return db.Member.Where(p => p.Name.Contains(keyword)).ToList();
        }

        /// <summary>
        /// 获取所有用户记录
        /// </summary>
        /// <returns>所有用户记录的列表</returns>
        public List<Member> GetAll()
        {
            return db.Member.ToList();
        }

        /// <summary>
        /// 插入新的用户记录
        /// 通过设置EntityState为Added标记新增，调用SaveChanges提交到数据库
        /// </summary>
        /// <param name="entity">要插入的用户记录对象</param>
        /// <returns>受影响的行数，大于0表示插入成功</returns>
        public int Insert(Member entity)
        {
            db.Entry(entity).State = System.Data.Entity.EntityState.Added;
            return db.SaveChanges();
        }

        /// <summary>
        /// 更新已有的用户记录
        /// 通过设置EntityState为Modified标记更新，调用SaveChanges提交到数据库
        /// </summary>
        /// <param name="entity">要更新的用户记录（需包含Id主键）</param>
        /// <returns>受影响的行数，大于0表示更新成功</returns>
        public int Update(Member entity)
        {
            db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            return db.SaveChanges();
        }
    }
}
