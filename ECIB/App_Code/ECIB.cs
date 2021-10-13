using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;

/// <summary>
/// Summary description for ECIB
/// </summary>
[WebService(Namespace = "http://ECIB.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class ECIB : System.Web.Services.WebService
{
    Escore_FBLEntities db = new Escore_FBLEntities();

    public ECIB()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public XmlDocument ECIBXML(string Authorization = "", string NIC = "")
    {
        XmlDocument doc = new XmlDocument();

        if (string.IsNullOrWhiteSpace(Authorization))
        {
            String errorKey =
          @"<message>
                <Error message=""Authorization key is require ""/>
            </message>";
            doc.LoadXml(errorKey);
            return doc;
        }

        var secretKey = Encryption.Encrypt(Authorization);
        var secretKeyGenerator = db.SecretKeyGenerators.Where(x => x.SecretKey == secretKey).FirstOrDefault();
        if (secretKeyGenerator == null)
        {
            String errorKey =
           @"<message>
                <Error message=""wrong authorization key ""/>
            </message>";
            doc.LoadXml(errorKey);
            return doc;
        }

        if (string.IsNullOrWhiteSpace(NIC))
        {
            String errorKey =
          @"<message>
                <Error message=""NIC is require ""/>
            </message>";
            doc.LoadXml(errorKey);
            return doc;
        }

        var XML = this.ExecuteDataset(NIC);
        if (XML == "<ecib />")
        {
            String noRecord =
            @"<message>
                <Error message=""No record found ""/>
            </message>";
            doc.LoadXml(noRecord);
            return doc;
        }

        doc.LoadXml(XML);
        return doc;
    }

    public string ExecuteDataset(string NIC)
    {
        var dataSet = new DataSet("ecib");

        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionFBL"].ToString());
        SqlCommand cmd = new SqlCommand("sp_ECIBData", con);
        cmd.CommandType = CommandType.StoredProcedure;
        SqlParameter param = new SqlParameter("@nic", NIC);
        cmd.Parameters.Add(param);
        SqlDataAdapter sda = new SqlDataAdapter();
        sda.SelectCommand = cmd;
        sda.Fill(dataSet);
        string strtablesName = "Bureau,Product,CreditEnquires,LoanCollateral,CreditHistory,CreditDetails,ConsumerProfile,CreditHistoryHeader";
        var tablesName = strtablesName.Split(',');
        for (int i = 0; i < tablesName.Length; i++)
        {
            dataSet.Tables[i].TableName = tablesName[i];
        }

        return dataSet.GetXml();
    }

}
