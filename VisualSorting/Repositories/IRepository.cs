using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSorting.Repositories
{
    /// <summary>
    /// 通用仓储接口
    /// 定义实体对象的标准CRUD操作契约
    /// 所有实体仓储接口都应继承此接口，确保统一的数据库操作规范
    /// 
    /// 泛型参数T：实体类型，必须是引用类型（class约束）
    /// 
    /// 实现类：ProductInspectionRecordRepository、MemberRepository
    /// 使用者：MainViewModel（通过ProductInspectionRecordRepository保存检测记录）
    ///         LoginViewModel（通过MemberRepository验证登录）
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 插入新实体记录到数据库
        /// </summary>
        /// <param name="entity">要插入的实体对象</param>
        /// <returns>受影响的行数，大于0表示插入成功</returns>
        int Insert(T entity);

        /// <summary>
        /// 更新已有的实体记录
        /// </summary>
        /// <param name="entity">要更新的实体对象（需包含主键）</param>
        /// <returns>受影响的行数，大于0表示更新成功</returns>
        int Update(T entity);

        /// <summary>
        /// 删除实体记录
        /// </summary>
        /// <param name="entity">要删除的实体对象（需包含主键）</param>
        /// <returns>受影响的行数，大于0表示删除成功</returns>
        int Delete(T entity);

        /// <summary>
        /// 获取所有实体记录
        /// </summary>
        /// <returns>所有实体的列表</returns>
        List<T> GetAll();

        /// <summary>
        /// 按关键字查找实体记录
        /// 不同实体的搜索字段不同，由实现类决定
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <returns>匹配的实体列表</returns>
        List<T> FindAll(string keyword);
    }
}
