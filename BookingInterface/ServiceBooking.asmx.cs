using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

namespace BookingInterface
{
    /// <summary>
    ///     Service1 的摘要说明
    ///     技术组列的清单：
    ///     1.获取Teetime   请求参数：球场、日期、时间范围、洞数、
    ///     2.订单成功提交   提交数据给你们  参数  订单号、球场、日期、Teetime、人数、洞数、联系号码、是否支付（支付：支付方式、流水号、支付金额）（有其他需要可以在加）
    ///     3.取消订单	参数：取消用户类型（会员/管理员）、订单号
    ///     技术组-陈文刊  13:49:12
    ///     4、取消成功返回 (支付：支付方式、流水号、支付金额、退款金额
    /// </summary>
    [WebService(Namespace = "http://192.168.0.247/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class ServiceBooking : WebService
    {
        /// <summary>
        /// 获取TeeTime
        /// </summary>
        /// <param name="resId">球场ID</param>
        /// <param name="timeStart">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <param name="holeNum">洞数</param>
        /// <returns>ds数据集 xml格式</returns>
        [WebMethod(Description = "获取TeeTime")]
        public string GetTeeTime(string resId, DateTime timeStart, DateTime timeEnd, int holeNum)
        {
            var resultXml = string.Empty;
            try
            {
                var strSql = string.Format(@"SELECT  [TeeTime] ,
                                                        [MaxNum] ,
                                                        [UseNum] ,
                                                        [ResID] ,
                                                        [Create_day]
                                                FROM    [GroupSource_TeeTime]
                                                WHERE   ResID = {0}
                                                        AND TeeTime > '{1}'
                                                        AND TeeTime < '{2}'",
                    resId, timeStart, timeEnd);
                DataSet resultDataSet = DbHelperSQL.Query(strSql);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(resultDataSet.GetXml());
                foreach (XmlNode childNode in xmlDoc.FirstChild.ChildNodes)
                {
                    childNode.FirstChild.InnerText = DateTime.Parse(childNode.FirstChild.InnerText).ToString("yyyy-MM-dd HH:mm:ss");
                    childNode.LastChild.InnerText = DateTime.Parse(childNode.LastChild.InnerText).ToString("yyyy-MM-dd HH:mm:ss");
                }
                resultXml = xmlDoc.OuterXml;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return resultXml;
        }

        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="ORDER_NUMBER">订单号</param>
        /// <param name="FULL_NAME">姓名</param>
        /// <param name="COURT_ID">球场id</param>
        /// <param name="COURT_NAME">球场名称</param>
        /// <param name="TEETIME">TeeTime</param>
        /// <param name="NUMBER_OF_PEOPLE">人数</param>
        /// <param name="HOLE_NUMBER">洞数</param>
        /// <param name="CONTACT_NUMBER">联系电话</param>
        /// <param name="WHETHER_TO_PAY">是否支付</param>
        /// <param name="PAYMENT">支付方式</param>
        /// <param name="SERIAL_NUMBER">流水号</param>
        /// <param name="PAYMENT_AMOUNT">支付金额</param>
        /// <returns>影响行数</returns>
        [WebMethod(Description = "提交订单")]
        public int SubmitOrder(string ORDER_NUMBER, string FULL_NAME, string COURT_ID, string COURT_NAME,
            DateTime TEETIME, int NUMBER_OF_PEOPLE, int HOLE_NUMBER, string CONTACT_NUMBER, string WHETHER_TO_PAY, string PAYMENT, string SERIAL_NUMBER, decimal PAYMENT_AMOUNT)
        {
            int result = 0;
            try
            {
                string strSql = @"INSERT  INTO [GroupSource_TeeTime_Appointment]
                                    ( [ORDER_NUMBER] ,
                                        [FULL_NAME] ,
                                        [COURT_ID] ,
                                        [COURT_NAME] ,
                                        [TEETIME] ,
                                        [NUMBER_OF_PEOPLE] ,
                                        [HOLE_NUMBER] ,
                                        [CONTACT_NUMBER] ,
                                        [WHETHER_TO_PAY] ,
                                        [PAYMENT] ,
                                        [SERIAL_NUMBER] ,
                                        [PAYMENT_AMOUNT] 
                                    )
                            VALUES  ( @ORDER_NUMBER ,
                                        @FULL_NAME ,
                                        @COURT_ID ,
                                        @COURT_NAME ,
                                        @TEETIME ,
                                        @NUMBER_OF_PEOPLE ,
                                        @HOLE_NUMBER ,
                                        @CONTACT_NUMBER ,
                                        @WHETHER_TO_PAY ,
                                        @PAYMENT ,
                                        @SERIAL_NUMBER ,
                                        @PAYMENT_AMOUNT
                                    )";
                SqlParameter[] parameters = {
                    new SqlParameter("@ORDER_NUMBER", ORDER_NUMBER), 
                    new SqlParameter("@FULL_NAME", FULL_NAME), 
                    new SqlParameter("@COURT_ID", COURT_ID), 
                    new SqlParameter("@COURT_NAME", COURT_NAME), 
                    new SqlParameter("@TEETIME", TEETIME), 
                    new SqlParameter("@NUMBER_OF_PEOPLE", NUMBER_OF_PEOPLE), 
                    new SqlParameter("@HOLE_NUMBER", HOLE_NUMBER), 
                    new SqlParameter("@CONTACT_NUMBER", CONTACT_NUMBER), 
                    new SqlParameter("@WHETHER_TO_PAY", WHETHER_TO_PAY), 
                    new SqlParameter("@PAYMENT", PAYMENT), 
                    new SqlParameter("@SERIAL_NUMBER", SERIAL_NUMBER), 
                    new SqlParameter("@PAYMENT_AMOUNT", PAYMENT_AMOUNT)
                };
                result = DbHelperSQL.ExecuteSql(strSql, parameters);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return result;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="cancelUserType">取消用户类型</param>
        /// <param name="ORDER_NUMBER">订单号</param>
        /// <returns>DataSet 结果集(包含:支付方式、流水号、支付金额、退款金额)</returns>
        [WebMethod(Description = "取消订单")]
        public string CancelOrder(string cancelUserType, string ORDER_NUMBER)
        {
            DataSet resultDataSet = null;
            try
            {
                string strSql = @"UPDATE  [GroupSource_TeeTime_Appointment]   SET     [CANCELL] = 1    WHERE   ORDER_NUMBER = @ORDER_NUMBER";
                SqlParameter[] parameters = {new SqlParameter("@ORDER_NUMBER", ORDER_NUMBER)};
                if (DbHelperSQL.ExecuteSql(strSql, parameters) > 0)
                {
                    strSql = @"SELECT   [PAYMENT] ,
                                        [SERIAL_NUMBER] ,
                                        [PAYMENT_AMOUNT] ,
                                        [REFUND_AMOUNT]
                                FROM    [GroupSource_TeeTime_Appointment]
                                WHERE   ORDER_NUMBER = @ORDER_NUMBER";
                    resultDataSet = DbHelperSQL.Query(strSql, parameters);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return resultDataSet == null ? string.Empty : resultDataSet.GetXml();
        }
    }
}