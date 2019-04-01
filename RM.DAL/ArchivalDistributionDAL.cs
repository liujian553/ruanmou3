using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RM.Common;

namespace RM.DAL
{
    [Injectable(typeof(ArchivalDistributionDAL))]
    public interface IArchivalDistributionDAL
    {
        IEnumerable<T> GetDistributionRecords<T>(Dictionary<string, string> recordDictionary, ref int total);
    }


    /// <summary>
    /// 档案上交 - 数据类
    /// </summary>
    public class ArchivalDistributionDAL : IArchivalDistributionDAL
    {
        public IEnumerable<T> GetDistributionRecords<T>(Dictionary<string, string> recordDictionary, ref int total)
        {
            string sql =
                @"select count(fr.ID) [Count],RecordID from tblRecordFileRelation fr where fr.IsFolder=0";
            return DBConn.RetrieveBySql<T>(sql, new { });
        }
    }
}
