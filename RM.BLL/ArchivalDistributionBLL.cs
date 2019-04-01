using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RM.Common;
using RM.DAL;

namespace RM.BLL
{
    [Injectable(typeof(ArchivalDistributionBLL))]
    public interface IArchivalDistributionBLL
    {
        IEnumerable<T> GetDistributionRecords<T>(Dictionary<string, string> recordDictionary, ref int total);

    }


    /// <summary>
    /// 档案上交 - 业务类
    /// </summary>
    public class ArchivalDistributionBLL : IArchivalDistributionBLL
    {
        IArchivalDistributionDAL ArchivalDistributionDAL;

        public ArchivalDistributionBLL(IArchivalDistributionDAL Dal)
        {
            ArchivalDistributionDAL = Dal;
        }
        public IEnumerable<T> GetDistributionRecords<T>(Dictionary<string, string> recordDictionary, ref int total)
        {
            return ArchivalDistributionDAL.GetDistributionRecords<T>(recordDictionary, ref total);
        }

    }
}
