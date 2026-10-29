using System.Globalization;

namespace TravelHub.Data
{
    public static class Media
    {

        public static string Destination(int id)
        {
            switch (id)
            {
                case 1:
                    return "/Content/images/destinations/danang1.jpg";
                case 2:
                    return "/Content/images/destinations/dalat1.jpg";
                case 3:
                    return "/Content/images/destinations/nhatrang1.jpg";
                case 4:
                    return "/Content/images/destinations/phuquoc1.jpg";
                case 5:
                    return "/Content/images/destinations/vungtau1.jpg";
                case 6:
                    return "/Content/images/destinations/hanoi.jpg";
                case 7:
                    return "/Content/images/destinations/hcm.jpg";
                case 8:
                    return "/Content/images/destinations/hoian.jpg";
                case 9:
                    return "/Content/images/destinations/hue.jpg";
                case 10:
                    return "/Content/images/destinations/sapa.jpg";
                case 11:
                    return "/Content/images/destinations/halong1.jpg";
                case 12:
                    return "/Content/images/destinations/ninhbinh.jpg";
                default:
                    return "/Content/images/destinations/danang1.jpg";
            }
        }


        public static string Hotel(int id)
        {
            switch (id)
            {
                // Khách sạn 1 - Đà Nẵng
                case 1:
                    return "/Content/images/hotels/seaview-danang.jpg";

                // Khách sạn 2 - Đà Lạt
                case 2:
                    return "/Content/images/hotels/dalat-palace.jpg";

                // Khách sạn 3 - Nha Trang
                case 3:
                    return "/Content/images/hotels/aquamarine-nhatrang.jpg";

                // Khách sạn 4 - Phú Quốc
                case 4:
                    return "/Content/images/hotels/phuquoc-sunsethill.jpg";

                // Khách sạn 5 - Vũng Tàu
                case 5:
                    return "/Content/images/hotels/vungtau-annatabeach.jpg";

                // Nếu không tìm thấy ID
                default:
                    return "/Content/images/hotels/seaview-danang.jpg";
            }
        }

        public static string Money(decimal value)
        {
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}₫", value);
        }

        public static string Stars(int count)
        {
            if (count < 0) count = 0;
            if (count > 5) count = 5;
            return new string('★', count) + new string('☆', 5 - count);
        }
    }
}
