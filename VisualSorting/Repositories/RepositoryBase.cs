using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualSorting.Entities;

namespace VisualSorting.Repositories
{
    /// <summary>
    /// 仓储基类
    /// 提供所有仓储类的公共基础设施：EF数据库上下文实例和IDisposable实现
    /// 
    /// 设计说明：
    /// - db字段是EF的DbContext实例，所有子类通过它访问数据库
    /// - 实现IDisposable标准模式，确保DbContext在使用完毕后释放
    /// - 子类（ProductInspectionRecordRepository、MemberRepository）继承此类获得数据库访问能力
    /// 
    /// 使用者：ProductInspectionRecordRepository、MemberRepository
    /// </summary>
    public abstract class RepositoryBase : IDisposable
    {
        /// <summary>
        /// EF数据库上下文实例
        /// 连接字符串名称：VisualSortingDBEntities3
        /// 包含Member和ProductInspectionRecord两个DbSet
        /// 子类通过此字段执行LINQ查询和EntityState操作
        /// </summary>
        protected VisualSortingDBEntities3 db = new VisualSortingDBEntities3();

        /// <summary>是否已释放标志</summary>
        private bool _disposed = false;

        /// <summary>
        /// 释放资源的核心方法（IDisposable标准模式）
        /// disposing=true时释放托管资源（DbContext）
        /// disposing=false时由终结器调用，仅释放非托管资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 释放资源（公共方法）
        /// 调用Dispose(true)释放所有资源，并阻止终结器执行
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
