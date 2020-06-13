using System;
using System.Collections.Generic;
using System.Windows;


namespace FCSChart.Axis
{
    /// <summary>
    /// 线性数字分割
    /// </summary>
    public class LinearNumberAxis : IAxis
    {
        static LinearNumberAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinearNumberAxis), new FrameworkPropertyMetadata(typeof(LinearNumberAxis)));
        }
        public LinearNumberAxis() { this.AxisName = "Line"; }
        #region function
        /// <summary>
        /// 实际值转化成坐标值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double ValueToAxisValue(double value)
        {
            return value;
        }
        /// <summary>
        /// 坐标值转化成实际值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double AxisValueToValue(double axisvalue)
        {
            return axisvalue;
        }
        /// <summary>
        /// 重新绘制坐标间隔
        /// </summary>
        internal override void Drawing()
        {
            if (!this.IsLoaded) return;
            var range = MaxAxis - MinAxis;
            int count = 0;
            switch (XYType)
            {
                case AxisType.X:
                    if (range < 1)
                    {
                        var temp = range;
                        int i = 0;
                        while (temp < 1)
                        {
                            temp *= 10;
                            i++;
                        }
                        count = Convert.ToInt32(this.ActualWidth / (Max.ToString(string.Concat("N", i)).Length * this.FontSize * 1.5));
                    }
                    else
                        count = Convert.ToInt32(this.ActualWidth / (Max.ToString("N0").Length * this.FontSize));
                    break;
                case AxisType.Y:
                    count = Convert.ToInt32(this.ActualHeight / (this.FontSize * 2));
                    break;
                default:
                    break;
            }
            var subrange = Math.Pow(10, Math.Floor(Math.Log(range / count) / Math.Log(10)));
            var residual = range / count / subrange;
            if (residual > 5)
                subrange = 10 * subrange;
            else if (residual > 2)
                subrange = 5 * subrange;
            else if (residual > 1)
                subrange = 2 * subrange;
            List<LinearNumbersScaleData> items = new List<LinearNumbersScaleData>();
            var start = Min % subrange != 0d ? (Min - Min % subrange) : Min;
            double actualLength = XYType == AxisType.X ? this.ActualWidth : this.ActualHeight;
            for (int i = 0; i < range / subrange; i++)
            {
                var value = Convert.ToDecimal(start) + Convert.ToDecimal(i * subrange);
                var valued = Convert.ToDouble(value);
                if (valued < Min || valued > Max) continue;
                var item = new LinearNumbersScaleData() { Value = value.ToShortString(), Location = (valued - Min) * actualLength / range };
                item.Location = (valued - Min) * actualLength / range;
                items.Add(item);
            }
            ItemsSource = items;
        }

        #endregion
    }

    internal class LinearNumbersScaleData : IScaleData
    {

    }
}
