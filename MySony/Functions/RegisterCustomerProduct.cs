using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MySony.Models;
using MySony.SonyServcies;
//using MySony.vn.com.sony.mysony;
using MySony.ViewModels;
using System.Globalization;


namespace MySony.Functions
{
    public class RegisterCustomerProduct
    {
        public static ArrayList LstSerialRegister  = new ArrayList();

        /// <summary>
        /// Function Register Customer Product
        /// </summary>
        /// <param name="db"></param>
        /// <param name="idCustomer"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <param name="serialNumber"></param>
        /// <param name="timePuCharsedId"></param>
        /// <param name="cityId"></param>
        /// <param name="shopId"></param>
        /// <returns> 
        /// 0: Have Exception
        /// 1: save succfully
        /// 2: Not Exist Serial with category and product           
        /// </returns>
        public static int RegisterProduct(MySonyEntities db, int idCustomer, int categoryId, int productId, string serialNumber, string timePuCharsedId, int shopId, int cityId)
        {
          
            int numberStatus;
            DateTime? warrantyEnd = null;
            if (DateTime.Compare(DateTime.Parse(timePuCharsedId), DateTime.Now) > 0)
            {
                numberStatus = 5; // thời gian mua ko hợp lệ
                return numberStatus;
            }
            try
            {
                var dSerial = GetSerial(db, categoryId, productId, serialNumber);
                var countDate = 0;
                if (dSerial != null)
                {
                    // Kiem tra product da duoc dang ki hay chua
                    var isCustomerProduct = CheckRegProduct(db,productId,serialNumber);// CheckCustomerProduct(db, idCustomer, productId, serialNumber);
                    if (isCustomerProduct)
                    {
                        numberStatus = 3;
                        return numberStatus;
                    }

                    var numberWarranty = (int)(dSerial.period * 12);

                    if (dSerial.manufacturingdate != null)
                    {
                        countDate = Cal_Warranty_Month((DateTime)dSerial.manufacturingdate, Convert.ToDateTime(timePuCharsedId));
                    }
                    DateTime dateWarranty;
                    if (countDate > 0)
                    {
                        dateWarranty = Convert.ToDateTime(timePuCharsedId).AddMonths(numberWarranty);
                    }
                    else
                    {
                        dateWarranty = Convert.ToDateTime(dSerial.manufacturingdate).AddMonths(numberWarranty);
                    }

                    if (dSerial.manufacturingdate != null)
                    {
                        var value = GetWarrantyTime(db, serialNumber, categoryId, DateTime.Parse(timePuCharsedId), dSerial);

                        if (!value.Equals(string.Empty))
                        {
                            warrantyEnd = DateTime.Parse(value);
                        }
                    }
                    var custPro = new order
                    {
                        customer_id = idCustomer,
                        product_id = productId,
                        serial = serialNumber,
                        regdate = DateTime.Now,
                        WarrantyStart = dateWarranty,
                        shop_id = shopId,
                        city_id = cityId,
                        Created = DateTime.Now,
                        date = DateTime.Parse(timePuCharsedId) ,
                        modifieddate = DateTime.Now,
                        status_id = 1,
                        catagory_id = categoryId,
                        WarrantyEnd = warrantyEnd
                        //city_id
                    };
                    db.orders.Add(custPro);
                    db.SaveChanges();
                    numberStatus = 1;
                    LstSerialRegister.Add(serialNumber);
                }
                else
                {
                    numberStatus = 2;
                   
                }           
            }
            catch (Exception ex)
            {                 
                numberStatus = 0;
            }
            return numberStatus;
        }

        public static serial GetSerial(MySonyEntities db, int categoryId, int productId, string serialnumber)
        {
            serial dSerial = null;
            try
            {
                // check XPERIA 
                if (categoryId == Constant.IdProductMobileXp)
                {
                    dSerial = GetModelXperiaService(db, serialnumber);
                }
                else
                {
                    var product = db.products.FirstOrDefault(x => x.category_id == categoryId && x.product_id == productId);
                    if (product != null)
                    {
                        var serial = db.serials.FirstOrDefault(x => x.modelname == product.name && x.serialnumber == serialnumber);
                        if (serial != null)
                        {
                            if (serialnumber.Equals(serial.serialnumber))
                            {
                                dSerial = serial;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return dSerial;
        }

        public static bool CheckCustomerProduct(MySonyEntities db, int idCustomer, int productId, string serialNumber)
        {
            var cusproduct = db.orders.FirstOrDefault(x => x.customer_id == idCustomer && x.product_id == productId && x.serial == serialNumber);
            if (cusproduct == null)
            {
                return false;
            }

            return true;
        }

        public static bool CheckRegProduct(MySonyEntities db, int productId, string serialNumber)
        {
            var cusproduct = db.orders.FirstOrDefault(x => x.product_id == productId && x.serial == serialNumber && x.status_id == 1);
            if (cusproduct == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Kiểm tra đăng ký trong khoảng 6 tháng từ ngày sx hay sau
        /// </summary>
        /// <param name="manufacturingdate"></param>
        /// <param name="dateRegister"></param>
        /// <returns>0: sau khoảng 6 tháng
        /// 6: khoảng 6 tháng
        /// </returns>
        public static int Cal_Warranty_Month(DateTime manufacturingdate , DateTime dateRegister)
        {
            int compare =  0;
            compare = DateTime.Compare(manufacturingdate, dateRegister);
            if (compare > 0)
            {
                return 0;
            }

            var dRegister = manufacturingdate.AddMonths(Constant.MounthWarranty);
            var result = DateTime.Compare(dRegister, dateRegister);
            if (result >= 0)
            {
                return Constant.MounthWarranty;
            }
            return 0;
        }


        public static bool CheckSerialExist(MySonyEntities db, int categoryId, int productId, string serialnumber)
        {
            var dSerial = false;
            try
            {
                var product = db.products.FirstOrDefault(x => x.category_id == categoryId && x.product_id == productId);
                if (product != null)
                {
                    var serial = db.serials.FirstOrDefault(x => x.productcode == product.productcode);
                    if (serial != null)
                    {
                        if (serialnumber.Equals(serial.serialnumber) )
                        {
                            dSerial = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                dSerial = false;
            }
            return dSerial;
        }
 
        //public static bool CheckActiveXperial(MySonyEntities db  , string serialNumber)
        //{
        //    var b = false;
        //    try
        //    {
        //        var checkIme = (from cat in db.categories
        //                         join prod in db.products on cat.category_id equals prod.category_id
        //                         join seri in db.serials on prod.productcode equals seri.productcode
        //                         join cuspro in db.orders on seri.serial_id equals cuspro.SerialID
        //                         where (seri.serialnumber.Equals(serialNumber) && cat.name.Contains(Constant.Mobile_XPERIA))
        //                         select new { 
        //                             CategoryName = cat.name, 
        //                             CategoryImage = cat.modelImage, 
        //                             ProductName = prod.name, 
        //                             Serial = seri.serialnumber  , 
        //                             Status=seri.status_id 
        //                         }).ToList();

        //        if (checkIme.Count > 0)
        //        {
        //            foreach (var obj in checkIme)
        //            {
        //                if (obj.Status == Constant.NotActive)
        //                {
        //                    b = false;
        //                }
                         
        //            }
        //        }
        //        else
        //        {
        //            b = true;
        //        }
        //    }
        //    catch
        //    {
        //        b = true;
        //    }
        //    return b;
        //}

        public static bool CheckXperial(MySonyEntities db, string serialNumber)
        {
            var b = false;
            try
            {
                var checkIme = (from cat in db.categories
                                join prod in db.products on cat.category_id equals prod.category_id
                                //join seri in db.serials on prod.productcode equals seri.productcode
                                join cuspro in db.orders on prod.product_id equals cuspro.product_id
                                where (cat.name.Contains(Constant.Mobile_XPERIA))
                                select new
                                {
                                    CategoryName = cat.name, 
                                    CategoryImage = cat.modelImage, 
                                    ProductName = prod.name, 
                                    Serial = cuspro.serial,
                                    Status = cuspro.status_id
                                }).ToList();

                if (checkIme.Count > 0)
                {
                    b = true;
                }
                else
                {
                    b = false;
                }
            }
            catch
            {
                b = false;
            }
            return b;
        }

        public static string GetTimeWarrantyXperia(MySonyEntities db, MySonyServiceClient sv, string serialNumber
                                                        , int categoryId, DateTime? createCusProdDate, serial dSerial)
        {
            string timeWarranty;
            try
            {
                //if (CheckXperial(db, serialNumber))
                if (categoryId == Constant.IdProductMobileXp)
                {
                    var obj = sv.GetMobile(serialNumber);
                    if (obj == null)
                    {
                        timeWarranty = string.Empty;
                    }
                    else if (obj.ModelName == null)
                    {
                        timeWarranty = string.Empty;
                    }
                    else if (obj.Status != null)
                    {
                        timeWarranty = string.Empty;
                    }
                    else
                    {
                        timeWarranty = Convert.ToDateTime(obj.ExpiredDate).ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    var numberWarranty = 0;
                    var countDate = 0;
                     
                    numberWarranty = (int) (dSerial.period*12);
                    if (dSerial.manufacturingdate != null)
                    {
                        countDate = Cal_Warranty_Month(Convert.ToDateTime(dSerial.manufacturingdate), Convert.ToDateTime(createCusProdDate));
                    }
                    else
                    {
                        timeWarranty = string.Empty;
                        return timeWarranty;
                    }
                    if (countDate > 0) // trong khoảng 6 tháng
                    {
                        timeWarranty = Convert.ToDateTime(createCusProdDate).AddMonths(numberWarranty).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        timeWarranty = Convert.ToDateTime(dSerial.manufacturingdate).AddMonths(numberWarranty).ToString("dd/MM/yyyy");
                    }                    
                }
            }
            catch (Exception)
            {
                timeWarranty = string.Empty;
            }
            return timeWarranty;
        }

        public static List<CustomerProduct> ListImage( MySonyEntities db,int userId)
        {
            using (var sv = new MySonyServiceClient())
            {
                if (sv.ClientCredentials != null)
                {
                    sv.ClientCredentials.UserName.UserName = Constant.UserNameService;
                    sv.ClientCredentials.UserName.Password = Constant.PasswordService;
                }

                List<CustomerProduct> listUserImage = (from cuspro in db.orders
                                                       join pro in db.products on cuspro.product_id equals pro.product_id
                                                       join cate in db.categories on pro.category_id equals cate.category_id
                                                       join sh in db.shops on cuspro.shop_id equals sh.shop_id
                                                       //join seri in db.serials on cuspro.serial equals seri.serialnumber
                                                       where cuspro.customer_id == userId
                                                       select new CustomerProduct
                                                       {
                                                           CategoryName = cate.name,
                                                           CategoryImage = cate.modelImage,
                                                           ProductName = pro.name,
                                                           Serial = cuspro.serial,
                                                           TimePucharsed = cuspro.regdate,
                                                           ShopName = sh.name,
                                                           TimeWarranty = cuspro.WarrantyEnd
                                                       }).ToList();
                                                       
                return listUserImage;
            }
        }
        /// <summary>
        /// Lấy thời hạn bảo hành của sản phẩm
        /// </summary>
        /// <param name="db"></param>
        /// <param name="serialNumber"></param>
        /// <param name="categoryId"></param>
        /// <param name="timePucharsed"></param>
        /// <param name="dSerial"></param>
        /// <returns></returns>
        public static string GetWarrantyTime(MySonyEntities db, string serialNumber, int categoryId, DateTime? timePucharsed,serial dSerial)// DateTime manufacturingdate, DateTime dateWarranty)
        {
            string time;
            try
            {
                using (var sv = new MySonyServiceClient())
                {
                    if (sv.ClientCredentials != null)
                    {
                        sv.ClientCredentials.UserName.UserName = Constant.UserNameService;
                        sv.ClientCredentials.UserName.Password = Constant.PasswordService;
                    }
                    time = GetTimeWarrantyXperia(db, sv, serialNumber, categoryId,
                        timePucharsed, dSerial);
                }

            }
            catch (Exception)
            {

                time = string.Empty;
            }
            return time;
        }

        public static string Message(MySonyEntities db, int userId )
        {
            var message = string.Empty; 
            var lstShops = ListImage(db, userId);
            bool showWarranty = Convert.ToBoolean(ConfigurationManager.AppSettings["show_warranty"]);
            var listProduct = LstSerialRegister;

            if (listProduct.Count > 0)
            {
                foreach (var obj in listProduct)
                {

                    var status = lstShops.FirstOrDefault(x => x.Serial.Trim() == obj.ToString());
                    if (status != null)
                    {                        
                        if (showWarranty)
                        {
                            message += "Tên sản phẩm: <b>" + status.ProductName + "</b><br/>"
                             + "Số seri: <b>" + status.Serial + "</b><br/>"
                             + "Ngày hết hạn BH: <b>" + ((status.TimeWarranty != null) ? Convert.ToDateTime(status.TimeWarranty).ToString("dd/MM/yyyy") : "") + "</b><br/>"
                             + "<hr/>";                            
                        }
                        else
                        {
                            message += "Tên sản phẩm: <b>" + status.ProductName + "</b><br/>"
                             + "Số seri: <b>" + status.Serial + "</b><br/>"                             
                             + "<hr/>";
                            
                        }
                        //message += "Tên sản phẩm: <b>" + status.ProductName + "</b><br/>"
                        //     + "Số seri: <b>" + status.Serial + "</b><br/>"
                        //     + "Ngày hết hạn BH: <b>" + ((status.TimeWarranty != null) ? Convert.ToDateTime(status.TimeWarranty).ToString("dd/MM/yyyy") : "") + "</b><br/>"
                        //     + "<hr/>";
                        
                    }
                     
                }
            }
            return message;
        }

        public static ArrayList ListXperia(MySonyEntities db, int cartegoryID)
        {
            var list = new ArrayList();
          
            using (var sv = new MySonyServiceClient())
            {
                if (sv.ClientCredentials != null)
                {
                    sv.ClientCredentials.UserName.UserName = Constant.UserNameService;
                    sv.ClientCredentials.UserName.Password = Constant.PasswordService;

                }
               var obj= sv.GetProducts(cartegoryID);
                if (obj.Length > 0)
                {
                    foreach (var pro in obj)
                    {
                        list.Add(new { id=pro.ModelId, name=pro.ModelName });
                    }
                }               
               return list;
            }
        }

        public static serial GetModelXperiaService(MySonyEntities db, string serialNumber)
        {
            var dSerial = new serial();
            using (var sv = new MySonyServiceClient())
            {
                if (sv.ClientCredentials != null)
                {
                    sv.ClientCredentials.UserName.UserName = Constant.UserNameService;
                    sv.ClientCredentials.UserName.Password = Constant.PasswordService;
                }
                var objlist = sv.GetMobile(serialNumber);
                dSerial.manufacturingdate = objlist.ManufacturingDate;
                dSerial.period = objlist.Period;
                dSerial.serialnumber = objlist.SerialNumber;                
                return dSerial;
            }
        }

        /// <summary>
        /// Kiểm tra chính hãng
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="serialNumber"></param>
        /// <returns>
        /// -1: Have exception
        /// 1:  Sản pham chinh hang
        /// 2: Không tồn tại sản phẩm
        /// 3: ModelName: tên sản phầm không đúng
        /// 4: Status: Chưa kích hoặt. Đề nghị kích hoạt
        /// 5: Sản phẩm không chính hãng
        ///  
        /// </returns>
        public static int CheckProdcutWebserive(int categoryId, string serialNumber , string productName = "")
        {
            int status = 1;
            try
            {
                using (var sv = new MySonyServiceClient())
                {
                    if (sv.ClientCredentials != null)
                    {
                        sv.ClientCredentials.UserName.UserName = Constant.UserNameService;
                        sv.ClientCredentials.UserName.Password = Constant.PasswordService;
                    }
                    
                    if (categoryId == Constant.IdProductMobileXp)
                    {
                        var lstMobile = sv.GetMobile(serialNumber);
                        if (lstMobile == null)
                        {
                            status = 2;
                        }
                        else if (!productName.Equals(lstMobile.ModelName) )
                        {
                            status = 3;
                        }
                        else if (Constant.XperiaNotActive.Equals(lstMobile.Status))
                        {
                            status = 4;
                        } 
                    }
                    else
                    {
                        var lst = sv.GetUnit(productName, serialNumber);
                        if (lst == null)
                        {
                            status = 5;
                        }
                    }
                }
            }
            catch (Exception)
            {

                 status  = -1;
            }
            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cus"></param>
        /// <returns>true => reject</returns>
        public static bool CheckValidate(customer cus)
        {
            if (Common.ValidateString(cus.address))
            {
                return true;
            }
            if (Common.ValidateString(cus.lastname))
            {
                return true;
            }
            if (Common.ValidateString(cus.firstname))
            {
                return true;
            }    
            return false;
        }

        /// <summary>
        /// when customer register , the product not active , send email to customer service
        /// </summary>
        /// <param name="cus"></param>
        /// <returns>true => reject</returns>
        public static void sendEmail()
        {
            
        }
    }
}