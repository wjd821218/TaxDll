using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaxCardX;

namespace TaxDll
{
    public struct OpenTaxInfo
    {
        public double iInvLimit;
        public string sTaxCode;
        public int sMachineNo;
        public int iIsInvEmpty;
        public int iIsRepReached;
        public int iIsLockReached;
    }
    public struct StockTaxInfo
    {
        public short iInfoKind;
        public string sInfoTypeCode;
        public int iInfoNumber;
        public int iInvStock;
        public DateTime dTaxClock;
    }

    public struct InvocieHeader
    {
        public string sInfoClientName;
        public string sInfoClientTaxCode;
        public string sInfoClientBankAccount;
        public string sInfoClientAddressPhone;
        public string sInfoSellerBankAccount;
        public string sInfoSellerAddressPhone;
        public short iInfoTaxRate;
        public string sInfoNotes;
        public string sInfoInvoicer;
        public string sInfoChecker;
        public string sInfoCashier;
        public string sInfoListName;
        public string sInfoBillNumber;
    }
    public struct InvoiceRetInfo
    {
        public double dRetInfoAmount;
        public double dRetInfoTaxAmount;
        public DateTime dRetInfoInvDate;
        public short dRetInfoMonth;
        public string sRetInfoTypeCode;
        public long sRetInfoNumber;
        public short sRetGoodsListFlag;
    }
    public struct BatchUpInfo
    {
        public string sGoodsNoVer;
        public string sGoodsTaxNo;
        public string sTaxPre;
        public string sZeroTax;
        public string sCropGoodsNo;
    }    
    public class ComTaxCard
    {
        public string sRetMsg;
        public int iResult;

        public short iInvType = 2;
        public int iTaxCount = 1;
        public short iInfoShowPrtDlg = 0; //打印标识

        public GoldTax MyTax = new GoldTax();
        public OpenTaxInfo _OpenTaxInfo = new OpenTaxInfo();        //开卡信息
        public StockTaxInfo _StockTaxInfo = new StockTaxInfo();    //普票库存
        public StockTaxInfo _StockTaxInfoPro = new StockTaxInfo(); //专票库存
        public InvocieHeader _InvocieHeader = new InvocieHeader();
        public InvoiceRetInfo _InvoiceRetInfo = new InvoiceRetInfo();

        public DataTable dtInvocieDetail = new DataTable("InvocieDetail");

        public string sBatchUpXml = "";
        public void Create()
        {            
            dtInvocieDetail.Columns.Add("ListGoodsName", typeof(string));
            dtInvocieDetail.Columns.Add("ListTaxItem", typeof(string));
            dtInvocieDetail.Columns.Add("ListStandard", typeof(int));
            dtInvocieDetail.Columns.Add("ListUnit", typeof(string));
            dtInvocieDetail.Columns.Add("ListNumber", typeof(double));
            dtInvocieDetail.Columns.Add("ListPrice", typeof(double));
            dtInvocieDetail.Columns.Add("ListAmount", typeof(double));
            dtInvocieDetail.Columns.Add("ListPriceKind", typeof(int));
            dtInvocieDetail.Columns.Add("ListTaxAmount", typeof(int));


            iInfoShowPrtDlg = 0 ;
        }
        public void OpenCard()
        {
            
            int iResult = 0;
            string sRetMsg = "";

            MyTax.OpenCard();

            iResult = MyTax.RetCode;
            sRetMsg = MyTax.RetMsg;
           
            if (iResult == 1011)
            {
                _OpenTaxInfo.iInvLimit = MyTax.InvLimit;                
                _OpenTaxInfo.sTaxCode = MyTax.TaxCode;
                _OpenTaxInfo.sMachineNo = MyTax.MachineNo;
                _OpenTaxInfo.iIsInvEmpty = MyTax.IsInvEmpty;
                _OpenTaxInfo.iIsRepReached = MyTax.IsRepReached;
                _OpenTaxInfo.iIsLockReached = MyTax.IsLockReached;

                //查询库存发票
                
                MyTax.InfoKind = 0;
                MyTax.GetInfo();

                _StockTaxInfoPro.sInfoTypeCode = MyTax.InfoTypeCode;
                _StockTaxInfoPro.iInfoNumber = MyTax.InfoNumber;
                _StockTaxInfoPro.iInvStock = MyTax.InvStock;
                _StockTaxInfoPro.dTaxClock = MyTax.TaxClock;

                MyTax.InfoKind = 2;
                MyTax.GetInfo();

                _StockTaxInfo.sInfoTypeCode = MyTax.InfoTypeCode;
                _StockTaxInfo.iInfoNumber = MyTax.InfoNumber;
                _StockTaxInfo.iInvStock = MyTax.InvStock;
                _StockTaxInfo.dTaxClock = MyTax.TaxClock;

            }
            else
            {
                iResult = 1;
                sRetMsg = "金税卡打开失败！";                
            }

        }
        public int InvoiceBillHeader()
        {
            MyTax.InvInfoInit();

            if (iTaxCount != 1) { sRetMsg = "填写发票头失败，明细中或存在多种税率。"; return 1; }

            MyTax.InfoClientName = _InvocieHeader.sInfoClientName;
            MyTax.InfoClientTaxCode = _InvocieHeader.sInfoClientTaxCode;
            MyTax.InfoClientBankAccount = _InvocieHeader.sInfoClientBankAccount;
            MyTax.InfoClientAddressPhone = _InvocieHeader.sInfoClientAddressPhone;
            MyTax.InfoSellerBankAccount = _InvocieHeader.sInfoSellerBankAccount;
            MyTax.InfoSellerAddressPhone = _InvocieHeader.sInfoSellerAddressPhone;
            MyTax.InfoTaxRate = _InvocieHeader.iInfoTaxRate;
            MyTax.InfoNotes = _InvocieHeader.sInfoNotes;
            MyTax.InfoInvoicer = _InvocieHeader.sInfoInvoicer;
            MyTax.InfoChecker = _InvocieHeader.sInfoChecker;
            MyTax.InfoCashier = _InvocieHeader.sInfoChecker;
            MyTax.InfoListName = _InvocieHeader.sInfoListName;
            MyTax.InfoBillNumber = _InvocieHeader.sInfoBillNumber;

            return 0;
        }

        public int InvoiceBillDetail()
        {
            MyTax.ClearInvList();        

            if (dtInvocieDetail.Rows.Count > 0)
            {
                for (int i = 0; i < dtInvocieDetail.Rows.Count; i++)
                {
                    MyTax.InvListInit();
                    //商品名称
                    MyTax.ListGoodsName = dtInvocieDetail.Rows[i]["NAME"].ToString();

                    //BacthUpLoad 
                    string sTaxItem = dtInvocieDetail.Rows[i]["TAXITEM"].ToString();
                    string sTaxRate = dtInvocieDetail.Rows[i]["TAXRATE"].ToString();
                    string sZeroTax = "";
                    if (sTaxRate == "0") { sZeroTax = "3"; }
                    sBatchUpXml = getDataXml("30.0", sTaxItem, "0", sZeroTax, dtInvocieDetail.Rows[i]["ARTID"].ToString());
                    sBatchUpXml = BatchUpLoad(sBatchUpXml);
                    string sRetXml = MyTax.BatchUpload(sBatchUpXml);
                    if (fVaildBatchUp(sRetXml) == 1) { return 1; }

                    //其他信息
                    MyTax.ListTaxItem = dtInvocieDetail.Rows[i]["ListTaxItem"].ToString();
                    MyTax.ListStandard = dtInvocieDetail.Rows[i]["ListStandard"].ToString();
                    MyTax.ListUnit = dtInvocieDetail.Rows[i]["ListUnit"].ToString();
                    MyTax.ListNumber = double.Parse(dtInvocieDetail.Rows[i]["ListNumber"].ToString());
                    MyTax.ListPrice = double.Parse(dtInvocieDetail.Rows[i]["ListPrice"].ToString());
                    MyTax.ListAmount = double.Parse(dtInvocieDetail.Rows[i]["ListAmount"].ToString()); ;      //金额
                    MyTax.ListPriceKind = short.Parse(dtInvocieDetail.Rows[i]["ListPriceKind"].ToString()); ;   //含税价标志
                    MyTax.ListTaxAmount = short.Parse(dtInvocieDetail.Rows[i]["ListTaxAmount"].ToString()); ; //税额

                    MyTax.AddInvList();
                }
                return 0;
            }
            else
            {
                sRetMsg = "无可开的发票明细。";
                return 1;
            }


        }

        private string getDataXml(string sGoodsNoVer, string sGoodsTaxNo, string sTaxPre, string sZeroTax, string sCropGoodsNo)
        {
            string sTaxPreCon = "";

            if (sZeroTax == "3") { sTaxPreCon = "免税"; }
            string sDataXml =
            "<?xml version=\"1.0\" encoding=\"GBK\"?>" +
            "<FPXT>" +
                "<INPUT>" +
                    "<GoodsNo>" +
                    "<GoodsNoVer>" + sGoodsNoVer + "</GoodsNoVer>" +
                    "<GoodsTaxNo>" + sGoodsTaxNo + "</GoodsTaxNo>" +
                    "<TaxPre>" + sTaxPre + "</TaxPre>" +
                    "<TaxPreCon>" + sTaxPreCon + "</TaxPreCon>" +
                    "<ZeroTax>" + sZeroTax + "</ZeroTax>" +
                    "<CropGoodsNo>" + sCropGoodsNo + "</CropGoodsNo>" +
                    "<TaxDeduction></TaxDeduction>" +
                    "</GoodsNo>" +
                    "</INPUT>" +
            "</FPXT>";

            return sDataXml;
        }
        private string BatchUpLoad(string sDataXml)
        {
            string sResultXml = "";

            sResultXml =
                "<?xml version=\"1.0\" encoding=\"GBK\"?>" +
                "<FPXT_COM_INPUT>" +
                "<ID>1100</ID> " +
                "<DATA>" + Base64.EncodeBase64(sDataXml) + "</DATA> " +
                "</FPXT_COM_INPUT>";

            return sResultXml;
        }
        private int fVaildBatchUp(string sXml)
        {
            string sCode = "";
            string sMess = "";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sXml);

            foreach (XmlNode node in doc.SelectNodes("FPXT_COM_OUTPUT"))
            {
                sCode = node.ChildNodes[1].FirstChild.Value.ToString();
                sMess = node.ChildNodes[2].FirstChild.Value.ToString();
            }

            if (sCode != "0000")
            {
                sRetMsg = "校验税收分类编码错误，错误代码：" + sCode + "--" + sMess + "!";
                return 1;

            }
            return 0;

        }
        public int InvoiceBill()
        {

            _InvoiceRetInfo.sRetInfoTypeCode = "0";
            _InvoiceRetInfo.sRetInfoNumber = 0;

            MyTax.InfoKind = iInvType;

            iResult = 0;

            if (dtInvocieDetail.Rows.Count >0)
            {
                if (InvoiceBillHeader() == 1) return 1;
                if (InvoiceBillDetail() == 1) return 1;

                int iRetCode = 0;
                MyTax.Invoice();
                iRetCode = MyTax.RetCode;

                if (iRetCode == 4011)
                {
                    _InvoiceRetInfo.dRetInfoAmount = MyTax.InfoAmount;
                    _InvoiceRetInfo.dRetInfoTaxAmount = MyTax.InfoTaxAmount;
                    _InvoiceRetInfo.dRetInfoInvDate = MyTax.InfoDate;
                    _InvoiceRetInfo.dRetInfoMonth = MyTax.InfoMonth;
                    _InvoiceRetInfo.sRetInfoTypeCode = MyTax.InfoTypeCode;
                    _InvoiceRetInfo.sRetInfoNumber = MyTax.InfoNumber;
                    _InvoiceRetInfo.sRetGoodsListFlag = MyTax.GoodsListFlag;
                }
                else
                {
                    sRetMsg = iRetCode.ToString() + " 开票失败";

                    if (iRetCode == 4001) { sRetMsg = iRetCode.ToString() + " 传入发票数据不合法"; return 1; }
                    if (iRetCode == 4002) { sRetMsg = iRetCode.ToString() + " 开票前金税卡状态错"; return 1; }
                    if (iRetCode == 4003) { sRetMsg = iRetCode.ToString() + " 金税卡开票调用错误"; return 1; }
                    if (iRetCode == 4001) { sRetMsg = iRetCode.ToString() + " 开票后取金税卡状态错"; return 1; }
                    if (iRetCode == 4012) { sRetMsg = iRetCode.ToString() + " 开票失败"; return 1; }
                    if (iRetCode == 4013) { sRetMsg = iRetCode.ToString() + " 所开发票已作废"; return 1; }

                    iResult = 1;                    
                }

            }
            return iResult;
        }
        private void InvPrint(Int32 iInfoNumber, string sInfoTypeCode,short iGoodsListFlag = 0, short iInfoShowPrtDlg = 1)
        {
            iResult = 0;

            MyTax.InfoTypeCode = sInfoTypeCode;
            MyTax.InfoNumber = iInfoNumber;
            MyTax.GoodsListFlag = iGoodsListFlag;
            MyTax.InfoShowPrtDlg = iInfoShowPrtDlg;

            MyTax.PrintInv();

            iResult = MyTax.RetCode;

            if ((MyTax.RetCode != 5011) && (MyTax.RetCode != 5001) && (MyTax.RetCode != 5012) && (MyTax.RetCode != 5013))
            { sRetMsg = MyTax.RetCode.ToString() + " 打印失败,其他原因！"; iResult = 1; }

            if (MyTax.RetCode == 5001) { sRetMsg = MyTax.RetCode.ToString() + " 未找到发票或清单"; iResult = 1; }
            if (MyTax.RetCode == 5011) { sRetMsg = MyTax.RetCode.ToString() + " 打印成功"; iResult = 0; }
            if (MyTax.RetCode == 5012) { sRetMsg = MyTax.RetCode.ToString() + " 未打印"; iResult = 0; }
            if (MyTax.RetCode == 5013) { sRetMsg = MyTax.RetCode.ToString() + " 打印失败"; iResult = 1; }

        }
        private int InvCancel(Int32 iInfoNumber, string sInfoTypeCode)
        {
            MyTax.InfoTypeCode = sInfoTypeCode;
            MyTax.InfoNumber = iInfoNumber;

            MyTax.CancelInv();
            iResult = MyTax.RetCode;

            if (iResult == 6001) { sRetMsg = MyTax.RetCode.ToString() + " 当月发票库未找到该发票"; iResult = 1; return 1; }
            if (iResult == 6002) { sRetMsg = MyTax.RetCode.ToString() + " 该发票已作废"; iResult = 1; return 1; }
            if (iResult == 6011) {sRetMsg = MyTax.RetCode.ToString() + " 作废成功"; return 1; }
            if (iResult == 6012) { sRetMsg = MyTax.RetCode.ToString() + " 未作废"; iResult = 1; return 1; }
            if (iResult == 6013) { sRetMsg = MyTax.RetCode.ToString() + " 作废失败"; iResult = 1; return 1; }

            return 0;

        }

        private int InvCloseCard()
        {
            int iResult = 0;
            string sResultMsg = "";

            MyTax.CloseCard();

            iResult = MyTax.RetCode;
            sResultMsg = MyTax.RetMsg;

            if (iResult != 9000)
            {
                iResult = 1;
                sRetMsg = "金税卡关闭失败！" + sResultMsg;
                return 1;
            }
            return 0;
        }
    }
    class Base64
    {
        public static string EncodeBase64(Encoding encode, string source)
        {
            string enString = "";
            byte[] bytes = encode.GetBytes(source);
            try
            {
                enString = Convert.ToBase64String(bytes);
            }
            catch
            {
                enString = source;
            }
            return enString;
        }

        public static string EncodeBase64(string source)
        {
            return EncodeBase64(Encoding.UTF8, source);
        }
        public static string DecodeBase64(Encoding encode, string result)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encode.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        public static string DecodeBase64(string result)
        {
            return DecodeBase64(Encoding.UTF8, result);
        }
    }
}