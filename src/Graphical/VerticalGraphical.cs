using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FCSChart.Graphical
{
    public class VerticalGraphical : BaseGraphical
    {
        public VerticalGraphical() : base() { }
        protected override void CreateNewMode()
        {
            GraphicalModel = new VerticalGraphicalModel
            {
                Name = CreateNewGraphicalNameFaction?.Invoke(this)
            };
        }
        public VerticalGraphical(VerticalGraphicalModel model) : base(model) { }

        #region 容器事件--绘制图形时使用
        public override void PanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCreateing && sender is Panel panel && GraphicalModel is VerticalGraphicalModel model)
            {
                var point = e.GetPosition(panel);
                var x = OwnerChart.XAxis.GetLocationValue(point.X);
                model.X1 = model.X2 = x;
            }
        }

        public override void PanelMouseMove(object sender, MouseEventArgs e)
        {
            if (IsCreateing && sender is Panel panel && GraphicalModel is VerticalGraphicalModel model && model.X1 != 0 && model.X2 != 0)
            {
                var point = e.GetPosition(panel);
                var x = OwnerChart.XAxis.GetLocationValue(point.X);
                model.X2 = x;
                Drawing();
            }
        }

        public override void PanelMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsCreateing && GraphicalModel is VerticalGraphicalModel model)
            {
                if (model.X1 == model.X2) return;
                Drawing();
                IsCreateing = false;
            }
        }
        #endregion

        #region 图形移动变形

        protected static readonly VerticalGraphicalModelToGeometryConverter VerticalModelToGeometryConverter = new VerticalGraphicalModelToGeometryConverter();
        public override void Drawing()
        {
            if (OwnerChart == null || !OwnerChart.IsLoaded
                || OwnerChart.XAxis == null || !OwnerChart.XAxis.IsLoaded || OwnerChart.XAxis.ActualWidth == 0) return;
            if (GraphicalShape == null)
            {
                var temp = new Path() { Cursor = Cursors.Hand };
                temp.SetBinding(Path.DataProperty, new Binding("GraphicalModel") { Source = this, Converter = VerticalModelToGeometryConverter, ConverterParameter = this.OwnerChart, Mode = BindingMode.OneWay });
                GraphicalShape = temp;
            }
            this.OnPropertyChanged("GraphicalModel");
        }
        public override void Move(double x, double y)
        {
            if (GraphicalModel is VerticalGraphicalModel model && x != 0)
            {
                model.X1 = OwnerChart.XAxis.GetLocationValue(OwnerChart.XAxis.GetValueLocation(model.X1) + x);
                model.X2 = OwnerChart.XAxis.GetLocationValue(OwnerChart.XAxis.GetValueLocation(model.X2) + x);
                Drawing();
            }
        }

        #endregion


        #region 门控制相关
        protected override void DrawingControl()
        {
            if (GraphicalShape is Path graphical && graphical.Data is PathGeometry geometry && geometry.Figures.Count == 1 && geometry.Figures[0].Segments.Count == 4)
            {
                var shapes = ControlShapes;
                for (int i = 0; i < 2; i++)
                {
                    var segment = geometry.Figures[0].Segments[i] as LineSegment;
                    var point = new Point(segment.Point.X, segment.Point.Y / 2);
                    if (shapes.Count > i)
                    {
                        if (shapes[i] is Path path && path.Data is EllipseGeometry ellipse)
                            ellipse.Center = point;
                    }
                    else
                        shapes.Add(new Path() { Data = new EllipseGeometry(point, 5, 30), Cursor = Cursors.SizeWE });
                }
            }
        }

        protected override void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Path path && path.Data is EllipseGeometry ellipse && GraphicalModel is VerticalGraphicalModel model)
            {
                var point = e.GetPosition(OwnerChart.ViewPanel);
                var index = ControlShapes.IndexOf(path);
                var value = OwnerChart.XAxis.GetLocationValue(point.X);
                if (index == 0) model.X1 = value;
                else model.X2 = value;
                Drawing();
                e.Handled = true;
            }
        }
        protected override void MonitorControlData(BaseGraphicalModel model)
        {
            if (model is VerticalGraphicalModel verticalModel)
            {
                verticalModel.PropertyChanged += (sender, e) =>
                {
                    if (this.GraphicalShape.IsFocused && ("X1".Equals(e.PropertyName) || "X2".Equals(e.PropertyName))) DrawingControl();
                };
            }
            this.PropertyChanged += (sender, e) =>
            {
                if (this.GraphicalShape.IsFocused && "GraphicalModel".Equals(e.PropertyName)) DrawingControl();
            };
        }
        #endregion

        public override void RefreshSubset()
        {
            if (!IsCreateing && GraphicalModel is VerticalGraphicalModel model && OwnerChart != null && OwnerChart.ItemsSource != null)
            {
                var min = Math.Min(model.X1, model.X2);
                var max = Math.Max(model.X1, model.X2);
                var index = OwnerChart.X;
                var items = OwnerChart.ItemsSource;
                Subset = items.Where(p => Convert.ToDouble(p[index]) >= min && Convert.ToDouble(p[index]) <= max).ToList();
            }
        }

    }

    public class VerticalGraphicalModel : BaseGraphicalModel
    {
        private double x1;

        public double X1
        {
            get { return x1; }
            set { x1 = value; OnPropertyChanged("X1"); }
        }

        private double x2;

        public double X2
        {
            get { return x2; }
            set { x2 = value; OnPropertyChanged("X2"); }
        }


    }

    /// <summary>
    /// 竖型门模型数据转化为图形
    /// </summary>
    public class VerticalGraphicalModelToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VerticalGraphicalModel model && parameter is Chart chart)
            {
                var length = chart.YAxis.ActualHeight;
                var x1 = chart.XAxis.GetValueLocation(model.X1);
                var x2 = chart.XAxis.GetValueLocation(model.X2);
                PathFigure figure = new PathFigure()
                {
                    StartPoint = new Point(x1, 0),
                    Segments = new PathSegmentCollection()
                    {
                        new LineSegment(new Point(x1, length), true),
                        new LineSegment(new Point(x2, length), false),
                        new LineSegment(new Point(x2, 0), true),
                        new LineSegment(new Point(x1, 0), false)
                    }
                };
                PathGeometry geometry = new PathGeometry() { FillRule = FillRule.Nonzero, Figures = new PathFigureCollection() { figure } };
                geometry.Freeze();
                return geometry;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
