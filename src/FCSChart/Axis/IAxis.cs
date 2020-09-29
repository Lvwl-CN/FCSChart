using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Diagnostics;

namespace FCSChart.Axis
{
    /// <summary>
    /// 轴分割方式
    /// </summary>
    public abstract class IAxis : ItemsControl
    {
        #region property
        public string AxisName { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(IAxis), new PropertyMetadata((sender, e) => { if (sender is IAxis axis && e.NewValue is double d) axis.MaxAxis = axis.ValueToAxisValue(d); }));

        /// <summary>
        /// 最小值
        /// </summary>
        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(IAxis), new PropertyMetadata((sender, e) => { if (sender is IAxis axis && e.NewValue is double d) axis.MinAxis = axis.ValueToAxisValue(d); }));

        /// <summary>
        /// 坐标轴的最大值-转化坐标后
        /// </summary>
        public double MaxAxis { get; protected set; }
        /// <summary>
        /// 坐标轴的最小值-转化坐标后
        /// </summary>
        public double MinAxis { get; protected set; }

        /// <summary>
        /// 用于x或y轴分割
        /// </summary>
        internal AxisType XYType
        {
            get { return (AxisType)GetValue(XYTypeProperty); }
            set { SetValue(XYTypeProperty, value); }
        }
        internal static readonly DependencyProperty XYTypeProperty = DependencyProperty.Register("XYType", typeof(AxisType), typeof(IAxis), new PropertyMetadata(AxisType.X));
        #endregion

        public IAxis()
        {
            MaxAxis = ValueToAxisValue(Max);
            MinAxis = ValueToAxisValue(Min);
            this.Loaded += (sender, e) =>
            {
                Drawing();
            };
        }

        #region function
        /// <summary>
        /// 实际值转换成坐标值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract double ValueToAxisValue(double value);
        /// <summary>
        /// 坐标值转换成实际值
        /// </summary>
        /// <param name="axisvalue"></param>
        /// <returns></returns>
        public abstract double AxisValueToValue(double axisvalue);
        /// <summary>
        /// 绘制数据分隔线
        /// </summary>
        /// <returns></returns>
        internal abstract void Drawing();


        /// <summary>
        /// 鼠标点位的坐标值
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual double PointAxisValue(Point point)
        {
            double length = 1d;
            double pointLocation = 0d;
            switch (XYType)
            {
                case AxisType.X:
                    length = this.ActualWidth;
                    pointLocation = point.X;
                    break;
                case AxisType.Y:
                    length = this.ActualHeight;
                    pointLocation = length - point.Y;//y轴是反向的
                    break;
                default:
                    break;
            }
            return pointLocation * (MaxAxis - MinAxis) / length + MinAxis;
        }
        /// <summary>
        /// 鼠标点位的实际值
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual double PointValue(Point point)
        {
            return AxisValueToValue(PointAxisValue(point));
        }

        #region 点位图形值，实际数值，实际坐标值相互转换
        /// <summary>
        /// 获取实际值转化成坐标值的参数，用于GetLocation()
        /// </summary>
        /// <returns></returns>
        public virtual ValueLocationConvertParam GetConvertParam()
        {
            return new ValueLocationConvertParam()
            {
                MaxAxis = MaxAxis,
                MinAxis = MinAxis,
                Length = XYType == AxisType.X ? this.ActualWidth : this.ActualHeight,
                XYType = XYType
            };
        }
        /// <summary>
        /// 获取数字对应的位置
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual double GetValueLocation(double value)
        {
            if (!IsLoaded) return default;
            return GetValueLocation(value, this.GetConvertParam());
        }
        /// <summary>
        /// 获取数据对应的坐标位置
        /// </summary>
        /// <param name="value">数据值</param>
        /// <param name="length">控件长度</param>
        /// <param name="max">控件当前显示的最大值</param>
        /// <param name="min">控件当前显示的最小值</param>
        /// <returns></returns>
        public virtual double GetValueLocation(double value, ValueLocationConvertParam convert)
        {
            value = ValueToAxisValue(value);
            return GetAxisValueLocation(value, convert);
        }
        /// <summary>
        /// 获取数字对应的位置
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual double GetAxisValueLocation(double value)
        {
            if (!IsLoaded) return default;
            return GetAxisValueLocation(value, this.GetConvertParam());
        }
        /// <summary>
        /// 获取数据对应的坐标位置
        /// </summary>
        /// <param name="value">数据值--xy轴坐标上的值</param>
        /// <param name="length">控件长度</param>
        /// <param name="max">控件当前显示的最大值</param>
        /// <param name="min">控件当前显示的最小值</param>
        /// <returns></returns>
        public virtual double GetAxisValueLocation(double value, ValueLocationConvertParam convert)
        {
            var temp = (value - convert.MinAxis) * convert.Length / (convert.MaxAxis - convert.MinAxis);
            return convert.XYType == AxisType.X ? temp : convert.Length - temp;
        }
        /// <summary>
        /// 获取点位对应的坐标值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double GetLocationAxisValue(double value)
        {
            return GetLocationAxisValue(value, GetConvertParam());
        }
        /// <summary>
        /// 获取点位对应的坐标值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public virtual double GetLocationAxisValue(double value, ValueLocationConvertParam convert)
        {
            return (convert.XYType == AxisType.X ? value : convert.Length - value) * (convert.MaxAxis - convert.MinAxis) / convert.Length + convert.MinAxis;
        }
        /// <summary>
        /// 获取点位对应的实际值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double GetLocationValue(double value)
        {
            return GetLocationValue(value, GetConvertParam());
        }
        /// <summary>
        /// 获取点位对应的实际值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public virtual double GetLocationValue(double value, ValueLocationConvertParam convert)
        {
            return AxisValueToValue(GetLocationAxisValue(value, convert));
        }
        #endregion
        /// <summary>
        /// 获取直方图数据
        /// </summary>
        /// <param name="items">数据集</param>
        /// <param name="index">第几个参数</param>
        /// <param name="count">从最大值到最小值切分多少份</param>
        /// <returns></returns>
        internal virtual Task<Dictionary<double[], double>> GetHistogramViewData(IEnumerable<IList> items, int index, int count)
        {
            var maxaxis = MaxAxis;
            var minaxis = MinAxis;
            var max = Max;
            var min = Min;
            var range = (maxaxis - minaxis) / count;
            var convert = this.GetConvertParam();
            return Task.Run(() =>
            {
                var begin = 0;
                var end = count;
                Dictionary<double[], double> datas = new Dictionary<double[], double>();
                Dictionary<int, double[]> dictionaries = new Dictionary<int, double[]>();
                for (int i = begin; i <= end; i++)
                {
                    var x1 = GetAxisValueLocation(range * i + minaxis, convert);
                    var x2 = GetAxisValueLocation(range * (i + 1) + minaxis, convert);
                    var key = new double[] { x1, x2 };
                    datas.Add(key, 0);
                    dictionaries.Add(i, key);
                }
                foreach (var item in items)
                {
                    var tempv = Convert.ToDouble(item[index]);
                    if (tempv > max || tempv < min) continue;
                    var value = (ValueToAxisValue(tempv) - minaxis) / range;
                    if (double.IsNaN(value) || double.IsInfinity(value)) continue;
                    var temp = value >= 0 ? (int)value : (int)value - 1;
                    datas[dictionaries[temp]] += 1;
                }
                return datas;
            });
        }
        /// <summary>
        /// 移动图形
        /// </summary>
        /// <param name="x">x轴移动距离</param>
        /// <param name="y">y轴移动距离</param>
        internal virtual void Move(double x, double y)
        {
            if (!IsLoaded) return;
            double controlActualLength = 1;
            double movelength = 0d;
            switch (XYType)
            {
                case AxisType.X:
                    controlActualLength = this.ActualWidth;
                    movelength = x;
                    break;
                case AxisType.Y:
                    controlActualLength = this.ActualHeight;
                    movelength = -y;//纵坐标的方向在图形显示和轴上是反向的
                    break;
                default:
                    break;
            }
            var temp = (MaxAxis - MinAxis) * movelength / controlActualLength;
            Max = AxisValueToValue(MaxAxis - temp);
            Min = AxisValueToValue(MinAxis - temp);
            Drawing();
        }
        /// <summary>
        /// 放大缩小
        /// </summary>
        /// <param name="value"></param>
        /// <param name="point"></param>
        internal virtual void Zoom(double percent, Point point)
        {
            if (!IsLoaded) return;
            double controlLength = 1;
            double pv = 0d;
            switch (XYType)
            {
                case AxisType.X:
                    controlLength = this.ActualWidth;
                    pv = point.X;
                    break;
                case AxisType.Y:
                    controlLength = this.ActualHeight;
                    pv = this.ActualHeight - point.Y;
                    break;
                default:
                    break;
            }
            percent = 1 / percent;
            var v = (MaxAxis - MinAxis) * (pv / controlLength) + MinAxis;
            Min = AxisValueToValue(v - (v - MinAxis) * percent);
            Max = AxisValueToValue(v + (MaxAxis - v) * percent);
            Drawing();
        }
        #endregion

    }

    internal abstract class IScaleData : NotifyPropertyChanged
    {
        private object _value;
        /// <summary>
        /// 名称
        /// </summary>
        public object Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }

        private int fontSize = 12;
        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; OnPropertyChanged("FontSize"); }
        }

        private double location;
        /// <summary>
        /// 坐标点所在位置的百分比
        /// </summary>
        public double Location
        {
            get { return location; }
            set { location = value; OnPropertyChanged("Location"); }
        }

        private int length;
        /// <summary>
        /// 分隔线长度
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; OnPropertyChanged("Length"); }
        }

        public IScaleData()
        {
            this.Length = 10;
        }
    }


    public struct ValueLocationConvertParam
    {
        public double MaxAxis { get; set; }
        public double MinAxis { get; set; }
        public double Length { get; set; }
        public AxisType XYType { get; set; }

    }
}
