using FCSChart.Graphical;
using System;
using System.Windows.Media;

namespace FCSChart
{
    public static class Helper
    {
        static readonly Random random = new Random();
        /// <summary>
        /// 获取新的颜色
        /// </summary>
        /// <returns></returns>
        public static Color GetNewColor(byte a = 0xff)
        {
            byte r = Convert.ToByte(random.Next(255));
            byte g = Convert.ToByte(random.Next(255));
            byte b = Convert.ToByte(random.Next(255));
            return new Color() { A = a, R = r, G = g, B = b };
        }
        /// <summary>
        /// 获取新的笔刷
        /// </summary>
        /// <returns></returns>
        public static SolidColorBrush GetNewSolidColorBrush(byte a = 0xff)
        {
            return new SolidColorBrush(GetNewColor(a));
        }
        /// <summary>
        /// 获取渐变颜色
        /// </summary>
        /// <param name="i">当前梯度值</param>
        /// <param name="max">最大梯度量</param>
        /// <returns></returns>
        public static Color GetGradientColor(long i, long max)
        {
            byte tempR = Convert.ToByte(255 * i / max);
            byte tempG = 0x90;
            byte tempB = 0x00;
            return new Color() { A = 0xff, R = tempR, G = tempG, B = tempB };
        }
        /// <summary>
        /// 获取透明渐变黑色
        /// </summary>
        /// <param name="i"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Color GetGradientBlack(long i, long max)
        {
            var temp = Convert.ToByte(255 * i / max);
            return new Color() { A = temp, R = 0x00, G = 0x00, B = 0x00 };
        }
        /// <summary>
        /// 获取某个对象的double属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static double GetObjectPropertyDoubleValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName)) return default;
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) return default;
            return Convert.ToDouble(property.GetValue(obj));
        }
        /// <summary>
        /// decimal转字符串
        /// 数字过大时用科学计数法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToShortString(this decimal value)
        {
            int i = 0;
            var temp = value;
            while (Math.Abs(temp / 10) >= 1)
            {
                temp /= 10;
                i++;
            }
            var tempStr = string.Concat(temp, "E+", i);
            var tempStr1 = value.ToString();
            return tempStr.Length > tempStr1.Length ? tempStr1 : tempStr;
        }


        public static int GraphicalIndex = 0;
        /// <summary>
        /// 创建新门的名称
        /// </summary>
        /// <param name="graphical"></param>
        /// <returns></returns>
        public static string CreateNewGraphicalNameFaction(BaseGraphical graphical)
        {
            GraphicalIndex++;
            return string.Format("G{0}", GraphicalIndex);
        }
    }
}
