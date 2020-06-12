using FCSChart.Axis;
using FCSChart.Graphical;
using FCSChart.Series;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FCSChart
{
    [TemplatePart(Name = "Part_Panel", Type = typeof(Panel))]
    public class Chart : Control
    {
        public Chart()
        {
            this.Loaded += (sender, e) =>
            {
                DrawingSeries();
            };
        }

        #region property
        /// <summary>
        /// 数据源
        /// </summary>
        public IEnumerable<IList> ItemsSource
        {
            get { return (IEnumerable<IList>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IList>), typeof(Chart), new PropertyMetadata(ItemsSource_Changed));

        /// <summary>
        /// 显示数据的图形
        /// </summary>
        public ISeries Series
        {
            get { return (ISeries)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }
        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series", typeof(ISeries), typeof(Chart), new PropertyMetadata((sender, e) => { if (sender is Chart chart && e.NewValue is ISeries series) series.OwnerChart = chart; }));

        #region xy轴类型
        public int X
        {
            get { return (int)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(int), typeof(Chart), new PropertyMetadata(0, (sender, e) => { if (sender is Chart chart && chart.IsLoaded) chart.DrawingSeries(); }));

        public int Y
        {
            get { return (int)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(int), typeof(Chart), new PropertyMetadata(1, (sender, e) => { if (sender is Chart chart && chart.IsLoaded) chart.DrawingSeries(); }));

        /// <summary>
        /// x轴
        /// </summary>
        public IAxis XAxis
        {
            get { return (IAxis)GetValue(XAxisProperty); }
            set { SetValue(XAxisProperty, value); }
        }
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register("XAxis", typeof(IAxis), typeof(Chart), new PropertyMetadata(XAxis_Changed));
        private static void XAxis_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Chart chart)
            {
                if (e.NewValue is IAxis axis)
                {
                    axis.XYType = AxisType.X;
                    axis.Loaded += chart.ChangedNeedRedrawing;
                }
                if (e.OldValue is IAxis oldAxis) oldAxis.Loaded -= chart.ChangedNeedRedrawing;
            }
        }


        /// <summary>
        /// y轴
        /// </summary>
        public IAxis YAxis
        {
            get { return (IAxis)GetValue(YAxisProperty); }
            set { SetValue(YAxisProperty, value); }
        }
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register("YAxis", typeof(IAxis), typeof(Chart), new PropertyMetadata(YAxis_Changed));
        private static void YAxis_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Chart chart)
            {
                if (e.NewValue is IAxis axis)
                {
                    axis.XYType = AxisType.Y;
                    axis.Loaded += chart.ChangedNeedRedrawing;
                }
                if (e.OldValue is IAxis oldAxis) oldAxis.Loaded -= chart.ChangedNeedRedrawing;
            }
        }

        private void ChangedNeedRedrawing(object sender, EventArgs e)
        {
            ChangedNeedRedrawing();
        }
        /// <summary>
        /// 属性改变需要重新绘制
        /// </summary>
        protected virtual void ChangedNeedRedrawing()
        {
            DrawingSeries();
        }
        #endregion

        #region 变形
        /// <summary>
        /// 缩放方式
        /// </summary>
        public AxisChangeType ZoomType
        {
            get { return (AxisChangeType)GetValue(ZoomTypeProperty); }
            set { SetValue(ZoomTypeProperty, value); }
        }
        public static readonly DependencyProperty ZoomTypeProperty = DependencyProperty.Register("ZoomType", typeof(AxisChangeType), typeof(Chart), new PropertyMetadata(AxisChangeType.None));

        /// <summary>
        /// 移动方式
        /// </summary>
        public AxisChangeType MoveType
        {
            get { return (AxisChangeType)GetValue(MoveTypeProperty); }
            set { SetValue(MoveTypeProperty, value); }
        }
        public static readonly DependencyProperty MoveTypeProperty = DependencyProperty.Register("MoveType", typeof(AxisChangeType), typeof(Chart), new PropertyMetadata(AxisChangeType.None));
        #endregion

        #endregion

        #region 重载
        /// <summary>
        /// 加载模板
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.MouseDown += (sender, e) => { this.Focus(); };
            var temp = GetTemplateChild("Part_Panel");
            if (temp != null)
            {
                ViewPanel = temp as Panel;
                ViewPanel.MouseLeftButtonDown += Panel_MouseLeftButtonDown;
                ViewPanel.MouseLeftButtonUp += Panel_MouseLeftButtonUp;
                ViewPanel.MouseMove += Panel_MouseMove;
                ViewPanel.MouseWheel += Panel_MouseWheel;
                ViewPanel.KeyDown += Panel_KeyDown;
                ViewPanel.KeyUp += Panel_KeyUp;
            }
        }

        /// <summary>
        /// 尺寸编号时重新绘制图形
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            XAxis?.Drawing();
            YAxis?.Drawing();
            DrawingSeries();
        }
        #endregion

        #region property changed
        /// <summary>
        /// 数据源改变
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void ItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Chart temp)
            {
                temp.DrawingSeries();
                if (e.OldValue != null && e.OldValue is INotifyCollectionChanged olddata)
                {
                    olddata.CollectionChanged -= temp.ItemsSource_CollectionChanged;
                }
                if (e.NewValue != null && e.NewValue is INotifyCollectionChanged newdata)
                {
                    newdata.CollectionChanged += temp.ItemsSource_CollectionChanged;
                }
            }
        }
        /// <summary>
        /// 数据源增减
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DrawingSeries();
        }
        #endregion

        #region 鼠标控制事件
        public Panel ViewPanel { get; protected set; }
        protected Point moveStartPoint;
        bool isMoved = false;
        /// <summary>
        /// 鼠标左键按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            moveStartPoint = e.GetPosition(ViewPanel);
        }
        /// <summary>
        /// 鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Panel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMoved)
            {
                DrawingSeries();
                isMoved = false;
            }
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (moveStartPoint != null && moveStartPoint != default)
                {
                    var point = e.GetPosition(ViewPanel);
                    var x = point.X - moveStartPoint.X;
                    var y = point.Y - moveStartPoint.Y;
                    moveStartPoint = point;
                    Move(x, y, MoveType);
                    isMoved = true;
                }
            }
        }
        /// <summary>
        /// 鼠标滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Panel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var point = e.GetPosition(ViewPanel);
            var percent = e.Delta > 0 ? 1.1d : 0.9d;
            Zoom(point, percent, ZoomType);
        }
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Panel_KeyDown(object sender, KeyEventArgs e)
        {
        }
        /// <summary>
        /// 键盘抬起事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Panel_KeyUp(object sender, KeyEventArgs e)
        {
        }
        #endregion

        #region function
        /// <summary>
        /// 移动图形
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="moveType"></param>
        public virtual void Move(double x, double y, AxisChangeType moveType)
        {
            if (moveType == AxisChangeType.None) return;
            switch (moveType)
            {
                case AxisChangeType.None:
                    break;
                case AxisChangeType.X:
                    XAxis?.Move(x, 0);
                    y = 0;
                    break;
                case AxisChangeType.Y:
                    YAxis?.Move(0, y);
                    x = 0;
                    break;
                case AxisChangeType.XY:
                    XAxis?.Move(x, y);
                    YAxis?.Move(x, y);
                    break;
                default:
                    break;
            }
            Series?.Move(x, y);
        }
        /// <summary>
        /// 缩放图形
        /// </summary>
        /// <param name="point"></param>
        /// <param name="percent"></param>
        /// <param name="zoomType"></param>
        public virtual void Zoom(Point point, double percent, AxisChangeType zoomType)
        {
            if (zoomType == AxisChangeType.None) return;
            switch (zoomType)
            {
                case AxisChangeType.None:
                    break;
                case AxisChangeType.X:
                    XAxis?.Zoom(percent, point);
                    break;
                case AxisChangeType.Y:
                    YAxis?.Zoom(percent, point);
                    break;
                case AxisChangeType.XY:
                    XAxis?.Zoom(percent, point);
                    YAxis?.Zoom(percent, point);
                    break;
                default:
                    break;
            }
            DrawingSeries();
        }
        /// <summary>
        /// 重新绘制图形
        /// </summary>
        protected virtual void DrawingSeries()
        {
            if (!IsLoaded || Series == null) return;
            Series.Drawing();
        }

        #endregion
    }
}
