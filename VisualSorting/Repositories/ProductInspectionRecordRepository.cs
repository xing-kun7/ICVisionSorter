using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualSorting.Entities;

namespace VisualSorting.Repositories
{
    /// <summary>
    /// 产品检测记录仓储
    /// 封装ProductInspectionRecord实体的数据库CRUD操作
    /// 
    /// 继承：RepositoryBase（提供DbContext和IDisposable）
    /// 实现：IRepository&lt;ProductInspectionRecord&gt;（标准CRUD契约）
    /// 
    /// 使用者：
    /// - MainViewModel._recordRepository：折线图数据查询（GetAll）
    /// - MainViewModel.SaveInspectionResult()：保存检测结果（Insert）
    /// - MainViewModel.LoadHistoryRecordsAsync()：加载历史记录（GetAll）
    /// </summary>
    public class ProductInspectionRecordRepository : RepositoryBase, IRepository<ProductInspectionRecord>
    {
        /// <summary>
        /// 删除检测记录
        /// 通过设置EntityState为Deleted标记删除，调用SaveChanges提交到数据库
        /// </summary>
        /// <param name="entity">要删除的检测记录（需包含RecordId主键）</param>
        /// <returns>受影响的行数，大于0表示删除成功</returns>
        public int Delete(ProductInspectionRecord entity)
        {
            db.Entry(entity).State = System.Data.Entity.EntityState.Deleted;
            return db.SaveChanges();
        }

        /// <summary>
        /// 按产品型号关键字查找检测记录
        /// 使用ProductModel字段的Contains方法进行模糊匹配
        /// </summary>
        /// <param name="keyword">搜索关键字，匹配ProductModel字段</param>
        /// <returns>匹配的检测记录列表</returns>
        public List<ProductInspectionRecord> FindAll(string keyword)
        {
            return db.ProductInspectionRecord.Where(p => p.ProductModel.Contains(keyword)).ToList();
        }

        /// <summary>
        /// 获取所有检测记录
        /// 注意：数据量大时建议配合OrderBy和Take使用，避免一次性加载过多数据
        /// </summary>
        /// <returns>所有检测记录的列表</returns>
        public List<ProductInspectionRecord> GetAll()
        {
            return db.ProductInspectionRecord.ToList();
        }

        /// <summary>
        /// 插入新的检测记录
        /// 通过设置EntityState为Added标记新增，调用SaveChanges提交到数据库
        /// </summary>
        /// <param name="entity">要插入的检测记录对象</param>
        /// <returns>受影响的行数，大于0表示插入成功</returns>
        public int Insert(ProductInspectionRecord entity)
        {
            db.Entry(entity).State = System.Data.Entity.EntityState.Added;
            return db.SaveChanges();
        }

        /// <summary>
        /// 更新已有的检测记录
        /// 通过设置EntityState为Modified标记更新，调用SaveChanges提交到数据库
        /// </summary>
        /// <param name="entity">要更新的检测记录（需包含RecordId主键）</param>
        /// <returns>受影响的行数，大于0表示更新成功</returns>
        public int Update(ProductInspectionRecord entity)
        {
            db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            return db.SaveChanges();
        }
    }
}
