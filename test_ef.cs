using System;
using System.Linq;
using VisualSorting.Entities;
using VisualSorting.Repositories;

namespace TestEF
{
    class Program
    {
        static void Main()
        {
            try
            {
                Console.WriteLine("Connecting to VisualSortingDB via EF...");
                using (var memberRepo = new MemberRepository())
                {
                    var users = memberRepo.GetAll();
                    Console.WriteLine($"[1] Member query: Found {users.Count} users:");
                    foreach (var u in users)
                    {
                        Console.WriteLine($"   - {u.Name} (Role: {u.Role})");
                    }
                }

                using (var recordRepo = new ProductInspectionRecordRepository())
                {
                    var record = new ProductInspectionRecord
                    {
                        ProductModel = "SOIC-16",
                        BatchNo = "BATCH-20261115-001",
                        InspectionTime = DateTime.Now,
                        TotalInspectedCount = 100,
                        QualifiedCount = 98,
                        DefectiveCount = 2,
                        DefectReasons = "Pin Bent (引脚弯折)",
                        OperatorId = 1,
                        DeviceNo = "LEADSHINE-LINE-01",
                        Remark = "WSL2 SQL Server 连通性与写入自动化验证"
                    };

                    int added = recordRepo.Insert(record);
                    Console.WriteLine($"[2] ProductInspectionRecord insert: Added {added} row, RecordId = {record.RecordId}");

                    var records = recordRepo.GetAll();
                    Console.WriteLine($"[3] Total inspection records in DB: {records.Count}");
                }
                Console.WriteLine("ALL DATABASE OPERATIONS PASSED PERFECTLY!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
            }
        }
    }
}
