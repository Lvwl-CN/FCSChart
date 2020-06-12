using FCSChart.Axis;
using FCSChart.Graphical;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace FCSChart
{
    public abstract class ChartWithGraphicals : Chart
    {
        public ChartWithGraphicals()
        {
            Graphicals = new ObservableCollection<BaseGraphical>();
            this.Loaded += (sender, e) =>
            {
                DrawingGraphicals();
            };
        }


        #region 门        
        /// <summary>
        /// 门的集合
        /// </summary>
        public ObservableCollection<BaseGraphical> Graphicals
        {
            get { return (ObservableCollection<BaseGraphical>)GetValue(GraphicalsProperty); }
            private set { SetValue(GraphicalsProperty, value); }
        }
        public static readonly DependencyProperty GraphicalsProperty = DependencyProperty.Register("Graphicals", typeof(ObservableCollection<BaseGraphical>), typeof(Chart), new PropertyMetadata(Graphicals_Changed));
        #endregion

        #region propertychanged
        protected override void ChangedNeedRedrawing()
        {
            base.ChangedNeedRedrawing();
            DrawingGraphicals();
        }

        /// <summary>
        /// 门的集合改变
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void Graphicals_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChartWithGraphicals temp && e.NewValue is IEnumerable<BaseGraphical> graphicals)
            {
                foreach (var graphical in graphicals)
                {
                    graphical.OwnerChart = temp;
                }
                temp.DrawingGraphicals();
                if (e.OldValue != null && e.OldValue is INotifyCollectionChanged olddata)
                {
                    olddata.CollectionChanged -= temp.Graphicals_CollectionChanged;
                }
                if (e.NewValue != null && e.NewValue is INotifyCollectionChanged newdata)
                {
                    newdata.CollectionChanged += temp.Graphicals_CollectionChanged;
                }
            }
        }
        /// <summary>
        /// 门的集合增减
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Graphicals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems)
                            if (item is BaseGraphical graphical)
                            {
                                graphical.OwnerChart = this;
                                graphical.Drawing();
                            }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region function

        /// <summary>
        /// 尺寸编号时重新绘制图形
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawingGraphicals();
        }
        /// <summary>
        /// 鼠标左键按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var createingGraphical = Graphicals.FirstOrDefault(p => p.IsCreateing == true);
            if (createingGraphical != null)
            {
                createingGraphical.PanelMouseDown(sender, e);
                e.Handled = true;
                return;
            }
            base.Panel_MouseLeftButtonDown(sender, e);
        }
        /// <summary>
        /// 鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Panel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var createingGraphical = Graphicals.FirstOrDefault(p => p.IsCreateing == true);
            if (createingGraphical != null)
            {
                createingGraphical.PanelMouseUp(sender, e);
                e.Handled = true;
                return;
            }
            base.Panel_MouseLeftButtonUp(sender, e);
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            var createingGraphical = Graphicals.FirstOrDefault(p => p.IsCreateing == true);
            if (createingGraphical != null)
            {
                createingGraphical.PanelMouseMove(sender, e);
                e.Handled = true;
                return;
            }
            if (this.IsFocused) base.Panel_MouseMove(sender, e);
        }
        /// <summary>
        /// 移动图形
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="moveType"></param>
        public override void Move(double x, double y, AxisChangeType moveType)
        {
            if (moveType == AxisChangeType.None) return;
            base.Move(x, y, moveType);
            DrawingGraphicals();
        }
        /// <summary>
        /// 缩放图形
        /// </summary>
        /// <param name="point"></param>
        /// <param name="percent"></param>
        /// <param name="zoomType"></param>
        public override void Zoom(Point point, double percent, AxisChangeType zoomType)
        {
            if (zoomType == AxisChangeType.None) return;
            base.Zoom(point, percent, zoomType);
            DrawingGraphicals();
        }
        /// <summary>
        /// 绘制所有门
        /// </summary>
        protected virtual void DrawingGraphicals()
        {
            if (!IsLoaded) return;
            foreach (var graphical in Graphicals)
                graphical.Drawing();
        }

        protected override void DrawingSeries()
        {
            base.DrawingSeries();
            if (Graphicals != null) foreach (var graphical in Graphicals) graphical.RefreshSubset();
        }
        #endregion
    }
}
