using FCSChart.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FCSChart.Graphical
{
    /// <summary>
    /// 多边形--多个点连线形成的形状
    /// </summary>
    public class PolygonGraphical : BaseGraphical
    {
        public PolygonGraphical() : base() { }
        protected override void CreateNewMode()
        {
            GraphicalModel = new PolygonGraphicalModel()
            {
                Points = new ObservableCollection<Point>() { new Point(0, 0) },
                Name = CreateNewGraphicalNameFaction?.Invoke(this)
            };
        }
        public PolygonGraphical(PolygonGraphicalModel model) : base(model)
        {

        }

        #region function
        /// <summary>
        /// 绘制图形
        /// </summary>
        public override void Drawing()
        {
            if (OwnerChart == null || !OwnerChart.IsLoaded
                || OwnerChart.XAxis == null || !OwnerChart.XAxis.IsLoaded || OwnerChart.XAxis.ActualWidth == 0
                || OwnerChart.YAxis == null || !OwnerChart.YAxis.IsLoaded || OwnerChart.XAxis.ActualHeight == 0) return;
            if (GraphicalShape == null)
            {
                Binding binding = new Binding("Points") { Source = this.GraphicalModel, Converter = GraphicalPointsConverter, ConverterParameter = OwnerChart, Mode = BindingMode.TwoWay };
                var temp = new Polygon() { FillRule = FillRule.Nonzero, Cursor = Cursors.Hand };
                temp.SetBinding(Polygon.PointsProperty, binding);
                GraphicalShape = temp;
            }
            GraphicalModel.OnPropertyChanged("Points");
        }

        #region panel event--用户门创建
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal override void PanelMouseDown(object sender, MouseButtonEventArgs e) { }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal override void PanelMouseMove(object sender, MouseEventArgs e)
        {
            if (IsCreateing && sender is Panel panel && GraphicalModel is PolygonGraphicalModel model)
            {
                var point = e.GetPosition(panel);
                var x = OwnerChart.XAxis.GetLocationValue(point.X);
                var y = OwnerChart.YAxis.GetLocationValue(point.Y);
                model.Points[0] = new Point(x, y);
                Drawing();
            }
        }
        /// <summary>
        /// 鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal override void PanelMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsCreateing && sender is Panel panel && GraphicalModel is PolygonGraphicalModel model)
            {
                var point = e.GetPosition(panel);
                var x = OwnerChart.XAxis.GetLocationValue(point.X);
                var y = OwnerChart.YAxis.GetLocationValue(point.Y);
                var tempPoint = new Point(x, y);
                if (model.Points.Count >= 4 && model.Points[0].Equals(model.Points[model.Points.Count - 1]))
                {
                    model.Points.RemoveAt(0);
                    IsCreateing = false;
                }
                else model.Points.Add(tempPoint);
            }
        }

        #endregion

        /// <summary>
        /// 移动门
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal override void Move(double x, double y)
        {
            if (GraphicalModel is PolygonGraphicalModel model)
            {
                ObservableCollection<Point> templist = new ObservableCollection<Point>();
                foreach (var point in model.Points)
                {
                    var xtemp = OwnerChart.XAxis.GetLocationValue(OwnerChart.XAxis.GetValueLocation(point.X) + x);
                    var ytemp = OwnerChart.YAxis.GetLocationValue(OwnerChart.YAxis.GetValueLocation(point.Y) + y);
                    templist.Add(new Point(xtemp, ytemp));
                }
                model.Points = templist;
            }
        }
        #endregion

        /// <summary>
        /// 绘制控制按钮
        /// </summary>
        protected override void DrawingControl()
        {
            if (GraphicalShape is Polygon graphical)
            {
                var points = graphical.Points;
                var shapes = ControlShapes;
                if (shapes.Count > points.Count)
                    for (int i = shapes.Count - 1; i >= points.Count; i--)
                        shapes.RemoveAt(i);
                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    if (shapes.Count > i)
                    {
                        if (shapes[i] is Path path && path.Data is EllipseGeometry ellipse)
                            ellipse.Center = point;
                    }
                    else
                        shapes.Add(new Path() { Data = new EllipseGeometry(point, 5, 5), Cursor = Cursors.SizeAll });
                }
            }
        }

        protected override void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Path path && path.Data is EllipseGeometry ellipse && GraphicalModel is PolygonGraphicalModel model)
            {
                var point = e.GetPosition(OwnerChart.ViewPanel);
                var index = ControlShapes.IndexOf(path);
                var value = GraphicalPointConverter.ConvertBack(point, null, OwnerChart, null);
                if (value is Point p)
                {
                    model.Points[index] = p;
                    Drawing();
                }
                ellipse.Center = point;
                e.Handled = true;
            }
        }

        protected override void MonitorControlData(BaseGraphicalModel model)
        {
            if (model is PolygonGraphicalModel polygonModel)
            {
                polygonModel.PropertyChanged += (sender, e) =>
                {
                    if ("Points".Equals(e.PropertyName) && this.GraphicalShape.IsFocused) DrawingControl();
                };
                polygonModel.Points.CollectionChanged += (sender, e) =>
                {
                    if (this.GraphicalShape.IsFocused)
                        switch (e.Action)
                        {
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                                DrawingControl();
                                break;
                            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                                break;
                            default:
                                break;
                        }
                };
            }
        }

        public override void RefreshSubset()
        {
            if (!IsCreateing && GraphicalModel is PolygonGraphicalModel model && OwnerChart != null && OwnerChart.ItemsSource != null)
            {
                List<Point> axisViewPoints = new List<Point>();
                foreach (var point in model.Points)
                    axisViewPoints.Add(new Point(OwnerChart.XAxis.ValueToAxisValue(point.X), OwnerChart.YAxis.ValueToAxisValue(point.Y)));
                var maxX = axisViewPoints.Max(p => p.X);
                var minX = axisViewPoints.Min(p => p.X);
                var maxY = axisViewPoints.Max(p => p.Y);
                var minY = axisViewPoints.Min(p => p.Y);

                var items = OwnerChart.ItemsSource;
                var x = OwnerChart.X;
                var y = OwnerChart.Y;
                Func<double, double> XAxisFunc = OwnerChart.XAxis.ValueToAxisValue;
                Func<double, double> YAxisFunc = OwnerChart.YAxis.ValueToAxisValue;
                int nvert = axisViewPoints.Count;
                List<IList> lists = new List<IList>();
                foreach (var item in items)
                {
                    var tempx = XAxisFunc(Convert.ToDouble(item[x]));
                    var tempy = YAxisFunc(Convert.ToDouble(item[y]));
                    if (tempx < minX || tempx > maxX || tempy < minY || tempy > maxY) continue;
                    int i, j, c = 0;
                    for (i = 0, j = nvert - 1; i < nvert; j = i++)
                    {
                        var pointI = axisViewPoints[i];
                        var pointJ = axisViewPoints[j];
                        if (((pointI.Y > tempy) != (pointJ.Y > tempy)) && (tempx < (pointJ.X - pointI.X) * (tempy - pointI.Y) / (pointJ.Y - pointI.Y) + pointI.X))
                        {
                            c = 1 + c;
                        }
                    }
                    if (c % 2 != 0) lists.Add(item);
                }
                Subset = lists;
            }
        }
    }


    public class PolygonGraphicalModel : BaseGraphicalModel
    {
        private ObservableCollection<Point> points;
        /// <summary>
        /// 多边形的点位置--真实数值数据，需要通过轴的GetLocation获取真实的位置
        /// </summary>
        public ObservableCollection<Point> Points
        {
            get { return points; }
            set { points = value; OnPropertyChanged("Points"); }
        }

    }
}
