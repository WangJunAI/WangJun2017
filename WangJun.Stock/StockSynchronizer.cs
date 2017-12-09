using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Stock
{
    /// <summary>
    /// 股市同步器
    /// </summary>
    public  class StockSynchronizer
    {
        #region 更新股票代码
        public void UpdateStockCode()
        {
            var inst = StockTaskExecutor.CreateInstance();
            var startTime = DateTime.Now;///开始运行时间
            while(true)
            {
                ///交易日每日收盘后更新
                
            }
        }
        #endregion
    }
}
