using System;
using System.Collections.Generic;
using System.Windows;


namespace FCSChart.Axis
{
    /// <summary>
    /// 对数数字分割
    /// </summary>
    public class LogarithmicNumberAxis : IAxis
    {
        static LogarithmicNumberAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LogarithmicNumberAxis), new FrameworkPropertyMetadata(typeof(LogarithmicNumberAxis)));
        }

        public LogarithmicNumberAxis()
        {
            this.AxisName = "Log";
            this.Min = 1d;
        }

        #region function
        /// <summary>
        /// 实际值转化成坐标值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double ValueToAxisValue(double value)
        {
            if (double.IsNaN(value)) return value;
            else if (double.IsPositiveInfinity(value)) return double.MaxValue;
            else if (value <= 0) return double.NegativeInfinity;
            return Math.Log10(value);
        }
        /// <summary>
        /// 坐标值转化成实际值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double AxisValueToValue(double axisvalue)
        {
            if (double.IsNaN(axisvalue)) return axisvalue;
            else if (double.IsPositiveInfinity(axisvalue)) return double.MaxValue;
            else if (double.IsNegativeInfinity(axisvalue)) return 0d;
            else return Math.Pow(10, axisvalue);
        }
        /// <summary>
        /// 重新绘制坐标间隔
        /// </summary>
        public override void Drawing()
        {
            if (!this.IsLoaded) return;
            if (double.IsInfinity(MaxAxis)) { MaxAxis = 5; Max = 100000d; }
            if (double.IsInfinity(MinAxis)) { MinAxis = 0; Min = 1; }
            var range = MaxAxis - MinAxis;

            var temp = MaxAxis % 1;
            var showedTempMax = temp > 0 ? Convert.ToInt32(MaxAxis - temp) + 1 : Convert.ToInt32(MaxAxis);
            temp = MinAxis % 1;
            var showedTempMin = temp > 0 ? Convert.ToInt32(MinAxis - temp) - 1 : Convert.ToInt32(MinAxis);

            double actualLength = XYType == AxisType.X ? this.ActualWidth : this.ActualHeight;
            List<LogarithmicNumbersScaleData> items = new List<LogarithmicNumbersScaleData>();
            for (int i = showedTempMin; i <= showedTempMax; i++)
            {
                var item = new LogarithmicNumbersScaleData() { Power = i, Value = 10, Location = (i - MinAxis) * actualLength / range };
                if (item.Location >= 0 && item.Location <= actualLength)
                    items.Add(item);
                for (int j = 1; j < 9; j++)
                {
                    var m = ValueToAxisValue(AxisValueToValue(i) * (1 + j));
                    var mscale = new LogarithmicNumbersScaleData() { Location = (m - MinAxis) * actualLength / range };
                    if (mscale.Location >= 0 && mscale.Location <= actualLength)
                        items.Add(mscale);
                }
            }
            ItemsSource = items;
        }


        #endregion
    }

    internal class LogarithmicNumbersScaleData : IScaleData
    {
        private int powerFontSize = 8;
        /// <summary>
        /// 幂的字体大小
        /// </summary>
        public int PowerFontSize
        {
            get { return powerFontSize; }
            set { powerFontSize = value; OnPropertyChanged("PowerFontSize"); }
        }

        private double? power = null;
        /// <summary>
        /// 幂
        /// </summary>
        public double? Power
        {
            get { return power; }
            set { power = value; OnPropertyChanged("Power"); }
        }

    }
}
