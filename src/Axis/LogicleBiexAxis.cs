using System;
using System.Collections.Generic;
using System.Windows;


namespace FCSChart.Axis
{
    /// <summary>
    /// 对数数字分割-带负数
    /// </summary>
    public class LogicleBiexAxis : IAxis
    {
        static LogicleBiexAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LogicleBiexAxis), new FrameworkPropertyMetadata(typeof(LogicleBiexAxis)));
        }

        public LogicleBiexAxis() { this.AxisName = "LogBiex"; }
        #region function
        /// <summary>
        /// 实际值转化成坐标值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double ValueToAxisValue(double value)
        {
            if (value >= 10) return Math.Log10(value);
            else if (value <= -10) return -Math.Log10(-value);
            else return value / 10;
        }
        /// <summary>
        /// 坐标值转化成实际值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double AxisValueToValue(double axisvalue)
        {
            if (axisvalue >= 1) return Math.Pow(10, axisvalue);
            else if (axisvalue <= -1) return -Math.Pow(10, -axisvalue);
            else return axisvalue * 10;
        }
        /// <summary>
        /// 重新绘制坐标间隔
        /// </summary>
        public override void Drawing()
        {
            if (!this.IsLoaded) return;
            var range = MaxAxis - MinAxis;
            var temp = MaxAxis % 1;
            var showedTempMax = temp > 0 ? Convert.ToInt32(MaxAxis - temp) + 1 : Convert.ToInt32(MaxAxis);
            temp = MinAxis % 1;
            var showedTempMin = temp > 0 ? Convert.ToInt32(MinAxis - temp) - 1 : Convert.ToInt32(MinAxis);
            double actualLength = XYType == AxisType.X ? this.ActualWidth : this.ActualHeight;
            List<LogicleBiexScaleData> items = new List<LogicleBiexScaleData>();
            for (int i = showedTempMin; i <= showedTempMax; i++)
            {
                var item = new LogicleBiexScaleData() { Power = i < 0 ? -i : i, Value = i < 0 ? -10 : 10, Location = (i - MinAxis) * actualLength / range };
                if (item.Location >= 0 && item.Location <= actualLength)
                    items.Add(item);
                if (i != 0)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        var m = ValueToAxisValue(AxisValueToValue(i) * (1 + j));
                        var mscale = new LogicleBiexScaleData() { Location = (m - MinAxis) * actualLength / range };
                        if (mscale.Location >= 0 && mscale.Location <= actualLength)
                            items.Add(mscale);
                    }
                }
                else
                {
                    for (int j = -9; j <= 9; j++)
                    {
                        var mscale = new LogicleBiexScaleData() { Location = (j / 10d - MinAxis) * actualLength / range };
                        if (mscale.Location >= 0 && mscale.Location <= actualLength)
                            items.Add(mscale);
                    }
                }
            }
            ItemsSource = items;
        }


        #endregion
    }

    internal class LogicleBiexScaleData : IScaleData
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
